using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.Globalization;
using System.Runtime.Loader;

namespace Usefull.PullPackage.Sample
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-us");

            // Configure and build puller
            var puller = Puller.Build(config => config
                .Framework(FrameworkMoniker.net8_0)     // set target framework version
                .Package("System.Text.Json", "9.0.1")   // define packages to pull
                .Package("Humanizer.Core", "2.14.1")
                .Source("local", "E:\\Work\\VisualStudio\\HDS\\.net\\.nuget\\") // define local folder source
                    .WithMapping("Humanizer.Core")                              // define package which wil be pulled from this source
                .Source("nuget.org", "https://api.nuget.org/v3/index.json")     // define nuget.org source
                    .WithMapping("*")                                           // all other packages will be pulled from nuget.org
                .Directory("E:\\_TEMP\\")   // set target directory to pull
            );

            // Perform pulling
            Console.Write("Pulling out assemblies...");
            await puller.PullAsync();
            Console.WriteLine($"\rSuccessfully pulled {puller.RestoreSummary.InstallCount} packages");
            Console.WriteLine(string.Empty);

            // Load all pulled assemblies
            var ctx = puller.LoadAll();

            // Using functionality of pulled assemblies by reflection
            ReflectionUsing(ctx);

            // Using functionality of pulled assemblies by scripting
            ScriptingUsing(ctx);
        }

        private static void ScriptingUsing(AssemblyLoadContext ctx)
        {
            Console.WriteLine("Using assemblies by scripting");
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");

            // Define references and imports (usings) for script
            var options = ScriptOptions.Default
                .AddReferences(ctx.Assemblies)
                .AddImports("System", "Humanizer");

            // Invoke script and get result
            var result = CSharpScript.RunAsync("DateTime.UtcNow.AddHours(-2).Humanize()", options)
                .GetAwaiter()
                .GetResult();

            Console.WriteLine(result.ReturnValue);
            Console.WriteLine(string.Empty);
        }

        private static void ReflectionUsing(AssemblyLoadContext ctx)
        {
            Console.WriteLine("Using assemblies by reflection");
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");

            // Find assembly System.Text.Json.dll in pulled packages
            var textJsonAssembly = ctx.Assemblies.FirstOrDefault(a => a.FullName?.Contains("Text.Json") ?? false);

            // Get type System.Text.Json.JsonSerializer
            var jsonSerializerType = textJsonAssembly?.GetType("System.Text.Json.JsonSerializer");

            // Get type System.Text.Json.JsonSerializerOptions
            var jsonSerializerOptionsType = textJsonAssembly?.GetType("System.Text.Json.JsonSerializerOptions");

            // Create instance of System.Text.Json.JsonSerializerOptions
            var jsonSerializerOptions = Activator.CreateInstance(jsonSerializerOptionsType);

            // Find method System.Text.Json.JsonSerializer.Serialize(object, Type, JsonSerializerOptions)
            var serializeMethod = jsonSerializerType.GetMethod("Serialize", [typeof(object), typeof(Type), jsonSerializerOptionsType]);

            // Invoke method
            var result = serializeMethod.Invoke(null, [new Entity { Name = "ewrerer", Value = 12 }, typeof(Entity), jsonSerializerOptions]);

            Console.WriteLine(result);
            Console.WriteLine(string.Empty);
        }

        internal class Entity
        {
            public string Name { get; set; }
            public int Value { get; set; }
        }
    }
}
