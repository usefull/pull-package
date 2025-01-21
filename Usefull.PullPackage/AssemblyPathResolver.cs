using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json.Nodes;

namespace Usefull.PullPackage
{
    /// <summary>
    /// The assembly file path resolution functionality.
    /// </summary>
    /// <param name="config">The puller configuration info.</param>
    /// <param name="assets">The pulled assets info.</param>
    /// <exception cref="ArgumentNullException">In case of any constrictor parameter is null.</exception>
    internal class AssemblyPathResolver(PullerConfig config, JsonNode assets)
    {
        private readonly PullerConfig _config = config ?? throw new ArgumentNullException(nameof(config));
        private readonly JsonNode _assets = assets ?? throw new ArgumentNullException(nameof(config), Resources.AssetsNotFound);
        private List<AssemblyInfo> _assetAssemblies = null;

        /// <summary>
        /// Resolves the assembly file path.
        /// </summary>
        /// <param name="assemblyName">The assembly's unique identity.</param>
        /// <returns>The assembly file path or null in case of failure.</returns>
        public string Resolve(AssemblyName assemblyName)
        {
            if (assemblyName == null)
                return null;

            return AssetAssemblies?.Where(a => a.Name.ToLower() == assemblyName.Name?.ToLower() && a.Version >= (assemblyName.Version ?? new Version()))
                ?.OrderByDescending(a => a.Version)
                ?.FirstOrDefault()
                ?.Path;
        }

        /// <summary>
        /// Resolves the assembly file path.
        /// </summary>
        /// <param name="assemblyName">The assembly name.</param>
        /// <param name="versionRange">The assembly version range.</param>
        /// <returns>The assembly file path or null in case of failure.</returns>
        public string Resolve(string assemblyName, VersionRange versionRange)
        {
            var vr = versionRange ?? VersionRange.All;
            return AssetAssemblies?.FirstOrDefault(a => a.Name == assemblyName && vr.Satisfies(a.NuGetVersion))?.Path;
        }

        /// <summary>
        /// The list of assemblies extracted from the pulled assets info or null in case of failure.
        /// </summary>
        private List<AssemblyInfo> AssetAssemblies
        {
            get
            {
                _assetAssemblies ??= _assets["targets"]?[config.FrameworkMoniker]?.AsObject()?.Select(i =>
                {
                    try
                    {
                        var parts = i.Key.Split('/');
                        var path = i.Value["runtime"]?.AsObject()?.FirstOrDefault().Key;
                        var nversion = new NuGetVersion(parts[1]);
                        var version = new Version(nversion.Major, nversion.Minor, nversion.Revision, nversion.Patch);
                        var res = new AssemblyInfo
                        {
                            Name = parts[0],
                            Version = version,
                            NuGetVersion = nversion,
                            Path = Path.Combine(_config.PackagesDirectory.FullName, i.Key, path).Replace('/', '\\')
                        };
                        return res;
                    }
                    catch
                    {
                        return null;
                    }
                }).Where(i => i != null).ToList();

                return _assetAssemblies;
            }
        }
    }
}