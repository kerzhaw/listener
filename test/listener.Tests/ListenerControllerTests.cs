using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using listener.Controllers;
using listener.Clients;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Moq;
using System.Threading;
using System.Net;
using System.Net.Http;
using listener.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;

namespace listener.Tests
{
    [TestClass]
    public class ListenerControllerTests : ListenerControllerTestsBase
    {
        [TestMethod, TestCategory("Validation")]
        public async Task PostAsync_Returns_400_For_Bad_UserAgent()
        {
            var mockContextAccessor = await CreateMockContextAccessor(new Dictionary<string, string>
            {
                {HeaderNames.UserAgent,"Dis a bad user agent mon"}
            });

            var controller = CreateController(mockContextAccessor);
            var result = await controller.PostAsync() as StatusCodeResult;

            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
        }

        [TestMethod, TestCategory("Validation")]
        public async Task PostAsync_Returns_400_For_Missing_UserAgent()
        {
            var mockContextAccessor = await CreateMockContextAccessor(new Dictionary<string, string>());

            var controller = CreateController(mockContextAccessor);
            var result = await controller.PostAsync() as StatusCodeResult;

            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
        }

        [TestMethod, TestCategory("Validation")]
        public async Task PostAsync_Returns_400_For_Missing_MessageType_Header()
        {
            var mockContextAccessor = await CreateMockContextAccessor(new Dictionary<string, string>{
                {HeaderNames.UserAgent, ListenerController.AwsSnsUserAgentHeaderValue}
            });

            var controller = CreateController(mockContextAccessor);
            var result = await controller.PostAsync() as StatusCodeResult;

            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
        }

        [TestMethod, TestCategory("Validation")]
        public async Task PostAsync_Returns_400_For_Bad_MessageType_Header()
        {
            var mockContextAccessor = await CreateMockContextAccessor(new Dictionary<string, string>{
                {HeaderNames.UserAgent, ListenerController.AwsSnsUserAgentHeaderValue},
                {ListenerController.AwsSnsMessageTypeHeaderName, "Bad value"}
            });

            var controller = CreateController(mockContextAccessor);
            var result = await controller.PostAsync() as StatusCodeResult;

            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
        }

        [TestMethod, TestCategory("Notification")]
        public async Task Notification_Happy()
        {
            var jsonBody = await File.ReadAllTextAsync(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sample-notification.json")
            );

            var mockContextAccessor = await CreateMockContextAccessor(new Dictionary<string, string>
            {
                {HeaderNames.UserAgent, ListenerController.AwsSnsUserAgentHeaderValue},
                {ListenerController.AwsSnsMessageTypeHeaderName, NotificationModel.AwsSnsMessageTypeHeaderValue}
            }, jsonBody);

            var mockSesClient = new Mock<IAmazonSimpleEmailService>();

            var controller = new ListenerController(mockContextAccessor, MockLogger, MockHttpClient, mockSesClient.Object);
            var result = await controller.PostAsync() as StatusCodeResult;

            mockSesClient.Verify(x => x.SendTemplatedEmailAsync(It.IsAny<SendTemplatedEmailRequest>(), It.IsAny<CancellationToken>()));
            Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);
        }

        [TestMethod, TestCategory("SubscriptionConfirmation")]
        public async Task SubscriptionConfirmation_Happy()
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

            var controller = CreateController(mockContextAccessor, mockClient.Object);
            var result = await controller.PostAsync() as StatusCodeResult;

            Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);
        }


    }
}
