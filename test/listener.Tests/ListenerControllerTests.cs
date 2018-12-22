using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using listener.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Moq;
using Xunit;

namespace listener.Tests
{
    public class ListenerControllerTests
    {
        private static readonly ILogger<ListenerController> mockLogger;

        static ListenerControllerTests()
        {
            mockLogger = new Mock<ILogger<ListenerController>>().Object;
        }

        private async Task<IHttpContextAccessor> CreateMockContextAccessor(IDictionary<string, string> headers, string jsonBody = null)
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
                var writer = new StreamWriter(stream);
                await writer.WriteAsync(jsonBody);

                stream.Seek(0, SeekOrigin.Begin);

                request.SetupGet(x => x.Body).Returns(stream).Callback(() =>
                {
                    stream.Dispose();
                });
            }

            request.SetupGet(x => x.Headers).Returns(headerDictionary);
            context.SetupGet(x => x.Request).Returns(request.Object);
            contextAccessor.SetupGet(x => x.HttpContext).Returns(context.Object);

            return contextAccessor.Object;
        }

        [Fact]
        public async Task ProcessSubscriptionConfirmationMessageType_Returns_400_For_Bad_UserAgent()
        {
            var mockContextAccessor = await CreateMockContextAccessor(new Dictionary<string, string>
            {
                {HeaderNames.UserAgent,"Dis a bad user agent mon"}
            });

            var controller = new ListenerController(mockContextAccessor, mockLogger);
            var result = await controller.PostAsync() as StatusCodeResult;

            Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task ProcessSubscriptionConfirmationMessageType_Returns_400_For_Missing_UserAgent()
        {
            var mockContextAccessor = await CreateMockContextAccessor(new Dictionary<string, string>());

            var controller = new ListenerController(mockContextAccessor, mockLogger);
            var result = await controller.PostAsync() as StatusCodeResult;

            Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task ProcessSubscriptionConfirmationMessageType_Returns_400_For_Missing_MessageType_Header()
        {
            var mockContextAccessor = await CreateMockContextAccessor(new Dictionary<string, string>{
                {HeaderNames.UserAgent, ListenerController.AwsSnsUserAgentHeaderValue}
            });

            var controller = new ListenerController(mockContextAccessor, mockLogger);
            var result = await controller.PostAsync() as StatusCodeResult;

            Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task ProcessSubscriptionConfirmationMessageType_Returns_400_For_Bad_MessageType_Header()
        {
            var mockContextAccessor = await CreateMockContextAccessor(new Dictionary<string, string>{
                {HeaderNames.UserAgent, ListenerController.AwsSnsUserAgentHeaderValue},
                {ListenerController.AwsSnsMessageTypeHeaderName, "Bad value"}
            });

            var controller = new ListenerController(mockContextAccessor, mockLogger);
            var result = await controller.PostAsync() as StatusCodeResult;

            Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task ProcessSubscriptionConfirmationMessageType_Happy()
        {
            var mockContextAccessor = await CreateMockContextAccessor(new Dictionary<string, string>
            {
                {HeaderNames.UserAgent, ListenerController.AwsSnsUserAgentHeaderValue},
                {ListenerController.AwsSnsMessageTypeHeaderName, ListenerController.AwsSnsMessageTypeHeaderValueSubscriptionConfirmation}
            });

            var controller = new ListenerController(mockContextAccessor, mockLogger);
            var result = await controller.PostAsync() as StatusCodeResult;

            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }


    }
}
