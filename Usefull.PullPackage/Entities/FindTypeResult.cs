using System;
using System.Reflection;
using System.Runtime.Loader;

namespace Usefull.PullPackage.Entities
{
    /// <summary>
    /// A <see cref="AssemblyLoadContext.FindType"/> method result.
    /// </summary>
    public class FindTypeResult
    {
        /// <summary>
        /// The assembly in which <see cref="Type"/> was found.
        /// </summary>
        public Assembly Assembly { get; set; }

        /// <summary>
        /// The name by which the <see cref="Type"/> was found.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The found type or <see cref="null"/> if the type was not found.
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// Error while searching for <see cref="Type"/> in <see cref="Assembly"/>.
        /// </summary>
        public Exception Error { get; set; }
    }
}