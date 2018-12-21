using System.Collections.Generic;
using System.Threading.Tasks;

using Xunit;
using Amazon.Lambda.TestUtilities;
using Amazon.Lambda.SQSEvents;
using Newtonsoft.Json;

namespace listener.Tests
{
    public class FunctionTest
    {
        [Fact]
        public async Task DeserializesMessage()
        {

            var logger = new TestLambdaLogger();
            var context = new TestLambdaContext
            {
                Logger = logger
            };

            var emailMessage = new EmailMessage
            {
                TemplateData = "{ \"name\":\"Alejandro\" }",
                TemplateName = "Template1",
                From = "paul@kerzhaw.com",
                To = new List<string>{
                    "paul@kerzhaw.com"
                }
            };

            var sqsEvent = new SQSEvent
            {
                Records = new List<SQSEvent.SQSMessage>
                {
                    new SQSEvent.SQSMessage
                    {
                        Body = JsonConvert.SerializeObject(emailMessage)
                    }
                }
            };

            var function = new Function();
            await function.FunctionHandler(sqsEvent, context);
        }

        [Fact]
        public async Task TestSQSEventLambdaFunction()
        {
            var sqsEvent = new SQSEvent
            {
                Records = new List<SQSEvent.SQSMessage>
                {
                    new SQSEvent.SQSMessage
                    {
                        Body = "foobar"
                    }
                }
            };

            var logger = new TestLambdaLogger();
            var context = new TestLambdaContext
            {
                Logger = logger
            };

            var function = new Function();
            await function.FunctionHandler(sqsEvent, context);

            Assert.Contains("Processed message foobar", logger.Buffer.ToString());
        }
    }
}
