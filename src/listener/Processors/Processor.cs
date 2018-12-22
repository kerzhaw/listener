using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace listener.Processors
{
    public abstract class Processor
    {
        protected readonly IHttpContextAccessor ContextAccessor;

        protected Processor(IHttpContextAccessor contextAccessor)
        {
            ContextAccessor = contextAccessor;
        }

        protected async Task<TModel> ReadModelAsync<TModel>(CancellationToken ct)
            where TModel : class
        {
            using (var streamReader = new StreamReader(ContextAccessor.HttpContext.Request.Body))
            {
                var message = await streamReader.ReadToEndAsync();
                return JsonConvert.DeserializeObject<TModel>(message);
            }
        }
    }
}