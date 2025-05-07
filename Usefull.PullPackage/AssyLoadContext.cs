using NuGet.Versioning;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace Usefull.PullPackage
{
    /// <summary>
    /// An assemblies loading context.
    /// </summary>
    /// <param name="resolver">The assembly file path resolver.</param>
    /// <param name="isCollectible">The sign of a collectible context that allows unloading.</param>
    /// <exception cref="ArgumentNullException">In case of <paramref name="resolver"/> is null.</exception>
#if NETSTANDARD
    internal class AssyLoadContext(AssemblyPathResolver resolver) : AssemblyLoadContext()
#else
    internal class AssyLoadContext(AssemblyPathResolver resolver, bool isCollectible = false) : AssemblyLoadContext(isCollectible)
#endif
    {
        private readonly AssemblyPathResolver _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));

        /// <summary>
        /// Loads the assembly using the given resolver.
        /// </summary>
        /// <param name="assemblyName">The assembly name.</param>
        /// <param name="versionRange">The assembly version range.</param>
        /// <returns>A information about the loaded assembly.</returns>
        /// <exception cref="FileNotFoundException">In case when it was not possible to find an assembly file to load.</exception>
        /// <exception cref="FileLoadException">In case when the assembly file found but could not be loaded.</exception>
        /// <exception cref="BadImageFormatException">In case when the assembly file found but it is not a valid assembly.</exception>
        public Assembly Load(string assemblyName, VersionRange versionRange = default)
        {
            var path = _resolver.Resolve(assemblyName, versionRange);
            if (string.IsNullOrWhiteSpace(path))
            {
                var assyName = $"{assemblyName} {versionRange}";
                throw new FileNotFoundException(string.Format(Resources.AssemblyNotFoundInAssets, assyName));
            }

            var assembly = LoadFromAssemblyPath(path);
            return assembly;
        }

        /// <summary>
        /// Allows an assembly to be resolved based on <paramref name="assemblyName"/>.
        /// </summary>
        /// <param name="assemblyName">The assembly's unique identity in full.</param>
        /// <returns>A resolved assembly, or null in case of failure.</returns>
        /// <exception cref="FileLoadException">In case when the assembly file found but could not be loaded.</exception>
        /// <exception cref="BadImageFormatException">In case when the assembly file found but it is not a valid assembly.</exception>
        protected override Assembly Load(AssemblyName assemblyName)
        {
            var path = _resolver.Resolve(assemblyName);
            if (string.IsNullOrWhiteSpace(path))
                return null;

            var assembly = LoadFromAssemblyPath(path);
            return assembly;
        }
    }
}