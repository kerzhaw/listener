using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace listener.Clients
{
    public class ListenerHttpClient : IListenerHttpClient
    {
        private static readonly HttpClient httpClient = new HttpClient();

        public Task<HttpResponseMessage> GetAsync(Uri requestUri, CancellationToken ct)
            => httpClient.GetAsync(requestUri, ct);
    }
}