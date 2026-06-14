using System.Reflection;
using System.Text.Json;

// Usage: dotnet run <TypeName> [<TypeName2> ...]
// Searches all Pantheon Il2Cpp interop assemblies for the given type(s) and dumps a member tree.
// Game install path is auto-detected; override with PANTHEON_DIR env var.
// Add --depth=N to control recursion depth (default: 2). Use --depth=0 for flat dump.
// Add --refs to show all types that reference this type in fields, properties, or method signatures.
// Add --html[=output.html] to generate a browsable HTML type browser (default: types.html).

Console.OutputEncoding = System.Text.Encoding.UTF8;

if (args.Length == 0)
{
    Console.Error.WriteLine("Usage: dotnet run <TypeName> [<TypeName2> ...]");
    Console.Error.WriteLine("  e.g. dotnet run EntityNpcGameObject");
    Console.Error.WriteLine("  e.g. dotnet run EntityNpcGameObject --depth=3");
    Console.Error.WriteLine("  e.g. dotnet run --html");
    Console.Error.WriteLine("  e.g. dotnet run --html=browser.html");
    return 1;
}

var maxDepth = 2;
var showRefs = false;
var showHtml = false;
var htmlPath = "types.html";
var typeArgs = args.Where(a => {
    if (a.StartsWith("--depth=") && int.TryParse(a["--depth=".Length..], out var d)) { maxDepth = d; return false; }
    if (a == "--refs") { showRefs = true; return false; }
    if (a == "--html") { showHtml = true; return false; }
    if (a.StartsWith("--html=")) { showHtml = true; htmlPath = a["--html=".Length..]; return false; }
    return true;
}).ToArray();

var gameDir = Environment.GetEnvironmentVariable("PANTHEON_DIR")
    ?? DetectGameDir()
    ?? throw new Exception("Could not find Pantheon install. Set PANTHEON_DIR env var.");

var il2cppDir = Path.Combine(gameDir, "MelonLoader", "Il2CppAssemblies");
var net6Dir   = Path.Combine(gameDir, "MelonLoader", "net6");
var runtimeDir = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory();

Console.WriteLine($"Loading assemblies from: {il2cppDir}");

var allDlls = Directory.GetFiles(il2cppDir, "*.dll")
    .Concat(Directory.GetFiles(net6Dir, "*.dll"))
    .Concat(Directory.GetFiles(runtimeDir, "*.dll"))
    .ToArray();

var resolver = new PathAssemblyResolver(allDlls);
using var mlc = new MetadataLoadContext(resolver);

// Scan all game DLLs: Assembly-CSharp plus any Il2Cpp* that aren't pure infrastructure
var gameAssemblyPaths = Directory.GetFiles(il2cppDir, "*.dll")
    .Where(path => {
        var name = Path.GetFileNameWithoutExtension(path);
        if (name == "Assembly-CSharp") return true;
        if (!name.StartsWith("Il2Cpp")) return false;
        // Exclude runtime infrastructure
        if (name.StartsWith("Il2CppSystem") || name.StartsWith("Il2CppMono") ||
            name.StartsWith("Il2CppInterop") || name.StartsWith("Il2CppMicrosoft") ||
            name.StartsWith("Il2CppNewtonsoft") || name.StartsWith("Il2CppTMPro") ||
            name.StartsWith("Il2CppUnityEngine") || name.StartsWith("Il2CppUnity.") ||
            name.StartsWith("Il2CppUnityStandard"))
            return false;
        return true;
    })
    .ToArray();

var allTypes = gameAssemblyPaths
    .SelectMany(path => {
        try { return mlc.LoadFromAssemblyPath(path).GetTypes(); }
        catch { return Array.Empty<Type>(); }
    })
    .GroupBy(t => t.FullName ?? t.Name)
    .ToDictionary(g => g.Key, g => g.First());

Console.WriteLine($"Indexed {allTypes.Count} types across game assemblies.\n");

if (showHtml)
{
    GenerateHtml(htmlPath);
    return 0;
}

