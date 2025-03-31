using NuGet.Versioning;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using Usefull.PullPackage.Entities;

namespace Usefull.PullPackage.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="JsonNode"/>.
    /// </summary>
    internal static class JsonNodeExtensions
    {
        /// <summary>
        /// Extracts pulled packages info fron JSON node representing the project.assets.json file content.
        /// </summary>
        /// <param name="jsonNode">The JSON node object.</param>
        /// <param name="framework">The framework moniker.</param>
        /// <param name="packagesPath">The pulling directory path.</param>
        /// <returns>The liat of pulled packages.</returns>
        public static List<PackageInfo> ToPackagesInfo(this JsonNode jsonNode, string framework, string packagesPath) =>
            (jsonNode?["targets"]?[framework]?.AsObject()?.Select(n =>
            {
                var parts = n.Key.Split('/');
                if (parts.Length < 2)
                    return null;

                if (!NuGetVersion.TryParse(parts[1], out var version))
                    return null;

                return new PackageInfo
                {
                    Name = parts[0],
                    Version = version,
                    Dependencies = (n.Value["dependencies"]?.AsObject()?.Select(d =>
                    {
                        if (!VersionRange.TryParse(d.Value.ToString(), out var versionRange))
                            return null;

                        return new PackageReference { PackageName = d.Key, VersionRange = versionRange };
                    })?.Where(d => d != null) ?? []).ToList(),
                    RuntimeAssemblies = (n.Value["runtime"]?.AsObject()?.Select(r =>
                    {
                        var i = r.Key.LastIndexOf('/');
                        var n = i >= 0 ? r.Key.Substring(i + 1) : r.Key;
                        i = n.LastIndexOf('.');
                        return new AssemblyInfo
                        {
                            Name = i >= 0 ? n.Substring(0, i) : n,
                            Version = new System.Version(version.ToFullString()),
                            Path = Path.Combine(packagesPath, parts[0], version.ToString(), r.Key).Replace('/', '\\')
                        };
                    }) ?? []).ToList()
                };
            }).Where(d => d != null) ?? []).ToList();
    }
}