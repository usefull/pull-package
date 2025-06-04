using Microsoft.CodeAnalysis.CSharp.Scripting;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using Usefull.PullPackage.Entities;
using Usefull.PullPackage.Extensions;

namespace Usefull.PullPackage.Sample
{
    internal class Program
    {
        static async Task Main()
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-us");

            await BasicDemoAsync();

            await UnloadingDemoAsync();
        }

        static async Task BasicDemoAsync()
        {
            Console.WriteLine("************************");
            Console.WriteLine("*** Basic usage demo ***");
            Console.WriteLine("************************");
            Console.WriteLine(string.Empty);

            using var puller = Puller.Build(config => config
                .Framework(FrameworkMoniker.net9_0)     // set target framework version
                .Package("System.Text.Json", "9.0.1")   // define packages to pull
                .Package("Humanizer.Core", "2.14.1")
                .Package("Npgsql", "9.0.2")
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
            await ScriptingUsing(ctx);

            // Using functionality of pulled assemblies by reflection in simplified manner
            SimplifiedReflectionUsing(ctx);

            Console.WriteLine(string.Empty);
        }

        static async Task UnloadingDemoAsync()
        {
            Console.WriteLine("********************************************************");
            Console.WriteLine("*** Unloading and deleting pulled packages after use ***");
            Console.WriteLine("********************************************************");
            Console.WriteLine(string.Empty);

            var directoryToPull = "E:\\_tmp\\";
            WeakReference ctxRef;

            using (var puller = Puller.Build(config => config
                .Framework(FrameworkMoniker.net8_0)     // set target framework version
                .Package("System.Text.Json", "9.0.1")   // define packages to pull
                .Package("Humanizer.Core", "2.14.1")
                .Package("Npgsql", "9.0.2")
                .Source("local", "E:\\Work\\VisualStudio\\HDS\\.net\\.nuget\\") // define local folder source
                    .WithMapping("Humanizer.Core")                              // define package which wil be pulled from this source
                .Source("nuget.org", "https://api.nuget.org/v3/index.json")     // define nuget.org source
                    .WithMapping("*")                                           // all other packages will be pulled from nuget.org
                .Directory(directoryToPull)   // set target directory to pull
            ))
            {
                // Perform pulling
                Console.Write("Pulling out assemblies...");
                await puller.PullAsync();
                Console.WriteLine($"\rSuccessfully pulled {puller.RestoreSummary.InstallCount} packages");
                Console.WriteLine(string.Empty);

                // Reading loaded assemblies list
                (var assys, ctxRef) = GetAssembles(puller);

                foreach(var assy in assys)
                    Console.WriteLine(assy);
            }
            Console.WriteLine(string.Empty);

            Console.Write("Waiting for context finalized...");
            while (ctxRef.IsAlive)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            Console.WriteLine($"\rContext finalized successfully  ");

            // Delete pulled packages files
            Directory.Delete(directoryToPull, true);
            Console.WriteLine($"Pulled packages files deleted");
            Console.WriteLine(string.Empty);
        }

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

        private static void SimplifiedReflectionUsing(AssemblyLoadContext ctx)
        {
            Console.WriteLine("Using assemblies by reflection (simplified)");
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");

            var dataSourceBuilder = ctx.CreateInstance(
                "Npgsql.NpgsqlDataSourceBuilder",
                ["Host=psqlhost;Port=5432;Username=user;Database=data;Password=pass"]);

            var dataSource = dataSourceBuilder.GetType().GetMethod("Build").Invoke(dataSourceBuilder, null);
            var connection = dataSource.GetType().GetMethod("OpenConnection").Invoke(dataSource, null);

            Console.WriteLine($"PostgreSQL connection state: {connection.GetType().GetProperty("State").GetValue(connection)}");

            connection.GetType().GetMethod("Close").Invoke(connection, null);
            Console.WriteLine($"PostgreSQL connection state: {connection.GetType().GetProperty("State").GetValue(connection)}");
        }

        private static async Task ScriptingUsing(AssemblyLoadContext ctx)
        {
            Console.WriteLine("Using assemblies by scripting");
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");

            // Build the script options
            var options = ctx.BuildScriptOptions()
                .AddImports("System", "Humanizer");

            // Invoke script and get result
            var result = await CSharpScript.RunAsync("DateTime.UtcNow.AddHours(-2).Humanize()", options);

            Console.WriteLine(result.ReturnValue);
            Console.WriteLine(string.Empty);
        }

        private static void ReflectionUsing(AssemblyLoadContext ctx)
        {
            Console.WriteLine("Using assemblies by reflection");
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");

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