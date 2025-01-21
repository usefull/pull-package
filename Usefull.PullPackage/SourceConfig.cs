using NuGet.Common;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Usefull.PullPackage
{
    /// <summary>
    /// A packages source configuration.
    /// </summary>
    /// <param name="name">The source name.</param>
    /// <param name="source">The source URI.</param>
    /// <param name="pullerConfig">The puller configuration in which this source configuration defined.</param>
    internal class SourceConfig(string name, string source, IPullerConfig pullerConfig) : ISourceConfig
    {
        private List<string> _matchingPatterns;

        /// <summary>
        /// Sets the framework version.
        /// </summary>
        /// <param name="framework">The framework version.</param>
        /// <returns>The current puller configuration.</returns>
        public IPullerConfig Framework(FrameworkMoniker framework) =>
            pullerConfig.Framework(framework);

        /// <summary>
        /// Appends the package to pull.
        /// </summary>
        /// <param name="id">The package identity.</param>
        /// <param name="version">The package version.</param>
        /// <returns>The current puller configuration.</returns>
        public IPullerConfig Package(string id, string version) =>
            pullerConfig.Package(id, version);

        /// <summary>
        /// Appends the packages source.
        /// </summary>
        /// <param name="name">The source name.</param>
        /// <param name="source">The source URI.</param>
        /// <returns>The current added source configuration.</returns>
        public ISourceConfig Source(string name, string source) =>
            pullerConfig.Source(name, source);

        /// <summary>
        /// Sets the directory path where the pulling will be performed.
        /// </summary>
        /// <param name="directoryPath">The directory path.</param>
        /// <returns>The current puller configuration.</returns>
        public IPullerConfig Directory(string directoryPath) =>
            pullerConfig.Directory(directoryPath);

        /// <summary>
        /// Sets the puller logger.
        /// </summary>
        /// <param name="logger">The puller logger.</param>
        /// <returns>The current puller configuration.</returns>
        public IPullerConfig Logger(ILogger logger) =>
            pullerConfig.Logger(logger);

        /// <summary>
        /// Appends the package mapping.
        /// </summary>
        /// <param name="pattern">The package mapping pattern.</param>
        /// <returns>The current source configuration.</returns>
        public ISourceConfig WithMapping(string pattern)
        {
            if (pattern != null)
            {
                var p = pattern.Trim();
                if (p.Any())
                {
                    _matchingPatterns ??= [];

                    if (!_matchingPatterns.Any(mp => mp == p))
                        _matchingPatterns.Add(p);
                }
            }
            return this;
        }

        /// <summary>
        /// Builds the XML section for the NuGet configuration file.
        /// </summary>
        /// <returns>A string that represents a XML configuration section.</returns>
        public string ToXmlConfigSection()
        {
            if (_matchingPatterns == null || !_matchingPatterns.Any())
                return string.Empty;

            return _matchingPatterns.Aggregate(
                new StringBuilder($"\t\t<packageSource key=\"{name}\">"),
                (sb, p) =>
                {
                    sb.AppendLine($"\t\t\t<package pattern=\"{p}\" />");
                    return sb;
                }
            ).AppendLine("\t\t</packageSource>")
            .ToString();
        }

        /// <summary>
        /// The source URI.
        /// </summary>
        public string SourceUri => source;

        /// <summary>
        /// The source name.
        /// </summary>
        public string Name => name;
    }
}