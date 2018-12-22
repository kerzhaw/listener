using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using listener.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace listener.Processors
{
    public class NotificationProcessor : Processor, IProcessor
    {
        private readonly ILogger<NotificationProcessor> _logger;
        private readonly IAmazonSimpleEmailService _sesEmailClient;

        public NotificationProcessor(
            IHttpContextAccessor contextAccessor,
            ILogger<NotificationProcessor> logger,
            IAmazonSimpleEmailService sesEmailClient)
            : base(contextAccessor)
        {
            _logger = logger;
            _sesEmailClient = sesEmailClient;
        }

        public string ProcessorType => NotificationModel.AwsSnsMessageTypeHeaderValue;

        public async Task<IActionResult> Process(CancellationToken ct)
        {
            var model = await ReadModelAsync<NotificationModel>(ct);
            var emailMessageModel = JsonConvert.DeserializeObject<EmailMessageModel>(model.Message);

            _logger.LogInformation($"Message received: {model.Message}");

            var request = new SendTemplatedEmailRequest
            {
                ConfigurationSetName = "MailConfigurationSet",
                Destination = new Destination(emailMessageModel.To),
                Source = emailMessageModel.From,
                Template = emailMessageModel.TemplateName,
                TemplateData = emailMessageModel.TemplateData
            };

            await _sesEmailClient.SendTemplatedEmailAsync(request, ct);

            _logger.LogInformation($"Email sent: {model.Message}");

            return new OkResult();
        }
    }
}