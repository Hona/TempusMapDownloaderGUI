using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using TempusToMomentumMapRenamer.Models;

namespace TempusToMomentumMapRenamer.Utilities
{
    public static class MapDataUtility
    {
        /// <summary>
        ///     Map data from Tempus Hub source.
        /// </summary>
        private const string MapDataUri =
            "https://raw.githubusercontent.com/TheRealHona/TempusHub/master/src/TempusHubBlazor/wwwroot/MapClasses.csv";

        private const string StickyJumpPrefix = "sj";
        private const string RocketJumpPrefix = "rj";

        /// <summary>
        ///     Loads map class data from Tempus Hub
        /// </summary>
        public static async Task<List<MapData>> GetMapDataAsync()
        {
            var output = new List<MapData>();

            var client = new WebClient();
            var data = await client.DownloadStringTaskAsync(MapDataUri);
            var lines = data.Trim().Split('\n');

            foreach (var line in lines)
            {
                var parts = line.Split(',', 2);
                var classChar = parts[0];
                var mapName = parts[1];

                if (string.IsNullOrWhiteSpace(mapName))
                {
                    throw new Exception("Map name invalid for line: " + line);
                }

                var mapData = new MapData
                {
                    IntendedClass = classChar.ToUpper() switch
                    {
                        "S" => ClassInfo.Soldier,
                        "D" => ClassInfo.Demoman,
                        "B" => ClassInfo.Both,
                        _ => throw new Exception("Unexpected class data for line: " + line)
                    },
                    Name = mapName
                };

                output.Add(mapData);
            }

            return output;
        }

        /// <summary>
        ///     Converts the jump_ name to Momentum Mod map name
        /// </summary>
        public static List<string> GetMomentumMapNames(this MapData mapData)
        {
            var nameParts = mapData.Name.Split('_', 2);
            var nameEnding = nameParts[1];

            var rocketJumpName = $"{RocketJumpPrefix}_{nameEnding}";
            var stickyJumpName = $"{StickyJumpPrefix}_{nameEnding}";

            return mapData.IntendedClass switch
            {
                ClassInfo.Both => new List<string> {stickyJumpName, rocketJumpName},
                ClassInfo.Soldier => new List<string> {rocketJumpName},
                ClassInfo.Demoman => new List<string> {stickyJumpName},
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}