using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using NuGet.Common;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using WeihanLi.Common.Helpers;

namespace NugetSample
{
    public class NugetClientSdkSample
    {
        public static async Task Test()
        {
            var packageId = "WeihanLi.Common";
            var packageVersion = new NuGetVersion("1.0.38");

            var logger = NullLogger.Instance;
            var cache = new SourceCacheContext();
            var repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");

            {
                var packagesFolder = Environment.GetEnvironmentVariable("NUGET_PACKAGES");

                if (string.IsNullOrEmpty(packagesFolder))
                {
                    // Nuget globalPackagesFolder resolve
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        var defaultConfigFilePath =
                            $@"{Environment.GetEnvironmentVariable("APPDATA")}\NuGet\NuGet.Config";
                        if (File.Exists(defaultConfigFilePath))
                        {
                            var doc = new XmlDocument();
                            doc.Load(defaultConfigFilePath);
                            var node = doc.SelectSingleNode("/configuration/config/add[@key='globalPackagesFolder']");
                            if (node != null)
                            {
                                packagesFolder = node.Attributes["value"]?.Value;
                            }
                        }

                        if (string.IsNullOrEmpty(packagesFolder))
                        {
                            packagesFolder = $@"{Environment.GetEnvironmentVariable("USERPROFILE")}\.nuget\packages";
                        }
                    }
                    else
                    {
                        var defaultConfigFilePath =
                            $@"{Environment.GetEnvironmentVariable("HOME")}/.config/NuGet/NuGet.Config";
                        if (File.Exists(defaultConfigFilePath))
                        {
                            var doc = new XmlDocument();
                            doc.Load(defaultConfigFilePath);
                            var node = doc.SelectSingleNode("/configuration/config/add[@key='globalPackagesFolder']");
                            if (node != null)
                            {
                                packagesFolder = node.Attributes["value"]?.Value;
                            }
                        }

                        if (string.IsNullOrEmpty(packagesFolder))
                        {
                            defaultConfigFilePath = $@"{Environment.GetEnvironmentVariable("HOME")}/.nuget/NuGet/NuGet.Config";
                            if (File.Exists(defaultConfigFilePath))
                            {
                                var doc = new XmlDocument();
                                doc.Load(defaultConfigFilePath);
                                var node = doc.SelectSingleNode("/configuration/config/add[@key='globalPackagesFolder']");
                                if (node != null)
                                {
                                    packagesFolder = node.Value;
                                }
                            }
                        }

                        if (string.IsNullOrEmpty(packagesFolder))
                        {
                            packagesFolder = $@"{Environment.GetEnvironmentVariable("HOME")}/.nuget/packages";
                        }
                    }
                }

                Console.WriteLine($"globalPackagesFolder: {packagesFolder}");
            }

            {
                // get packages
                var autoCompleteResource = await repository.GetResourceAsync<AutoCompleteResource>();
                var packages =
                    await autoCompleteResource.IdStartsWith("WeihanLi", false, logger, CancellationToken.None);
                foreach (var package in packages)
                {
                    Console.WriteLine($"Found Package {package}");
                }
            }

            {
                // get package versions
                var findPackageByIdResource = await repository.GetResourceAsync<FindPackageByIdResource>();
                var versions = await findPackageByIdResource.GetAllVersionsAsync(
                    packageId,
                    cache,
                    logger,
                    CancellationToken.None);

                foreach (var version in versions)
                {
                    Console.WriteLine($"Found version {version}");
                }
            }

            {
                var resource = await repository.GetResourceAsync<PackageSearchResource>();
                var searchFilter = new SearchFilter(includePrerelease: false);

                var results = await resource.SearchAsync(
                    "weihanli",
                    searchFilter,
                    skip: 0,
                    take: 20,
                    logger,
                    CancellationToken.None);
                foreach (var result in results)
                {
                    Console.WriteLine($"Found package {result.Identity.Id} {result.Identity.Version}");
                }
            }

            {
                var packageMetadataResource = await repository.GetResourceAsync<PackageMetadataResource>();
                var packages = await packageMetadataResource.GetMetadataAsync(
                    "WeihanLi.Npoi",
                    includePrerelease: true,
                    includeUnlisted: false,
                    cache,
                    logger,
                    CancellationToken.None);

                foreach (var package in packages)
                {
                    Console.WriteLine($"Version: {package.Identity.Version}");
                    Console.WriteLine($"Listed: {package.IsListed}");
                    Console.WriteLine($"Tags: {package.Tags}");
                    Console.WriteLine($"Description: {package.Description}");
                }
            }

            {
                var pkgDownloadContext = new PackageDownloadContext(cache);
                var downloadRes = await repository.GetResourceAsync<DownloadResource>();

                var downloadResult = await RetryHelper.TryInvokeAsync(async () =>
                    await downloadRes.GetDownloadResourceResultAsync(
                        new PackageIdentity(packageId, packageVersion),
                        pkgDownloadContext,
                        @"C:\Users\liweihan\.nuget\packages",
                        logger,
                        CancellationToken.None), r => true);
                Console.WriteLine(downloadResult.Status.ToString());
            }
        }
    }
}
