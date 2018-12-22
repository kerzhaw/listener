using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using listener.Controllers;
using listener.Clients;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Moq;
using Xunit;
using System.Threading;
using System.Net;
using System.Net.Http;
using listener.Models;

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

    public class ListenerControllerTests : ListenerControllerTestsBase
    {

        [Fact]
        public async Task ProcessSubscriptionConfirmationMessageType_Returns_400_For_Bad_UserAgent()
        {
            var mockContextAccessor = await CreateMockContextAccessor(new Dictionary<string, string>
            {
                {HeaderNames.UserAgent,"Dis a bad user agent mon"}
            });

            var controller = new ListenerController(mockContextAccessor, MockLogger, new Mock<IListenerHttpClient>().Object);
            var result = await controller.PostAsync() as StatusCodeResult;

            Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task ProcessSubscriptionConfirmationMessageType_Returns_400_For_Missing_UserAgent()
        {
            var mockContextAccessor = await CreateMockContextAccessor(new Dictionary<string, string>());

            var controller = new ListenerController(mockContextAccessor, MockLogger, new Mock<IListenerHttpClient>().Object);
            var result = await controller.PostAsync() as StatusCodeResult;

            Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task ProcessSubscriptionConfirmationMessageType_Returns_400_For_Missing_MessageType_Header()
        {
            var mockContextAccessor = await CreateMockContextAccessor(new Dictionary<string, string>{
                {HeaderNames.UserAgent, ListenerController.AwsSnsUserAgentHeaderValue}
            });

            var controller = new ListenerController(mockContextAccessor, MockLogger, new Mock<IListenerHttpClient>().Object);
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

            var controller = new ListenerController(mockContextAccessor, MockLogger, new Mock<IListenerHttpClient>().Object);
            var result = await controller.PostAsync() as StatusCodeResult;

            Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task ProcessSubscriptionConfirmationMessageType_Happy_Notification()
        {
            var jsonBody = await File.ReadAllTextAsync(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sample-notification.json")
            );

            var mockContextAccessor = await CreateMockContextAccessor(new Dictionary<string, string>
            {
                {HeaderNames.UserAgent, ListenerController.AwsSnsUserAgentHeaderValue},
                {ListenerController.AwsSnsMessageTypeHeaderName, NotificationModel.AwsSnsMessageTypeHeaderValue}
            }, jsonBody);

            var controller = new ListenerController(mockContextAccessor, MockLogger, new Mock<IListenerHttpClient>().Object);
            var result = await controller.PostAsync() as StatusCodeResult;

            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

        [Fact]
        public async Task ProcessSubscriptionConfirmationMessageType_Happy_SubscriptionConfirmation()
        {
            var jsonBody = await File.ReadAllTextAsync(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sample-subscription-confirmation.json")
            );

            var mockContextAccessor = await CreateMockContextAccessor(new Dictionary<string, string>
            {
                {HeaderNames.UserAgent, ListenerController.AwsSnsUserAgentHeaderValue},
                {ListenerController.AwsSnsMessageTypeHeaderName, SubscriptionConfirmationModel.AwsSnsMessageTypeHeaderValue}
            }, jsonBody);

            var mockClient = new Mock<IListenerHttpClient>();
            mockClient
                .Setup(x => x.GetAsync(It.IsAny<Uri>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("")
                })
                .Verifiable();

            var controller = new ListenerController(mockContextAccessor, MockLogger, mockClient.Object);
            var result = await controller.PostAsync() as StatusCodeResult;

            mockClient.Verify(x => x.GetAsync(It.IsAny<Uri>(), It.IsAny<CancellationToken>()));
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }


    }
}
