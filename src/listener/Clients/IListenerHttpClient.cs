using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace listener.Clients
{
    public interface IListenerHttpClient
    {
        Task<HttpResponseMessage> GetAsync(Uri requestUri, CancellationToken cancellationToken);
    }
}