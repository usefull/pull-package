using NuGet.Commands;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.ProjectModel;
using NuGet.Protocol.Core.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using Usefull.PullPackage.Entities;
using Usefull.PullPackage.Extensions;

namespace Usefull.PullPackage
{
    /// <summary>
    /// A puller configuration.
    /// </summary>
    internal class PullerConfig : IPullerConfig
    {
        private readonly string _projectName = "spec";
        private readonly string _configFileName = "nuget.config";
        private readonly string _assetsFileName = "project.assets.json";
        private readonly string _packagesDirectoryName = "packages";
        private readonly string _projectFileExtensions = ".csproj";

        private List<SourceConfig> _sources;
        private FrameworkMoniker _framework;
        private List<(string Id, string Version)> _packages;
        private DirectoryInfo _directory;
        private DirectoryInfo _packagesDirectory;
        private ILogger _logger;

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <remarks>Sets the default logger and the current framework version.</remarks>
        public PullerConfig()
        {
            _framework = Environment.Version.ToFrameworkMoniker();
            _logger = new DummyLogger();
        }

        /// <summary>
        /// The pulled assets description file path.
        /// </summary>
        /// <remarks>Null until pulled.</remarks>
        public string AssetsFilePath => _directory != null ? Path.Combine(_directory.FullName, _assetsFileName) : null;

        /// <summary>
        /// The assigned framework version.
        /// </summary>
        public string FrameworkMoniker => _framework.ToStringToken();

        /// <summary>
        /// The pulled packages directory path.
        /// </summary>
        /// <remarks>Null until pulled.</remarks>
        public DirectoryInfo PackagesDirectory => _packagesDirectory;

        /// <summary>
        /// Prepares the directory where the pulling will be performed.
        /// </summary>
        /// <exception cref="InvalidOperationException">In case when pulling is performed without a directory path set in the configuration.</exception>
        /// <exception cref="ArgumentException">In case when the directory path does not specify a valid file path or contains invalid characters.</exception>
        /// <exception cref="DirectoryNotFoundException">In case when the directory path is invalid, such as being on an unmapped drive.</exception>
        /// <exception cref="IOException">In case of the subdirectory creation failure, for example, there is an open handle on the directory.</exception>
        /// <exception cref="PathTooLongException">In case of the directory path exceed the system-defined maximum length.</exception>
        /// <exception cref="SecurityException">In case when the caller does not have code access permission to create the directory.</exception>
        /// <exception cref="NotSupportedException">In case when the directory path contains a colon character (:) that is not part of a drive label.</exception>
        /// <exception cref="UnauthorizedAccessException">In case when the directory contains a read-only files.</exception>
        public void PrepareDirectory()
        {
            if (_directory == null)
                throw new InvalidOperationException(string.Format(Resources.DirectoryPathNotDefined, nameof(Directory)));

            if (_directory.Exists)
            {
                _directory.EnumerateDirectories().ToList().ForEach(d => d.Delete(true));
                _directory.EnumerateFiles().ToList().ForEach(f => f.Delete());
            }
            else
                _directory.Create();

            _packagesDirectory = _directory.CreateSubdirectory(_packagesDirectoryName);
        }

