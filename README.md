# Usefull.PullPackage
.NET library for pulling NuGet packages and loading them dynamically in runtime.
## Usage
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
var puller = Puller.Build(config => config
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
Then you can use loaded assemblies functionality by scripting:
```cs
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
...
// Define references and imports (usings) for script
var options = ScriptOptions.Default
    .AddReferences(ctx.GetAssemblies())
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