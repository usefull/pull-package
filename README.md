# Usefull.PullPackage
.NET library for pulling NuGet packages and loading them dynamically in runtime.
## Basic usage
Install NuGet package:
```
dotnet add package Usefull.PullPackage
```
Add using derective:
```cs
using Usefull.PullPackage;
```
Configure and build puller object:
```cs
using var puller = Puller.Build(config => config
    .Framework(FrameworkMoniker.net8_0)     // set target framework version
    .Package("System.Text.Json", "9.0.1")   // define packages to pull
    .Package("Humanizer.Core", "2.14.1")
    .Package("Npgsql", "9.0.2")
    .Source("local", "E:\\Work\\VisualStudio\\HDS\\.net\\.nuget\\") // define local folder source
        .WithMapping("Humanizer.Core")                              // define package which will be pulled from this source
    .Source("nuget.org", "https://api.nuget.org/v3/index.json")     // define nuget.org source
        .WithMapping("*")                                           // all other packages will be pulled from nuget.org
    .Directory("E:\\_TEMP\\")   // set target directory to pull
);
```
Perform pulling:
```cs
await puller.PullAsync();
```
Load all pulled assemblies:
```cs
var ctx = puller.LoadAll();
```
After that you can check out the pulling result in the property *puller.RestoreSummary.Success* and the list of errors in the property *puller.RestoreSummary.Errors*.

Then you can use loaded assemblies functionality by scripting:
```cs
using Microsoft.CodeAnalysis.CSharp.Scripting;
...
// Build script options and define imports (usings) for script
var options = ctx.BuildScriptOptions()
    .AddImports("System", "Humanizer");

// Invoke script and get result
var result = await CSharpScript.RunAsync("DateTime.UtcNow.AddHours(-2).Humanize()", options);
```
Or you can use it by reflection:
```cs
// Find assembly System.Text.Json.dll in pulled packages
var textJsonAssembly = ctx.GetAssemblies()
    .FirstOrDefault(a => a.FullName?.Contains("Text.Json") ?? false);

// Get type System.Text.Json.JsonSerializer
var jsonSerializerType = textJsonAssembly?.GetType("System.Text.Json.JsonSerializer");

// Get type System.Text.Json.JsonSerializerOptions
var jsonSerializerOptionsType = textJsonAssembly?.GetType("System.Text.Json.JsonSerializerOptions");

// Create instance of System.Text.Json.JsonSerializerOptions
var jsonSerializerOptions = Activator.CreateInstance(jsonSerializerOptionsType);

// Find method System.Text.Json.JsonSerializer.Serialize(object, Type, JsonSerializerOptions)
var serializeMethod = jsonSerializerType.GetMethod("Serialize", [typeof(object), typeof(Type), jsonSerializerOptionsType]);

// Invoke method
var result = serializeMethod.Invoke(null, [new Entity { Name = "Some entity", Value = 12 }, typeof(Entity), jsonSerializerOptions]);
```
Simplified usage by reflection:
```cs
// Attempt to create an instance of NpgsqlDataSourceBuilder
var dataSourceBuilder = ctx.CreateInstance(
    "Npgsql.NpgsqlDataSourceBuilder",
    ["Host=psqlhost;Port=5432;Username=user;Database=data;Password=pass"]);

// Build PostgreSQL data source and open connection
var dataSource = dataSourceBuilder.GetType().GetMethod("Build").Invoke(dataSourceBuilder, null);
var connection = dataSource.GetType().GetMethod("OpenConnection").Invoke(dataSource, null);
```
## Unloading the context after use
In order to be able to unload after use, it is necessary to load assemblies into a collectible context:
```cs
var ctx = puller.LoadAll(true);
```
When the context is no longer needed initiate unloading by calling:
```cs
ctx.Unload();
```
However, this does not guarantee immediate release of the memory occupied by the context. To force unloading, use the following technique:

1. First, wrap all context operations, starting with loading, inside a single non-asynchronous method. Mark the method with an attribute *MethodImplAttribute*. This method, in addition to the necessary results, should return a weak reference to the context. For example, like this:
```cs
[MethodImpl(MethodImplOptions.NoInlining)]
private static (List<string>, WeakReference) GetAssembles(Puller puller)
{
    // Load all pulled assemblies
    var ctx = puller.LoadAll(true);

    // Read assemblies location.
    var result = puller.Packages.SelectMany(a => a.RuntimeAssemblies)
        .Select(a => a.Path).ToList();

    // Unload context
    ctx.Unload();           

    // Return result and the context weak redference
    return (result, new WeakReference(ctx));
}
```
2. After disposing the puller, force an immediately garbage collection and wait for the context finalized:
```cs
    using (var puller = Puller.Build( ... ))
    {
        await puller.PullAsync();
    
        // Do the necessary work with the context
        (var assys, ctxRef) = GetAssembles(puller);

        foreach(var assy in assys)
            Console.WriteLine(assy);
    }

    // Waiting for the context finalized
    while (ctxRef.IsAlive)
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }
```
Under certain conditions, the described approach allows for the complete unloading of the context.
For more details see ["Use collectible AssemblyLoadContext"](https://learn.microsoft.com/en-us/dotnet/standard/assembly/unloadability#use-collectible-assemblyloadcontext).

A full usage examples are available in our [repository](https://github.com/usefull/pull-package/blob/main/Usefull.PullPackage.Sample/Program.cs).
