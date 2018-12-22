using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using listener.Clients;
using listener.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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

        public ListenerController(
            IHttpContextAccessor contextAccessor,
            ILogger<ListenerController> logger,
            IListenerHttpClient httpClient)
        {
            _contextAccessor = contextAccessor;
            _logger = logger;
            _httpClient = httpClient;
        }

        private async Task<TModel> ReadModelAsync<TModel>(CancellationToken ct)
            where TModel : class
        {
            using (var streamReader = new StreamReader(_contextAccessor.HttpContext.Request.Body))
            {
                var message = await streamReader.ReadToEndAsync();
                Debug.Print(message);
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

            return Ok();
        }


        [HttpPost]
        public async Task<IActionResult> PostAsync(CancellationToken ct = default(CancellationToken))
        {
            var request = _contextAccessor.HttpContext.Request;

            if (!request.Headers.TryGetValue(HeaderNames.UserAgent, out var userAgent))
                return BadRequest();

            if (!userAgent.Equals(AwsSnsUserAgentHeaderValue))
                return BadRequest();

            if (!request.Headers.TryGetValue(AwsSnsMessageTypeHeaderName, out var messageType))
                return BadRequest();

            if (messageType.Equals(SubscriptionConfirmationModel.AwsSnsMessageTypeHeaderValue))
            {
                // SubscriptionConfirmation
                return await ProcessSubscriptionConfirmationMessageType(ct);
            }

            if (messageType.Equals(NotificationModel.AwsSnsMessageTypeHeaderValue))
            {
                // Notification
                return await ProcessNotificationMessageType(ct);
            }

            return BadRequest();
        }
    }
}
