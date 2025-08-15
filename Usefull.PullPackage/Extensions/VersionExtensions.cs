using System;
using Usefull.PullPackage.Entities;

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
        public static FrameworkMoniker ToFrameworkMoniker(this Version version)
        {
            switch (version.Major)
            {
                case 9:
                    return FrameworkMoniker.net9_0;
                case 8:
                    return FrameworkMoniker.net8_0;
                case 7:
                    return FrameworkMoniker.net7_0;
                case 6:
                    return FrameworkMoniker.net6_0;
                case 5:
                    return FrameworkMoniker.net5_0;
                case 4:
                    return FrameworkMoniker.net481;
                case 3:
                    return FrameworkMoniker.netcoreapp3_1;
                case 2:
                    return FrameworkMoniker.net35;
                case 1:
                    return FrameworkMoniker.netcoreapp1_1;
                default:
                    return FrameworkMoniker.Unknown;
            };
        }
    }
}