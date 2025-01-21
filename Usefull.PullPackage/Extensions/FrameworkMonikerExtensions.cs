namespace Usefull.PullPackage.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="FrameworkMoniker"/> enum.
    /// </summary>
    internal static class FrameworkMonikerExtensions
    {
        /// <summary>
        /// Returns the standardized string token that represents the framework version.
        /// </summary>
        /// <param name="frameworkMoniker">The framework version moniker.</param>
        /// <returns>The standardized string token that represents the framework version.</returns>
        public static string ToStringToken(this FrameworkMoniker frameworkMoniker) =>
            frameworkMoniker.ToString().Replace('_', '.');
    }
}