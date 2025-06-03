using System;
using System.Reflection;

namespace Usefull.PullPackage.Entities
{
    /// <summary>
    /// An assembly descripting information.
    /// </summary>
    public class AssemblyInfo
    {
        /// <summary>
        /// The assembly name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The assembly version.
        /// </summary>
        public Version Version { get; set; }

        /// <summary>
        /// The path to the assembly file.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Loaded assembly.
        /// </summary>
        public Assembly Loaded { get; set; }
    }
}