using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace listener.Controllers
{
    [Route("api/[controller]")]
    public class ListenerController : Controller
    {
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

        private async Task<IActionResult> ProcessSubscriptionConfirmationMessageType()
        {
            await Task.CompletedTask;
            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync(CancellationToken ct = default(CancellationToken))
        {
            var request = _contextAccessor.HttpContext.Request;

            if (!request.Headers.TryGetValue("x-amz-sns-message-type", out var messageType))
                return BadRequest();

            if (messageType.Equals("SubscriptionConfirmation"))
            {
                // SubscriptionConfirmation
                return await ProcessSubscriptionConfirmationMessageType();
            }

            return BadRequest();
        }
    }
}