if (typeArgs.Length == 0)
{
    Console.Error.WriteLine("No type names specified. Use --html to generate the type browser.");
    return 1;
}

foreach (var typeName in typeArgs)
{
    var matches = allTypes.Values
        .Where(t => t.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase))
        .ToArray();

    if (matches.Length == 0)
    {
        matches = allTypes.Values
            .Where(t => t.Name.Contains(typeName, StringComparison.OrdinalIgnoreCase))
            .ToArray();
        if (matches.Length == 0) { Console.WriteLine($"{typeName}: NOT FOUND\n"); continue; }
        Console.WriteLine($"Exact match not found for '{typeName}'. Partial matches:");
        foreach (var m in matches) Console.WriteLine($"  {m.FullName}");
        Console.WriteLine();
        continue;
    }

    foreach (var type in matches)
    {
        PrintTree(type, "", true, 0, new HashSet<string>());
        Console.WriteLine();

        if (showRefs)
            PrintRefs(type);
    }
}

return 0;

// ── HTML generation ────────────────────────────────────────────────────────

void GenerateHtml(string outputPath)
{
    Console.Error.Write("Building type data... ");
    var entries = allTypes.Values
        .Where(t => !IsGeneratedType(t))
        .OrderBy(t => t.FullName)
        .Select(BuildTypeData)
        .ToArray();
    Console.Error.WriteLine($"{entries.Length} types");

    var json = JsonSerializer.Serialize(entries);
    var html = HtmlTemplate().Replace("/*DATA*/", json);
    File.WriteAllText(outputPath, html);
    Console.WriteLine($"Written: {Path.GetFullPath(outputPath)}");
}

object BuildTypeData(Type t)
{
    var kind = t.IsEnum ? "enum" : t.IsInterface ? "interface" : t.IsValueType ? "struct" : "class";
    var members = new List<object>();

    if (t.IsEnum)
    {
        foreach (var f in t.GetFields(BindingFlags.Public | BindingFlags.Static)
            .OrderBy(f => { try { return Convert.ToInt64(f.GetRawConstantValue()); } catch { return 0L; } }))
        {
            try { members.Add(new { k = "v", n = f.Name, v = f.GetRawConstantValue() }); }
            catch { members.Add(new { k = "v", n = f.Name, v = (object)0 }); }
        }
    }
    else
    {
        var flags = BindingFlags.Public | BindingFlags.Instance;
        var baseMethodNames = new HashSet<string>(
            CollectBaseTypes(t).SelectMany(b => { try { return b.GetMethods(flags).Select(m => m.Name); } catch { return Array.Empty<string>(); } }));

        try
        {
            foreach (var f in t.GetFields(flags)
                .Where(f => !f.IsSpecialName && !f.Name.EndsWith("k__BackingField") && !IsNoisyMember(f.Name))
                .OrderBy(f => f.Name))
            {
                members.Add(new { k = "f", t = FriendlyTypeName(f.FieldType), tf = f.FieldType.FullName ?? "", n = f.Name });
            }
        }
        catch { }

        try
        {
            foreach (var p in t.GetProperties(flags)
                .Where(p => !p.Name.EndsWith("k__BackingField") && !IsNoisyMember(p.Name))
                .OrderBy(p => p.Name))
            {
                members.Add(new { k = "p", t = FriendlyTypeName(p.PropertyType), tf = p.PropertyType.FullName ?? "", n = p.Name });
            }
        }
        catch { }

        try
        {
            foreach (var m in t.GetMethods(flags)
                .Where(m => !m.IsSpecialName && !baseMethodNames.Contains(m.Name) && !IsNoisyMember(m.Name))
                .OrderBy(m => m.Name))
            {
                var ps = m.GetParameters().Select(p => new { t = FriendlyTypeName(p.ParameterType), tf = p.ParameterType.FullName ?? "", n = p.Name }).ToArray();
                members.Add(new { k = "m", r = FriendlyTypeName(m.ReturnType), rf = m.ReturnType.FullName ?? "", n = m.Name, ps });
            }
        }
        catch { }
    }

