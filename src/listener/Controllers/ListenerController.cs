﻿using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace listener.Controllers
{
    [Route("api/[controller]")]
    public class ListenerController : Controller
    {
        public const string AwsSnsMessageTypeHeaderName = "x-amz-sns-message-type";
        public const string AwsSnsMessageTypeHeaderValueSubscriptionConfirmation = "SubscriptionConfirmation";
        public const string AwsSnsUserAgentHeaderValue = "Amazon Simple Notification Service Agent";

        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ILogger<ListenerController> _logger;

        public ListenerController(IHttpContextAccessor contextAccessor, ILogger<ListenerController> logger)
        {
            _contextAccessor = contextAccessor;
            _logger = logger;
        }

        public async Task<IActionResult> GetAsync(CancellationToken ct = default(CancellationToken))
        {
            _logger.LogInformation("Just letting you know this works!");
            await Task.CompletedTask;
            return Ok("It works");
        }

        private async Task<IActionResult> ProcessSubscriptionConfirmationMessageType(CancellationToken ct = default(CancellationToken))
        {
            await Task.CompletedTask;
            return BadRequest();
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

            if (messageType.Equals(AwsSnsMessageTypeHeaderValueSubscriptionConfirmation))
            {
                // SubscriptionConfirmation
                return await ProcessSubscriptionConfirmationMessageType(ct);
            }

            return BadRequest();
        }
    }
}