        /// <summary>
        /// Prepares the NuGet configuration file.
        /// </summary>
        /// <exception cref="IOException">In case of file creating failure.</exception>
        public void PrepareConfigFile()
        {
            var sb = new StringBuilder("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sb.AppendLine("<configuration>");

            if (_sources != null && _sources.Any())
            {
                sb.AppendLine("\t<packageSourceMapping>");
                sb.AppendLine("\t\t<clear />");
                _sources.ForEach(s => sb.AppendLine(s.ToXmlConfigSection()));
                sb.AppendLine("\t</packageSourceMapping>");
            }

            sb.AppendLine("\t<disabledPackageSources>");
            sb.AppendLine("\t\t<clear />");
            sb.AppendLine("\t</disabledPackageSources>");

            sb.AppendLine("</configuration>");

            File.WriteAllText(Path.Combine(_directory.FullName, _configFileName), sb.ToString());
        }

        /// <summary>
        /// Prepares the NuGet packages restore context.
        /// </summary>
        /// <param name="cacheContext">The source cache context.</param>
        /// <returns>A NuGet packages restore context.</returns>
        public RestoreArgs PrepareRestoreContext(SourceCacheContext cacheContext)
        {
            var spec = PreparePackageSpec();

            spec.RestoreMetadata.Sources = _sources.Select(s => new PackageSource(s.SourceUri, s.Name)).ToList();
            spec.RestoreMetadata.ConfigFilePaths = [Path.Combine(_directory.FullName, _configFileName)];
            spec.RestoreMetadata.PackagesPath = _packagesDirectory.FullName;

            var dgSpec = new DependencyGraphSpec();
            dgSpec.AddProject(spec);
            dgSpec.AddRestore(spec.RestoreMetadata.ProjectUniqueName);

            var providerCache = new RestoreCommandProvidersCache();

            var restoreContext = new RestoreArgs()
            {
                CacheContext = cacheContext,
                DisableParallel = true,
                GlobalPackagesFolder = _packagesDirectory.FullName,
                ConfigFile = Path.Combine(_directory.FullName, _configFileName),
                Log = _logger,
                PreLoadedRequestProviders =
                [
                    new DependencyGraphSpecRequestProvider(providerCache, dgSpec)
                ]
            };

            return restoreContext;
        }

        /// <summary>
        /// Prepares the pulling specification.
        /// </summary>
        /// <returns>A pulling specification.</returns>
        private PackageSpec PreparePackageSpec()
        {
            var sb = new StringBuilder($"{{\"frameworks\":{{\"{_framework.ToStringToken()}\":{{\"dependencies\":{{");
            _packages.ForEach(p => sb.Append($"\"{p.Id}\":\"{p.Version}\","));
            var specJson = $"{sb.ToString().TrimEnd(',')}}}}}}}}}";

            var spec = JsonPackageSpecReader.GetPackageSpec(specJson, _projectName, _directory.FullName);

            var updated = spec.Clone();
            var packageSpecFile = new FileInfo(spec.FilePath);

            var projectDir = (packageSpecFile.Attributes & FileAttributes.Directory) == FileAttributes.Directory && !spec.FilePath.EndsWith(_projectFileExtensions) ?
                packageSpecFile.FullName :
                packageSpecFile.Directory.FullName;

            var projectPath = Path.Combine(projectDir, $"{spec.Name}{_projectFileExtensions}");
            updated.FilePath = projectPath;

            updated.RestoreMetadata = new ProjectRestoreMetadata
            {
                CrossTargeting = updated.TargetFrameworks.Count > 1,
                OriginalTargetFrameworks = updated.TargetFrameworks.Select(e => e.FrameworkName.GetShortFolderName()).ToList(),
                OutputPath = projectDir,
                ProjectStyle = ProjectStyle.DotnetToolReference,
                ProjectName = spec.Name,
                ProjectUniqueName = projectPath,
                ProjectPath = projectPath
            };
            updated.RestoreMetadata.CentralPackageVersionsEnabled = spec.RestoreMetadata?.CentralPackageVersionsEnabled ?? false;
            updated.RestoreMetadata.CentralPackageTransitivePinningEnabled = spec.RestoreMetadata?.CentralPackageTransitivePinningEnabled ?? false;

            updated.RestoreMetadata.RestoreAuditProperties = new RestoreAuditProperties()
            {
                EnableAudit = bool.FalseString
            };

            for (int i = 0; i < updated.TargetFrameworks.Count; i++)
            {
                var framework = updated.TargetFrameworks[i];
                if (string.IsNullOrEmpty(framework.TargetAlias))
                {
                    var clone = framework.Clone();
                    clone.TargetAlias = framework.FrameworkName.GetShortFolderName();
                    updated.TargetFrameworks[i] = clone;
                }
            }

            foreach (var framework in updated.TargetFrameworks)
            {
                updated.RestoreMetadata.TargetFrameworks.Add(new ProjectRestoreMetadataFrameworkInfo(framework.FrameworkName) { TargetAlias = framework.TargetAlias });
            }
            return updated;
        }

        #region IPullerConfig implementation

        /// <summary>
        /// Sets the framework version.
        /// </summary>
        /// <param name="framework">The framework version.</param>
        /// <returns>The current puller configuration.</returns>
        public IPullerConfig Framework(FrameworkMoniker framework)
        {
            _framework = framework;
            return this;
        }

        /// <summary>
        /// Appends the package to pull.
        /// </summary>
        /// <param name="id">The package identity.</param>
        /// <param name="version">The package version.</param>
        /// <returns>The current puller configuration.</returns>
        public IPullerConfig Package(string id, string version)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (version == null) throw new ArgumentNullException(nameof(version));

            var i = id.Trim();
            var v = version.Trim();

            if (i.Length == 0) throw new ArgumentException(Resources.PackageIdentifierCantBeEmpty, nameof(id));
            if (v.Length == 0) throw new ArgumentException(Resources.PackageVersionCantBeEmpty, nameof(version));

            _packages ??= [];

            if (_packages.Any(p => p.Id == i))
                throw new InvalidOperationException(Resources.PackageAlreadyRegistered);

            _packages.Add((i, v));

            return this;
        }

        /// <summary>
        /// Appends the packages source.
        /// </summary>
        /// <param name="name">The source name.</param>
        /// <param name="source">The source URI.</param>
        /// <returns>An added source configuration.</returns>
        public ISourceConfig Source(string name, string sourceUri)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (sourceUri == null) throw new ArgumentNullException(nameof(sourceUri));

            var n = name.Trim();
            var uri = sourceUri.Trim();

            if (n.Length == 0) throw new ArgumentException(Resources.SourceNameCantBeEmpty, nameof(name));
            if (uri.Length == 0) throw new ArgumentException(Resources.SourceUriCantBeEmpty, nameof(sourceUri));

            _sources ??= [];

            if (_sources.Any(s => s.Name == n))
                throw new InvalidOperationException(Resources.SourceAlreadyRegistered);

            if (_sources.Any(s => s.SourceUri == uri))
                throw new InvalidOperationException(Resources.SourceUriAlreadyRegistered);

            var sourceConfig = new SourceConfig(n, uri, this);
            _sources.Add(sourceConfig);

            return sourceConfig;
        }

        /// <summary>
        /// Sets the directory path where the pulling will be performed.
        /// </summary>
        /// <param name="directoryPath">The directory path.</param>
        /// <returns>The current puller configuration.</returns>
        public IPullerConfig Directory(string directoryPath)
        {
            _directory = new DirectoryInfo(directoryPath);
            return this;
        }

        /// <summary>
        /// Sets the puller logger.
        /// </summary>
        /// <param name="logger">The puller logger.</param>
        /// <returns>The current puller configuration.</returns>
        public IPullerConfig Logger(ILogger logger)
        {
            _logger = logger;
            return this;
        }

        #endregion
    }
}