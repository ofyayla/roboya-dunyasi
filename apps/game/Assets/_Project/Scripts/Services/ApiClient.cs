using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Roboya.Services
{
    /// <summary>
    /// Small JSON client for the Roboya API (/v1). Hand-written until the OpenAPI client is generated
    /// (packages/api-contract); the endpoint paths and field names follow apps/api exactly.
    /// </summary>
    public sealed class ApiClient
    {
        private static readonly JsonSerializerSettings Json = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };

        private readonly string _baseUrl;
        private readonly IHttpTransport _transport;

        public ApiClient(string baseUrl, IHttpTransport transport)
        {
            _baseUrl = baseUrl.TrimEnd('/');
            _transport = transport;
        }

        public async Task<T> SendAsync<T>(string method, string path, object body = null, string bearer = null)
        {
            string text = await SendRawAsync(method, path, body, bearer);
            return JsonConvert.DeserializeObject<T>(text, Json);
        }

        public async Task SendAsync(string method, string path, object body = null, string bearer = null)
        {
            await SendRawAsync(method, path, body, bearer);
        }

        /// <summary>The response body as text, for downloads that are saved as they are (data export).</summary>
        public Task<string> GetTextAsync(string path, string bearer) => SendRawAsync("GET", path, null, bearer);

        private async Task<string> SendRawAsync(string method, string path, object body, string bearer)
        {
            var response = await _transport.SendAsync(new HttpRequest
            {
                Method = method,
                Url = _baseUrl + path,
                Body = body == null ? null : JsonConvert.SerializeObject(body, Json),
                BearerToken = bearer,
            });

            if (response.Status == 0)
            {
                throw new ApiException(0, ApiException.NetworkCode);
            }

            if (response.Status >= 200 && response.Status < 300)
            {
                return response.Body;
            }

            throw new ApiException(response.Status, CodeOf(response));
        }

        private static string CodeOf(HttpResponse response)
        {
            try
            {
                return JObject.Parse(response.Body ?? "{}").Value<string>("code") ?? "error";
            }
            catch (JsonException)
            {
                return "error";
            }
        }
    }
}
