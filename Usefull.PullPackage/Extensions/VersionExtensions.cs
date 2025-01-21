using System;

namespace Usefull.PullPackage.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="Version"/> enum.
    /// </summary>
    internal static class VersionExtensions
    {
        /// <summary>
        /// Returns the framework version that matches the environment version.
        /// </summary>
        /// <param name="version">The environment version.</param>
        /// <returns>The framework version that matches the environment version.</returns>
        public static FrameworkMoniker ToFrameworkMoniker(this Version version) => version.Major switch
        {
            9 => FrameworkMoniker.net9_0,
            8 => FrameworkMoniker.net8_0,
            7 => FrameworkMoniker.net7_0,
            6 => FrameworkMoniker.net6_0,
            5 => FrameworkMoniker.net5_0,
            4 => FrameworkMoniker.net481,
            3 => FrameworkMoniker.netcoreapp3_1,
            2 => FrameworkMoniker.net35,
            1 => FrameworkMoniker.netcoreapp1_1,
            _ => FrameworkMoniker.Unknown
        };
    }
}