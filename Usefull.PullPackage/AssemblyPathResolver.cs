using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Usefull.PullPackage.Entities;

namespace Usefull.PullPackage
{
    /// <summary>
    /// The assembly file path resolution functionality.
    /// </summary>
    /// <param name="packages">The pulled packages info.</param>
    /// <exception cref="ArgumentNullException">In case of a constrictor parameter is null.</exception>
    internal class AssemblyPathResolver(List<PackageInfo> packages)
    {
        private readonly List<PackageInfo> _packages = packages ?? throw new ArgumentNullException(nameof(packages), Resources.AssetsNotFound);

        /// <summary>
        /// Resolves the assembly file path.
        /// </summary>
        /// <param name="assemblyName">The assembly's unique identity.</param>
        /// <returns>The assembly file path or null in case of failure.</returns>
        public string Resolve(AssemblyName assemblyName)
        {
            if (assemblyName == null)
                return null;

            return _packages.SelectMany(p => p.RuntimeAssemblies).Where(a => a.Name.ToLower() == assemblyName.Name?.ToLower())
                .OrderByDescending(a => a.Version)
                .FirstOrDefault()
                ?.Path;
        }

        /// <summary>
        /// Resolves the assembly file path.
        /// </summary>
        /// <param name="assemblyName">The assembly name.</param>
        /// <param name="versionRange">The assembly version range.</param>
        /// <returns>The assembly file path or null in case of failure.</returns>
        public string Resolve(string assemblyName, VersionRange versionRange)
        {
            var vr = versionRange ?? VersionRange.All;
            return _packages.SelectMany(p => p.RuntimeAssemblies.Select(i => new { Name = i.Name.ToLower(), p.Version, i.Path }))
                .FirstOrDefault(i => i.Name == assemblyName.ToLower() && vr.Satisfies(i.Version))
                ?.Path;
        }
    }
}