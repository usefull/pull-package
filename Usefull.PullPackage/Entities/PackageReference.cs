using NuGet.Versioning;

namespace Usefull.PullPackage.Entities
{
    /// <summary>
    /// The package reference.
    /// </summary>
    public class PackageReference
    {
        /// <summary>
        /// The package name.
        /// </summary>
        public string PackageName { get; set; }

        /// <summary>
        /// The package version range.
        /// </summary>
        public VersionRange VersionRange { get; set; }
    }
}