using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace Roboya.Services
{
    public sealed class HttpRequest
    {
        public string Method { get; set; }

        public string Url { get; set; }

        /// <summary>JSON text, or null.</summary>
        public string Body { get; set; }

        public string BearerToken { get; set; }
    }

    public sealed class HttpResponse
    {
        public HttpResponse(int status, string body)
        {
            Status = status;
            Body = body;
        }

        /// <summary>0 when the server could not be reached.</summary>
        public int Status { get; }

        public string Body { get; }
    }

    /// <summary>The only place that touches the network stack, so everything above it is testable with a fake.</summary>
    public interface IHttpTransport
    {
        Task<HttpResponse> SendAsync(HttpRequest request);
    }

    public sealed class UnityHttpTransport : IHttpTransport
    {
        private const int TimeoutSeconds = 15;

        public Task<HttpResponse> SendAsync(HttpRequest request)
        {
            var done = new TaskCompletionSource<HttpResponse>();
            var web = new UnityWebRequest(request.Url, request.Method)
            {
                downloadHandler = new DownloadHandlerBuffer(),
                timeout = TimeoutSeconds,
            };
            if (request.Body != null)
            {
                web.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(request.Body));
                web.SetRequestHeader("Content-Type", "application/json");
            }

            if (request.BearerToken != null)
            {
                web.SetRequestHeader("Authorization", "Bearer " + request.BearerToken);
            }

            var op = web.SendWebRequest();
            op.completed += _ =>
            {
                // Connection failures and timeouts are reported as status 0, never as exceptions.
                bool reached = web.result == UnityWebRequest.Result.Success || web.result == UnityWebRequest.Result.ProtocolError;
                done.TrySetResult(new HttpResponse(reached ? (int)web.responseCode : 0, web.downloadHandler?.text));
                web.Dispose();
            };
            return done.Task;
        }
    }

    /// <summary>An expected failure: a stable server <see cref="Code"/>, or "network" when the server could not be reached.</summary>
    public sealed class ApiException : Exception
    {
        public const string NetworkCode = "network";

        public ApiException(int status, string code)
            : base("API error " + status + " " + code)
        {
            Status = status;
            Code = code;
        }

        public int Status { get; }

        public string Code { get; }

        public bool IsNetwork => Code == NetworkCode;
    }
}
