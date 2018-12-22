using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
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

        static ListenerControllerTestsBase()
        {
            MockLogger = new Mock<ILogger<ListenerController>>().Object;
        }

        protected async Task<IHttpContextAccessor> CreateMockContextAccessor(IDictionary<string, string> headers, string jsonBody = null)
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
            contextAccessor.SetupGet(x => x.HttpContext).Returns(context.Object);

            return contextAccessor.Object;
        }
    }
}