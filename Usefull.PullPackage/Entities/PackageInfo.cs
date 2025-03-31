using NuGet.Versioning;
using System.Collections.Generic;

namespace Usefull.PullPackage.Entities
{
    /// <summary>
    /// The package info.
    /// </summary>
    public class PackageInfo
    {
        /// <summary>
        /// The package name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The package version.
        /// </summary>
        public NuGetVersion Version { get; set; }

        /// <summary>
        /// The package depencencies.
        /// </summary>
        public List<PackageReference> Dependencies { get; set; }

        /// <summary>
        /// Assemblies included in the package.
        /// </summary>
        public List<AssemblyInfo> RuntimeAssemblies { get; set; }
    }
}