using System.Reflection;

// Usage: dotnet run <TypeName> [<TypeName2> ...]
// Searches all Pantheon Il2Cpp interop assemblies for the given type(s) and dumps their members.
// Game install path is auto-detected; override with PANTHEON_DIR env var.

if (args.Length == 0)
{
    Console.Error.WriteLine("Usage: dotnet run <TypeName> [<TypeName2> ...]");
    Console.Error.WriteLine("  e.g. dotnet run EntityNpcGameObject EntityNameplate");
    return 1;
}

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

// Load all game assemblies and collect types
var gameAssemblies = new[] { "Il2CppScripts.dll", "Assembly-CSharp.dll", "Il2CppPlugins.dll" };
var allTypes = gameAssemblies
    .Select(name => Path.Combine(il2cppDir, name))
    .Where(File.Exists)
    .SelectMany(path => {
        try { return mlc.LoadFromAssemblyPath(path).GetTypes(); }
        catch { return Array.Empty<Type>(); }
    })
    .ToArray();

Console.WriteLine($"Indexed {allTypes.Length} types across game assemblies.\n");

foreach (var typeName in args)
{
    var matches = allTypes.Where(t => t.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase)).ToArray();

    if (matches.Length == 0)
    {
        // Fuzzy fallback: partial match
        matches = allTypes.Where(t => t.Name.Contains(typeName, StringComparison.OrdinalIgnoreCase)).ToArray();
        if (matches.Length == 0)
        {
            Console.WriteLine($"=== {typeName}: NOT FOUND ===\n");
            continue;
        }
        Console.WriteLine($"Exact match not found for '{typeName}'. Showing {matches.Length} partial match(es):");
        foreach (var m in matches) Console.WriteLine($"  {m.FullName}");
        Console.WriteLine();
        continue;
    }

    foreach (var type in matches)
    {
        DumpType(type);
        Console.WriteLine();
    }
}

return 0;

static void DumpType(Type type)
{
    var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
    var baseMethodNames = new HashSet<string>(
        CollectBaseTypes(type).SelectMany(b => b.GetMethods(flags).Select(m => m.Name)));

    Console.WriteLine($"=== {type.FullName} ===");
    Console.WriteLine($"Base: {type.BaseType?.FullName ?? "none"}");

    var props = type.GetProperties(flags)
        .Where(p => !p.Name.EndsWith("k__BackingField"))
        .OrderBy(p => p.Name)
        .ToArray();
    if (props.Length > 0)
    {
        Console.WriteLine("\nPROPERTIES:");
        foreach (var p in props)
            Console.WriteLine($"  {p.PropertyType.Name,-30} {p.Name}");
    }

    var fields = type.GetFields(flags)
        .Where(f => !f.Name.EndsWith("k__BackingField"))
        .OrderBy(f => f.Name)
        .ToArray();
    if (fields.Length > 0)
    {
        Console.WriteLine("\nFIELDS:");
        foreach (var f in fields)
            Console.WriteLine($"  {f.FieldType.Name,-30} {f.Name}");
    }

    var methods = type.GetMethods(flags)
        .Where(m => !m.IsSpecialName && !baseMethodNames.Contains(m.Name))
        .OrderBy(m => m.Name)
        .ToArray();
    if (methods.Length > 0)
    {
        Console.WriteLine("\nMETHODS (type-specific, base methods excluded):");
        foreach (var m in methods)
        {
            var parms = string.Join(", ", m.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
            Console.WriteLine($"  {m.ReturnType.Name,-20} {m.Name}({parms})");
        }
    }
}

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
