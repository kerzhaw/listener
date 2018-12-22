using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using listener.Clients;
using listener.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;

namespace listener.Controllers
{
    [Route("api/[controller]")]
    public class ListenerController : Controller
    {
        public const string AwsSnsMessageTypeHeaderName = "x-amz-sns-message-type";
        public const string AwsSnsUserAgentHeaderValue = "Amazon Simple Notification Service Agent";

        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ILogger<ListenerController> _logger;
        private readonly IListenerHttpClient _httpClient;
        private readonly IAmazonSimpleEmailService _sesEmailClient;

        public ListenerController(
            IHttpContextAccessor contextAccessor,
            ILogger<ListenerController> logger,
            IListenerHttpClient httpClient,
            IAmazonSimpleEmailService sesEmailClient)
        {
            _contextAccessor = contextAccessor;
            _logger = logger;
            _httpClient = httpClient;
            _sesEmailClient = sesEmailClient;
        }

        private async Task<TModel> ReadModelAsync<TModel>(CancellationToken ct)
            where TModel : class
        {
            using (var streamReader = new StreamReader(_contextAccessor.HttpContext.Request.Body))
            {
                var message = await streamReader.ReadToEndAsync();
                return JsonConvert.DeserializeObject<TModel>(message);
            }
        }

        private async Task<IActionResult> ProcessSubscriptionConfirmationMessageType(CancellationToken ct)
        {
            var model = await ReadModelAsync<SubscriptionConfirmationModel>(ct);

            try
            {
                var response = await _httpClient.GetAsync(model.SubscribeURL, ct);

                if (!response.IsSuccessStatusCode)
                    return BadRequest();

                var content = response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Subscription confirmed: {content}");

                return Ok();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        private async Task<IActionResult> ProcessNotificationMessageType(CancellationToken ct)
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

            return Ok();
        }

        private bool ValidRequest(out StringValues messageType)
        {
            messageType = StringValues.Empty;
            var request = _contextAccessor.HttpContext.Request;

            if (!request.Headers.TryGetValue(HeaderNames.UserAgent, out var userAgent))
                return false;

            if (!userAgent.Equals(AwsSnsUserAgentHeaderValue))
                return false;

            if (!request.Headers.TryGetValue(AwsSnsMessageTypeHeaderName, out messageType))
                return false;

            return true;
        }


        [HttpPost]
        public async Task<IActionResult> PostAsync(CancellationToken ct = default(CancellationToken))
        {
            var request = _contextAccessor.HttpContext.Request;

            if (!ValidRequest(out var messageType))
                return BadRequest();

            // SubscriptionConfirmation
            if (messageType.Equals(SubscriptionConfirmationModel.AwsSnsMessageTypeHeaderValue))
                return await ProcessSubscriptionConfirmationMessageType(ct);

            // Notification
            if (messageType.Equals(NotificationModel.AwsSnsMessageTypeHeaderValue))
                return await ProcessNotificationMessageType(ct);

            return BadRequest();
        }
    }
}