    var displayName = t.DeclaringType != null ? $"{t.DeclaringType.Name}.{t.Name}" : t.Name;

    return new
    {
        n  = displayName,
        fn = t.FullName ?? t.Name,
        ns = t.Namespace ?? "",
        b  = t.BaseType?.Name ?? "",
        bf = t.BaseType?.FullName ?? "",
        k  = kind,
        m  = members.ToArray()
    };
}

string HtmlTemplate() => """
<!DOCTYPE html>
<html lang="en">
<head>
<meta charset="UTF-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>Pantheon Type Browser</title>
<style>
*{box-sizing:border-box;margin:0;padding:0}
body{font-family:'Segoe UI',system-ui,sans-serif;background:#1e1e2e;color:#cdd6f4;height:100vh;display:flex;flex-direction:column;overflow:hidden}
#hdr{padding:10px 14px;background:#181825;border-bottom:1px solid #313244;display:flex;align-items:center;gap:10px;flex-shrink:0}
#hdr h1{font-size:15px;font-weight:700;color:#cba6f7;white-space:nowrap}
#search{flex:1;padding:6px 11px;background:#313244;border:1px solid #45475a;border-radius:6px;color:#cdd6f4;font-size:13px;outline:none}
#search:focus{border-color:#cba6f7}
#stats{font-size:12px;color:#6c7086;white-space:nowrap}
#list{flex:1;overflow-y:auto;padding:6px 8px}
.ti{border-radius:6px;margin-bottom:2px;border:1px solid transparent}
.ti:hover{border-color:#313244}
.ti.open{border-color:#45475a;background:#181825}
.ti.flash{outline:2px solid #cba6f7;outline-offset:-2px}
.th{display:flex;align-items:center;gap:8px;padding:5px 10px;cursor:pointer;user-select:none}
.th:hover .tn{color:#cba6f7}
.badge{font-size:10px;font-weight:700;padding:1px 5px;border-radius:3px;text-transform:uppercase;letter-spacing:.05em;flex-shrink:0}
.badge.enum{background:#f38ba8;color:#1e1e2e}
.badge.class{background:#89b4fa;color:#1e1e2e}
.badge.interface{background:#cba6f7;color:#1e1e2e}
.badge.struct{background:#a6e3a1;color:#1e1e2e}
.tn{font-weight:600;font-size:13px}
.tns{font-size:11px;color:#6c7086;margin-left:2px}
.tbase{font-size:11px;color:#45475a}
.arr{margin-left:auto;color:#45475a;font-size:11px;transition:transform .15s}
.open .arr{transform:rotate(90deg)}
.tb{padding:2px 10px 10px 36px}
.section-label{font-size:10px;text-transform:uppercase;letter-spacing:.08em;color:#45475a;margin:8px 0 3px}
table{width:100%;border-collapse:collapse;font-family:'Cascadia Code','Fira Code',monospace;font-size:12px}
td{padding:2px 6px;vertical-align:top}
td.kc{color:#6c7086;width:52px;flex-shrink:0}
td.tc{color:#89dceb}
td.nc{color:#a6e3a1}
td.vc{color:#fab387}
td.pc{color:#6c7086;font-size:11px}
tr:hover td{background:#313244;border-radius:3px}
a.tl{color:#89b4fa;text-decoration:none;cursor:pointer}
a.tl:hover{text-decoration:underline}
.refs-btn{font-size:11px;padding:2px 8px;border-radius:4px;border:1px solid #45475a;background:none;color:#6c7086;cursor:pointer;margin-top:6px}
.refs-btn:hover{border-color:#cba6f7;color:#cba6f7}
.refs-list{margin-top:4px;font-size:12px;font-family:'Cascadia Code','Fira Code',monospace}
.refs-list div{padding:1px 0;color:#cdd6f4}
.refs-list .rm{color:#6c7086;font-size:11px}
.hidden{display:none!important}
#refd-lbl{font-size:12px;color:#6c7086;white-space:nowrap;cursor:pointer;display:flex;align-items:center;gap:5px}
#refd-lbl input{cursor:pointer;accent-color:#cba6f7}
</style>
</head>
<body>
<div id="hdr">
  <h1>⚔ Pantheon Types</h1>
  <input type="search" id="search" placeholder="Search types…" autofocus>
  <label id="refd-lbl"><input type="checkbox" id="refd"> Hide unreferenced</label>
  <div id="stats"></div>
</div>
<div id="list"></div>
<script>
const T = /*DATA*/;
const FN = new Map(T.map((t,i) => [t.fn, i]));

function tlink(name, full) {
  if (!full || !FN.has(full)) return `<span class="tc">${esc(name)}</span>`;
  return `<a class="tl" data-fn="${esc(full)}">${esc(name)}</a>`;
}
function esc(s) {
  return s.replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;').replace(/"/g,'&quot;');
}

function renderMembers(t) {
  if (!t.m.length) return '<div style="color:#6c7086;font-size:12px;padding:4px 0">No members</div>';
  const vals = t.m.filter(m=>m.k==='v');
  const fields = t.m.filter(m=>m.k==='f');
  const props = t.m.filter(m=>m.k==='p');
  const methods = t.m.filter(m=>m.k==='m');
  let html = '';

  if (vals.length) {
    html += '<div class="section-label">Values</div><table>';
    for (const m of vals)
      html += `<tr><td class="kc">val</td><td class="vc">${esc(m.n)}</td><td class="pc">= ${m.v}</td></tr>`;
    html += '</table>';
  }
  if (fields.length) {
    html += '<div class="section-label">Fields</div><table>';
    for (const m of fields)
      html += `<tr><td class="kc">field</td><td class="tc">${tlink(m.t,m.tf)}</td><td class="nc">${esc(m.n)}</td></tr>`;
    html += '</table>';
  }
  if (props.length) {
    html += '<div class="section-label">Properties</div><table>';
    for (const m of props)
      html += `<tr><td class="kc">prop</td><td class="tc">${tlink(m.t,m.tf)}</td><td class="nc">${esc(m.n)}</td></tr>`;
    html += '</table>';
  }
  if (methods.length) {
    html += '<div class="section-label">Methods</div><table>';
    for (const m of methods) {
      const ps = m.ps.map(p=>`${tlink(p.t,p.tf)} <span style="color:#cdd6f4">${esc(p.n)}</span>`).join(', ');
      html += `<tr><td class="kc">method</td><td class="tc">${tlink(m.r,m.rf)}</td><td class="nc">${esc(m.n)}</td><td class="pc">(${ps})</td></tr>`;
    }
    html += '</table>';
  }
  return html;
}

function findRefs(fullName) {
  const out = [];
  for (const t of T) {
    const hits = [];
    for (const m of t.m) {
      if ((m.k==='f'||m.k==='p') && m.tf===fullName) hits.push(`${m.k==='f'?'field':'prop'} ${m.n}`);
      if (m.k==='m') {
        if (m.rf===fullName) hits.push(`return ${m.n}(…)`);
        else if (m.ps.some(p=>p.tf===fullName)) hits.push(`param ${m.n}(…)`);
      }
    }
    if (hits.length) out.push({t, hits});
  }
  return out;
}

// Render all type rows once
const listEl = document.getElementById('list');
function buildList() {
  const frag = document.createDocumentFragment();
  for (let i = 0; i < T.length; i++) {
    const t = T[i];
    const div = document.createElement('div');
    div.className = 'ti';
    div.id = `t${i}`;
    div.innerHTML =
      `<div class="th"><span class="badge ${t.k}">${t.k}</span>` +
      `<span class="tn">${esc(t.n)}</span>` +
      `<span class="tns">${esc(t.ns)}</span>` +
      (t.b ? `<span class="tbase">: ${esc(t.b)}</span>` : '') +
      `<span class="arr">▶</span></div>` +
      `<div class="tb" style="display:none"></div>`;
    div.querySelector('.th').addEventListener('click', () => toggle(i));
    frag.appendChild(div);
  }
  listEl.appendChild(frag);
}

function toggle(i) {
  const div = document.getElementById(`t${i}`);
  const body = div.querySelector('.tb');
  const open = div.classList.toggle('open');
  body.style.display = open ? '' : 'none';
  if (open && !body.dataset.r) {
    const t = T[i];
    body.innerHTML = renderMembers(t) +
      `<button class="refs-btn" data-i="${i}">Referenced by…</button><div class="refs-list" id="refs${i}" style="display:none"></div>`;
    body.dataset.r = '1';
    body.querySelectorAll('a.tl').forEach(a => a.addEventListener('click', e => {
      e.preventDefault();
      navigateTo(a.dataset.fn);
    }));
    body.querySelector('.refs-btn').addEventListener('click', () => showRefs(i));
  }
}

function showRefs(i) {
  const btn = document.querySelector(`.refs-btn[data-i="${i}"]`);
  const box = document.getElementById(`refs${i}`);
  if (box.style.display !== 'none') { box.style.display='none'; btn.textContent='Referenced by…'; return; }
  btn.textContent = 'Computing…';
  setTimeout(() => {
    const refs = findRefs(T[i].fn);
    if (!refs.length) { box.innerHTML='<div style="color:#6c7086;font-size:12px">None found</div>'; }
    else {
      box.innerHTML = refs.map(({t,hits}) =>
        `<div><a class="tl" data-fn="${esc(t.fn)}">${esc(t.n)}</a> <span class="rm">${hits.map(esc).join(', ')}</span></div>`
      ).join('');
      box.querySelectorAll('a.tl').forEach(a => a.addEventListener('click', e => {
        e.preventDefault(); navigateTo(a.dataset.fn);
      }));
    }
    box.style.display = '';
    btn.textContent = 'Referenced by ▲';
  }, 0);
}

function navigateTo(fullName) {
  const i = FN.get(fullName);
  if (i === undefined) return;
  const div = document.getElementById(`t${i}`);
  if (!div) return;
  if (div.classList.contains('hidden')) {
    document.getElementById('search').value = '';
    document.getElementById('refd').checked = false;
    applyFilter();
  }
  if (!div.classList.contains('open')) toggle(i);
  div.scrollIntoView({behavior:'smooth', block:'nearest'});
  div.classList.add('flash');
  setTimeout(() => div.classList.remove('flash'), 1200);
  location.hash = encodeURIComponent(fullName);
}

let refSet = null;
function getRefSet() {
  if (refSet) return refSet;
  refSet = new Set();
  for (const t of T)
    for (const m of t.m) {
      if (m.tf) refSet.add(m.tf);
      if (m.rf) refSet.add(m.rf);
      if (m.ps) for (const p of m.ps) if (p.tf) refSet.add(p.tf);
    }
  return refSet;
}

function typeMatches(t, q) {
  if (t.fn.toLowerCase().includes(q) || t.n.toLowerCase().includes(q)) return true;
  return t.m.some(m => m.n.toLowerCase().includes(q));
}

function applyFilter() {
  const q = document.getElementById('search').value.toLowerCase().trim();
  const hideUnref = document.getElementById('refd').checked;
  const rs = hideUnref ? getRefSet() : null;
  let n = 0;
  for (let i = 0; i < T.length; i++) {
    const t = T[i];
    const show = (!q || typeMatches(t, q)) && (!rs || rs.has(t.fn));
    document.getElementById(`t${i}`).classList.toggle('hidden', !show);
    if (show) n++;
  }
  document.getElementById('stats').textContent = `${n} / ${T.length} types`;
}

document.getElementById('search').addEventListener('input', applyFilter);
document.getElementById('refd').addEventListener('change', applyFilter);
document.getElementById('stats').textContent = `${T.length} types`;

buildList();

if (location.hash) {
  const fn = decodeURIComponent(location.hash.slice(1));
  if (fn) setTimeout(() => navigateTo(fn), 80);
}
</script>
</body>
</html>
""";

