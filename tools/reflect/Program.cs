using System.Reflection;

// Usage: dotnet run <TypeName> [<TypeName2> ...]
// Searches all Pantheon Il2Cpp interop assemblies for the given type(s) and dumps a member tree.
// Game install path is auto-detected; override with PANTHEON_DIR env var.
// Add --depth=N to control recursion depth (default: 2). Use --depth=0 for flat dump.

Console.OutputEncoding = System.Text.Encoding.UTF8;

if (args.Length == 0)
{
    Console.Error.WriteLine("Usage: dotnet run <TypeName> [<TypeName2> ...]");
    Console.Error.WriteLine("  e.g. dotnet run EntityNpcGameObject");
    Console.Error.WriteLine("  e.g. dotnet run EntityNpcGameObject --depth=3");
    return 1;
}

var maxDepth = 2;
var typeArgs = args.Where(a => {
    if (a.StartsWith("--depth=") && int.TryParse(a["--depth=".Length..], out var d)) { maxDepth = d; return false; }
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

var gameAssemblies = new[] { "Il2CppScripts.dll", "Assembly-CSharp.dll", "Il2CppPlugins.dll" };
var allTypes = gameAssemblies
    .Select(name => Path.Combine(il2cppDir, name))
    .Where(File.Exists)
    .SelectMany(path => {
        try { return mlc.LoadFromAssemblyPath(path).GetTypes(); }
        catch { return Array.Empty<Type>(); }
    })
    .GroupBy(t => t.FullName ?? t.Name)
    .ToDictionary(g => g.Key, g => g.First());

Console.WriteLine($"Indexed {allTypes.Count} types across game assemblies.\n");

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
    }
}

return 0;

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

    // Collect interesting members: properties and type-specific methods
    var props = type.GetProperties(flags)
        .Where(p => !p.Name.EndsWith("k__BackingField") && !IsNoisyMember(p.Name))
        .OrderBy(p => p.Name)
        .ToArray();

    var methods = type.GetMethods(flags)
        .Where(m => !m.IsSpecialName && !baseMethodNames.Contains(m.Name) && !IsNoisyMember(m.Name))
        .OrderBy(m => m.Name)
        .ToArray();

    var allMembers = props.Length + methods.Length;
    var memberIndex = 0;

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

Type? ResolveGameType(Type t)
{
    var ns = t.Namespace ?? "";
    // Only recurse into game-specific namespaces, not Unity/System/Interop infrastructure
    if (!ns.StartsWith("Il2Cpp") && !ns.StartsWith("Il2CppPantheon")) return null;
    // Exclude infrastructure namespaces
    if (ns.StartsWith("Il2CppSystem") || ns.StartsWith("Il2CppInterop") || ns.StartsWith("Il2CppMono")) return null;
    allTypes.TryGetValue(t.FullName ?? t.Name, out var resolved);
    return resolved;
}

string FriendlyTypeName(Type t)
{
    if (!t.IsGenericType) return t.Name;
    var baseName = t.Name[..t.Name.IndexOf('`')];
    var args = t.GetGenericArguments().Select(FriendlyTypeName);
    return $"{baseName}<{string.Join(", ", args)}>";
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
