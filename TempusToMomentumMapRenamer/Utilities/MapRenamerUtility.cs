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
    }
}