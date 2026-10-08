using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Roboya.Services
{
    /// <summary>
    /// Where the API lives. Nothing is compiled in: the address comes from the ROBOYA_API_URL variable or a
    /// <c>server.json</c> next to the progress files. Without either the game stays offline (no hosting is chosen yet).
    /// </summary>
    public static class ApiConfig
    {
        public const string Variable = "ROBOYA_API_URL";
        public const string File = "server.json";

        public static string Resolve(string folder)
        {
            string fromEnv = Environment.GetEnvironmentVariable(Variable);
            if (!string.IsNullOrWhiteSpace(fromEnv))
            {
                return fromEnv.Trim();
            }

            string path = Path.Combine(folder, File);
            if (!System.IO.File.Exists(path))
            {
                return null;
            }

            try
            {
                string url = JObject.Parse(System.IO.File.ReadAllText(path)).Value<string>("apiUrl");
                return string.IsNullOrWhiteSpace(url) ? null : url.Trim();
            }
            catch (Newtonsoft.Json.JsonException)
            {
                return null;
            }
        }
    }
}