// ── Console output ─────────────────────────────────────────────────────────

void PrintTree(Type type, string indent, bool isRoot, int depth, HashSet<string> visited)
{
    var flags = BindingFlags.Public | BindingFlags.Instance;
    var baseMethodNames = new HashSet<string>(
        CollectBaseTypes(type).SelectMany(b => b.GetMethods(flags).Select(m => m.Name)));

    if (isRoot)
    {
        Console.WriteLine($"{type.FullName}");
        Console.WriteLine($"{indent}(base: {type.BaseType?.Name ?? "none"})");
    }

    visited.Add(type.FullName ?? type.Name);

    if (type.IsEnum)
    {
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static)
            .OrderBy(f => { try { return Convert.ToInt64(f.GetRawConstantValue()); } catch { return 0L; } })
            .ToArray();
        for (int i = 0; i < fields.Length; i++)
        {
            var f = fields[i];
            var branch = i == fields.Length - 1 ? "└── " : "├── ";
            try { Console.WriteLine($"{indent}{branch}{f.Name} = {f.GetRawConstantValue()}"); }
            catch { Console.WriteLine($"{indent}{branch}{f.Name}"); }
        }
        return;
    }

    var pubFields = type.GetFields(flags)
        .Where(f => !f.IsSpecialName && !f.Name.EndsWith("k__BackingField") && !IsNoisyMember(f.Name))
        .OrderBy(f => f.Name)
        .ToArray();

    var props = type.GetProperties(flags)
        .Where(p => !p.Name.EndsWith("k__BackingField") && !IsNoisyMember(p.Name))
        .OrderBy(p => p.Name)
        .ToArray();

    var methods = type.GetMethods(flags)
        .Where(m => !m.IsSpecialName && !baseMethodNames.Contains(m.Name) && !IsNoisyMember(m.Name))
        .OrderBy(m => m.Name)
        .ToArray();

    var allMembers = pubFields.Length + props.Length + methods.Length;
    var memberIndex = 0;

    foreach (var field in pubFields)
    {
        memberIndex++;
        var isLast = memberIndex == allMembers;
        var branch = isLast ? "└── " : "├── ";
        var childIndent = indent + (isLast ? "    " : "│   ");
        var memberType = field.FieldType;
        var gameType = ResolveGameType(memberType);

        Console.WriteLine($"{indent}{branch}[f] {FriendlyTypeName(memberType)}  {field.Name}");

        if (depth < maxDepth && gameType != null && !visited.Contains(gameType.FullName ?? gameType.Name))
            PrintTree(gameType, childIndent, false, depth + 1, visited);
    }

    foreach (var prop in props)
    {
        memberIndex++;
        var isLast = memberIndex == allMembers;
        var branch = isLast ? "└── " : "├── ";
        var childIndent = indent + (isLast ? "    " : "│   ");
        var memberType = prop.PropertyType;
        var gameType = ResolveGameType(memberType);

        Console.WriteLine($"{indent}{branch}{FriendlyTypeName(memberType)}  {prop.Name}");

        if (depth < maxDepth && gameType != null && !visited.Contains(gameType.FullName ?? gameType.Name))
            PrintTree(gameType, childIndent, false, depth + 1, visited);
    }

    foreach (var method in methods)
    {
        memberIndex++;
        var isLast = memberIndex == allMembers;
        var branch = isLast ? "└── " : "├── ";
        var parms = string.Join(", ", method.GetParameters().Select(p => $"{FriendlyTypeName(p.ParameterType)} {p.Name}"));
        Console.WriteLine($"{indent}{branch}{FriendlyTypeName(method.ReturnType)}  {method.Name}({parms})");
    }
}

