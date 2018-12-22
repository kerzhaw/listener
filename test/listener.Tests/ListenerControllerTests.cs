using System.Collections.Generic;
using System.Threading.Tasks;
using listener.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moq;
using Xunit;

namespace listener.Tests
{
    public class ListenerControllerTests
    {

        private IHttpContextAccessor CreateMockContextAccessor(IDictionary<string, string> headers)
        {
            var contextAccessor = new Mock<IHttpContextAccessor>();
            var context = new Mock<HttpContext>();
            var request = new Mock<HttpRequest>();
            var headerDictionary = new HeaderDictionary();

            foreach (var key in headers.Keys)
                headerDictionary.Add(key, new StringValues(headers[key]));

            request.SetupGet(x => x.Headers).Returns(headerDictionary);
            context.SetupGet(x => x.Request).Returns(request.Object);
            contextAccessor.SetupGet(x => x.HttpContext).Returns(context.Object);

            return contextAccessor.Object;
        }

        [Fact]
        public async Task ProcessSubscriptionConfirmationMessageType_Happy()
        {
            var mockContextAccessor = CreateMockContextAccessor(new Dictionary<string, string>
            {
                {"x-amz-sns-message-type","SubscriptionConfirmation"}
            });

            var controller = new ListenerController(mockContextAccessor);

            await controller.PostAsync();
        }


    }
}
