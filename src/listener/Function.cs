using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Newtonsoft.Json;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace listener
{
    public class EmailMessage
    {
        public string TemplateName { get; set; }
        public string TemplateData { get; set; }
        public List<string> To { get; set; }
        public string From { get; set; }
    }

    public class Function
    {
        private readonly IAmazonSimpleEmailService _sesEmailClient;

        /// <summary>
        /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
        /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
        /// region the Lambda function is executed in.
        /// </summary>
        public Function()
        {
            var regionSetting = Environment.GetEnvironmentVariable("SESRegion") ?? "us-east-1";
            var region = RegionEndpoint.GetBySystemName(regionSetting);

            _sesEmailClient = new AmazonSimpleEmailServiceClient(region);
        }


        /// <summary>
        /// This method is called for every Lambda invocation. This method takes in an SQS event object and can be used 
        /// to respond to SQS messages.
        /// </summary>
        /// <param name="evnt"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
        {
            var ct = new CancellationToken();

            foreach (var message in evnt.Records)
            {
                await ProcessMessageAsync(message, context, ct);
            }
        }

        private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context, CancellationToken ct)
        {
            var emailMessage = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<EmailMessage>(message.Body), ct);

            var request = new SendTemplatedEmailRequest
            {
                ConfigurationSetName = "MailConfigurationSet",
                Destination = new Destination(emailMessage.To),
                Source = emailMessage.From,
                Template = emailMessage.TemplateName,
                TemplateData = emailMessage.TemplateData
            };

            await _sesEmailClient.SendTemplatedEmailAsync(request, ct);
        }
    }
}