void PrintRefs(Type target)
{
    var targetName = target.FullName ?? target.Name;
    Console.WriteLine($"References to {target.Name}:");

    bool any = false;
    foreach (var t in allTypes.Values.OrderBy(t => t.FullName))
    {
        var allFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
        var hits = new List<string>();

        try
        {
            foreach (var f in t.GetFields(allFlags))
                if (!f.IsSpecialName && !f.Name.EndsWith("k__BackingField") && TypeReferences(f.FieldType, targetName))
                    hits.Add($"field  {FriendlyTypeName(f.FieldType)} {f.Name}");
        }
        catch { }

        try
        {
            foreach (var p in t.GetProperties(allFlags))
                if (!p.Name.EndsWith("k__BackingField") && TypeReferences(p.PropertyType, targetName))
                    hits.Add($"prop   {FriendlyTypeName(p.PropertyType)} {p.Name}");
        }
        catch { }

        try
        {
            foreach (var m in t.GetMethods(allFlags).Where(m => !m.IsSpecialName))
            {
                var parms = string.Join(", ", m.GetParameters().Select(p => $"{FriendlyTypeName(p.ParameterType)} {p.Name}"));
                if (TypeReferences(m.ReturnType, targetName))
                    hits.Add($"return {FriendlyTypeName(m.ReturnType)} {m.Name}({parms})");
                else if (m.GetParameters().Any(p => TypeReferences(p.ParameterType, targetName)))
                    hits.Add($"param  {FriendlyTypeName(m.ReturnType)} {m.Name}({parms})");
            }
        }
        catch { }

        if (hits.Count > 0)
        {
            any = true;
            Console.WriteLine($"  {t.FullName}");
            foreach (var h in hits.Distinct())
                Console.WriteLine($"    {h}");
        }
    }

    if (!any)
        Console.WriteLine("  (none found)");
    Console.WriteLine();
}

