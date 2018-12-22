using System;
using System.Threading;
using System.Threading.Tasks;
using listener.Clients;
using listener.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace listener.Processors
{
    public class SubscriptionConfirmationProcessor : Processor, IProcessor
    {
        private readonly IListenerHttpClient _listenerHttpClient;
        private readonly ILogger<SubscriptionConfirmationProcessor> _logger;

        public SubscriptionConfirmationProcessor(
            IHttpContextAccessor contextAccessor,
            IListenerHttpClient listenerHttpClient,
            ILogger<SubscriptionConfirmationProcessor> logger)
            : base(contextAccessor)
        {
            _listenerHttpClient = listenerHttpClient;
            _logger = logger;
        }

        public string ProcessorType => SubscriptionConfirmationModel.AwsSnsMessageTypeHeaderValue;

        public async Task<IActionResult> Process(CancellationToken ct)
        {
            var model = await ReadModelAsync<SubscriptionConfirmationModel>(ct);

            try
            {
                var response = await _listenerHttpClient.GetAsync(model.SubscribeURL, ct);

                if (!response.IsSuccessStatusCode)
                    return new BadRequestResult();

                var content = response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Subscription confirmed: {content}");

                return new OkResult();
            }
            catch (Exception)
            {
                return new BadRequestResult();
            }
        }
    }
}