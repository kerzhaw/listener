using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace listener.Controllers
{
    [Route("api/[controller]")]
    public class ListenerController : Controller
    {
        public async Task<IActionResult> GetAsync(CancellationToken ct = default(CancellationToken))
        {
            await Task.CompletedTask;
            return Ok();
        }

        private async Task<IActionResult> ProcessSubscriptionConfirmationMessageType()
        {
            await Task.CompletedTask;
            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync(CancellationToken ct = default(CancellationToken))
        {
            if (!Request.Headers.TryGetValue("x-amz-sns-message-type", out var messageType))
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