bool TypeReferences(Type t, string targetFullName)
{
    if (t.FullName == targetFullName) return true;
    if (t.IsGenericType)
        return t.GetGenericArguments().Any(a => TypeReferences(a, targetFullName));
    return false;
}

Type? ResolveGameType(Type t)
{
    var ns = t.Namespace ?? "";
    if (!ns.StartsWith("Il2Cpp") && !ns.StartsWith("Il2CppPantheon")) return null;
    if (ns.StartsWith("Il2CppSystem") || ns.StartsWith("Il2CppInterop") || ns.StartsWith("Il2CppMono")) return null;
    allTypes.TryGetValue(t.FullName ?? t.Name, out var resolved);
    return resolved;
}

string FriendlyTypeName(Type t)
{
    if (!t.IsGenericType) return t.Name;
    var backtick = t.Name.IndexOf('`');
    var baseName = backtick >= 0 ? t.Name[..backtick] : t.Name;
    var args = t.GetGenericArguments().Select(FriendlyTypeName);
    return $"{baseName}<{string.Join(", ", args)}>";
}

bool IsGeneratedType(Type t)
{
    var name = t.Name;
    // Compiler-generated: closures (<>c), async state machines (<Method>d__0), etc.
    if (name.Contains('<') || name.Contains('>')) return true;
    // IL2CPP / compiler internals starting with double-underscore or underscore-Private
    if (name.StartsWith("__") || name.StartsWith("_Private")) return true;
    // Nested inside a generated type (e.g. _PrivateImplementationDetails_.ValueType*)
    if (t.DeclaringType != null && IsGeneratedType(t.DeclaringType)) return true;
    return false;
}

bool IsNoisyMember(string name) => name is
    "m_CachedPtr" or "ObjectClass" or "Pointer" or "WasCollected" or
    "NativeFieldInfoPtr" or "NativeMethodInfoPtr" or "isWrapped" or "pooledPtr" or
    "destroyCancellationToken" or "m_CancellationTokenSource" or
    "didAwake" or "didStart" or "useGUILayout" or
    "hideFlags" or "tag" or "transformHandle" ||
    name.StartsWith("NativeFieldInfoPtr_") || name.StartsWith("NativeMethodInfoPtr_");

static IEnumerable<Type> CollectBaseTypes(Type type)
{
    var t = type.BaseType;
    while (t != null) { yield return t; t = t.BaseType; }
}

static string? DetectGameDir()
{
    var candidates = new[]
    {
        @"D:\PantheonPTR\App",
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), @"Documents\My Games\Pantheon\App"),
        @"C:\Program Files (x86)\Steam\steamapps\common\Pantheon Rise of the Fallen",
    };
    return candidates.FirstOrDefault(d => Directory.Exists(Path.Combine(d, "MelonLoader")));
}
