using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.BZip2;
using TempusToMomentumMapRenamer.Models;

namespace TempusToMomentumMapRenamer.Utilities
{
    public static class MapRenamerUtility
    {
        private static HttpClient _httpClient = new HttpClient();
        private static string GetMapSourcePath(string sourceDirectory, MapData mapData)
            => Path.Join(sourceDirectory, mapData.Name + ".bsp");

        private static SemaphoreSlim _webclientSemaphore = new SemaphoreSlim(5, 5);

        private static IEnumerable<string> GetMapDestinationPaths(string destinationDirectory, MapData mapData)
        {
            var mapNames = mapData.GetMomentumMapNames();

            foreach (var mapName in mapNames)
            {
                yield return Path.Combine(destinationDirectory, mapName + ".bsp");
            }
        }

        private static string RenameMap(MapData mapData, string sourceDirectory, string destinationDirectory, bool downloadMissingMap, bool copyToMomentumMod, Action<string> log)
        {
            var mapPath = GetMapSourcePath(sourceDirectory, mapData);
            var destinationPaths = GetMapDestinationPaths(destinationDirectory, mapData).ToList();

            if (!File.Exists(mapPath))
            {
                if (!downloadMissingMap)
                {
                    return mapData.Name + " doesn't exist in source";
                }

                if (copyToMomentumMod && destinationPaths.TrueForAll(File.Exists))
                {
                    return mapData.Name + " already exists in Momentum Mod files";
                }

                _webclientSemaphore.Wait();

                // FastDL provides map as bz2, need to decompress before saving to disk
                var mapUri = "http://tempus.site.nfoservers.com/server/maps/" + mapData.Name + ".bsp.bz2";
                log($"Downloading and decompressing {mapUri}");
                var compressedStream = _httpClient.GetStreamAsync(mapUri).GetAwaiter().GetResult();
                BZip2.Decompress(compressedStream, File.Create(mapPath), true);
                log($"Decompressed {mapData.Name}");

                _webclientSemaphore.Release();

                // File is decompressed and saved to source dir, safe to continue
            }

            if (!copyToMomentumMod)
            {
                return mapData.Name;
            }

            foreach (var destinationPath in destinationPaths)
            {
                if (File.Exists(destinationPath))
                {
                    log("Map already exists in destination directory: " + destinationPath);
                    continue;
                }

                File.Copy(mapPath, destinationPath);
            }


            return mapData.Name;
        }

        public static async Task RenameMapsAsync(List<MapData> selectedMaps, string sourceDirectory,
            string destinationDirectory, bool downloadMissingMaps, bool copyToMomentumMod, Action<string> mapDoneAction, Action<string> log)
        {
            log("Starting...");
            var copyTasks = selectedMaps
                .Select(x => Task.Run(() => RenameMap(x, sourceDirectory, destinationDirectory, downloadMissingMaps, copyToMomentumMod, log))).ToList();

            while (copyTasks.Any())
            {
                var finishedTask = await Task.WhenAny(copyTasks);
                copyTasks.Remove(finishedTask);

                mapDoneAction(finishedTask.Result);
            }
        }
    }
}