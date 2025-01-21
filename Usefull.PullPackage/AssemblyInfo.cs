using NuGet.Versioning;
using System;

namespace Usefull.PullPackage
{
    /// <summary>
    /// An assembly descripting information.
    /// </summary>
    internal class AssemblyInfo
    {
        /// <summary>
        /// The assembly name/
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The assembly version.
        /// </summary>
        public Version Version { get; set; }

        /// <summary>
        /// The assembly version in the NuGet format.
        /// </summary>
        public NuGetVersion NuGetVersion { get; set; }

        /// <summary>
        /// The path to the assembly file.
        /// </summary>
        public string Path { get; set; }
    }
}