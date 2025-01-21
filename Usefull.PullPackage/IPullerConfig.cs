using NuGet.Common;

namespace Usefull.PullPackage
{
    /// <summary>
    /// The puller configuration setup interface.
    /// </summary>
    public interface IPullerConfig
    {
        /// <summary>
        /// Sets the framework version.
        /// </summary>
        /// <param name="framework">The framework version.</param>
        /// <returns>A puller configuration.</returns>
        IPullerConfig Framework(FrameworkMoniker framework);

        /// <summary>
        /// Appends the packages source.
        /// </summary>
        /// <param name="name">The source name.</param>
        /// <param name="source">The source URI.</param>
        /// <returns>An added source configuration.</returns>
        ISourceConfig Source(string name, string source);

        /// <summary>
        /// Appends the package to pull.
        /// </summary>
        /// <param name="id">The package identity.</param>
        /// <param name="version">The package version.</param>
        /// <returns>A puller configuration.</returns>
        IPullerConfig Package(string id, string version);

        /// <summary>
        /// Sets the directory path where the pulling will be performed.
        /// </summary>
        /// <param name="directoryPath">The directory path.</param>
        /// <returns>A puller configuration.</returns>
        IPullerConfig Directory(string directoryPath);

        /// <summary>
        /// Sets the puller logger.
        /// </summary>
        /// <param name="logger">The puller logger.</param>
        /// <returns>A puller configuration.</returns>
        IPullerConfig Logger(ILogger logger);
    }
}