using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Amazon.SimpleEmail;
using listener.Clients;
using listener.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;

namespace listener.Tests
{
    public abstract class ListenerControllerTestsBase
    {
        protected static readonly ILogger<ListenerController> MockLogger;
        protected static readonly IListenerHttpClient MockHttpClient;
        protected static readonly IAmazonSimpleEmailService MockSesClient;

        static ListenerControllerTestsBase()
        {
            MockLogger = new Mock<ILogger<ListenerController>>().Object;
            MockHttpClient = new Mock<IListenerHttpClient>().Object;
            MockSesClient = new Mock<IAmazonSimpleEmailService>().Object;
        }

        protected ListenerController CreateController(IHttpContextAccessor contextAccessor)
                => new ListenerController(contextAccessor, MockLogger, MockHttpClient, MockSesClient);

        protected ListenerController CreateController(IHttpContextAccessor contextAccessor, IListenerHttpClient httpClient)
            => new ListenerController(contextAccessor, MockLogger, httpClient, MockSesClient);

        protected ListenerController CreateController(IHttpContextAccessor contextAccessor, IAmazonSimpleEmailService sesClient)
            => new ListenerController(contextAccessor, MockLogger, MockHttpClient, sesClient);

        protected async Task<IHttpContextAccessor> CreateMockContextAccessor(
            IDictionary<string, string> headers,
            string jsonBody = null)
        {
            var contextAccessor = new Mock<IHttpContextAccessor>();
            var context = new Mock<HttpContext>();
            var request = new Mock<HttpRequest>();
            var headerDictionary = new HeaderDictionary();

            foreach (var key in headers.Keys)
                headerDictionary.Add(key, new StringValues(headers[key]));

            if (!string.IsNullOrWhiteSpace(jsonBody))
            {
                var stream = new MemoryStream();
                var writer = new StreamWriter(stream, Encoding.UTF8);

                await writer.WriteAsync(jsonBody);

                writer.Flush();
                stream.Seek(0, SeekOrigin.Begin);

                request.SetupGet(x => x.Body).Returns(stream);
            }

            request.SetupGet(x => x.Headers).Returns(headerDictionary);
            context.SetupGet(x => x.Request).Returns(request.Object);

            contextAccessor
                .SetupGet(x => x.HttpContext)
                .Returns(context.Object);

            return contextAccessor.Object;
        }
    }
}