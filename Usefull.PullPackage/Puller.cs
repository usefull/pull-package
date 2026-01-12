using NuGet.Commands;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Usefull.PullPackage.Entities;
using Usefull.PullPackage.Extensions;

namespace Usefull.PullPackage
{
    /// <summary>
    /// The package pulling functionality.
    /// </summary>
    public sealed class Puller : IDisposable
    {
        private readonly PullerConfig _config;

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="cofig">The puller configuration.</param>
        private Puller(PullerConfig cofig)
        {
            _config = cofig;
        }

        /// <summary>
        /// The pulling result summary.
        /// </summary>
        /// <remarks>Null until pulled.</remarks>
        public RestoreSummary RestoreSummary { get; private set; }

        /// <summary>
        /// The pulled assets description.
        /// </summary>
        /// <remarks>Null until pulled.</remarks>
        public JsonNode Assets { get; private set; }

        /// <summary>
        /// The pulled packages.
        /// </summary>
        /// <remarks>Null until pulled.</remarks>
        public List<PackageInfo> Packages { get; private set; }

        /// <summary>
        /// Creates the package puller on specified configuration.
        /// </summary>
        /// <param name="configure">The configuration definition action.</param>
        /// <returns>The package puller.</returns>
        public static Puller Build(Action<IPullerConfig> configure)
        {
            var config = new PullerConfig();

            configure(config);

            return new Puller(config);
        }

        /// <summary>
        /// Performs packages pulling.
        /// </summary>
        /// <returns>A task that represents the asynchronous pulling operation.</returns>
        /// <exception cref="InvalidOperationException">In case when pulling is performed without a directory path set in the configuration.</exception>
        /// <exception cref="ArgumentException">In case when the directory path does not specify a valid file path or contains invalid characters.</exception>
        /// <exception cref="DirectoryNotFoundException">In case when the directory path is invalid, such as being on an unmapped drive.</exception>
        /// <exception cref="IOException">In case of an IO operation failure, for example, there is an open handle on the directory.</exception>
        /// <exception cref="PathTooLongException">In case of the directory path exceed the system-defined maximum length.</exception>
        /// <exception cref="SecurityException">In case when the caller does not have code access permission to create the directory.</exception>
        /// <exception cref="NotSupportedException">In case when the directory path contains a colon character (:) that is not part of a drive label.</exception>
        /// <exception cref="UnauthorizedAccessException">In case when the directory contains a read-only files.</exception>
        public async Task PullAsync()
        {
            RestoreSummary = null;
            Assets = null;
            Packages = null;

            _config.PrepareDirectory();
            _config.PrepareConfigFile();

            using (var cacheContext = new SourceCacheContext() { DirectDownload = true })
            {
                var restgoreArgs = _config.PrepareRestoreContext(cacheContext);
                RestoreSummary = (await RestoreRunner.RunAsync(restgoreArgs)).Single();

                using (var stream = File.OpenRead(_config.AssetsFilePath))
                {
                    Assets = JsonNode.Parse(stream);
                    Packages = Assets.ToPackagesInfo(_config.FrameworkMoniker, _config.PackagesDirectory.FullName);
                }
            }
        }

        /// <summary>
        /// Loads all pulled assemblies.
        /// </summary>
        /// <param name="isCollectible">true prescribes the creation of a collectible context that allows unloading.</param>
        /// <returns>A load context.</returns>
        public AssemblyLoadContext LoadAll(bool isCollectible = false)
        {
            var context = CreateLoadingContext(isCollectible);

            foreach (var assembly in Packages?.SelectMany(p => p.RuntimeAssemblies) ?? Enumerable.Empty<AssemblyInfo>())
                assembly.Loaded = context.LoadFromAssemblyPath(assembly.Path);

            return context;
        }

        /// <summary>
        /// Loads the specified package and all dependencies.
        /// </summary>
        /// <param name="packageName">The package name.</param>
        /// <param name="versionRange">The package version range.</param>
        /// <param name="context">The loading context.</param>
        /// <param name="isCollectible">true prescribes the creation of a collectible context that allows unloading.</param>
        /// <returns>A load context.</returns>
        /// <exception cref="ArgumentException">In case of an unacceptable load context.</exception>
        public AssemblyLoadContext Load(string packageName, VersionRange versionRange = default, AssemblyLoadContext context = null, bool isCollectible = false)
        {
            if (context == null)
                context = CreateLoadingContext(isCollectible);

            if (context is AssyLoadContext assyLoadContext)
            {
                var vr = versionRange ?? VersionRange.All;
                foreach (var assembly in Packages?.Where(p => p.Name == packageName && vr.Satisfies(p.Version))?.SelectMany(p => p.RuntimeAssemblies) ?? Enumerable.Empty<AssemblyInfo>())
                    assembly.Loaded = assyLoadContext.LoadFromAssemblyPath(assembly.Path);
            }
            else
                throw new ArgumentException(Resources.UnacceptableLoadContext, nameof(context));

            return context;
        }

        /// <summary>
        /// Nullifies all references to loaded assemblies to allow unloading.
        /// </summary>
        public void Dispose()
        {
            foreach (var a in Packages.SelectMany(p => p.RuntimeAssemblies))
                a.Loaded = null;
        }

        /// <summary>
        /// Creates the load context.
        /// </summary>
        /// <param name="isCollectible">true prescribes the creation of a collectible context that allows unloading.</param>
        /// <returns>A load context.</returns>
        private AssemblyLoadContext CreateLoadingContext(bool isCollectible = false) => new AssyLoadContext(CreateAssemblyPathResolver(), isCollectible);

        /// <summary>
        /// Creates the assembly path resolver.
        /// </summary>
        /// <returns>An assembly path resolver.</returns>
        private AssemblyPathResolver CreateAssemblyPathResolver() => new AssemblyPathResolver(Packages);
    }
}