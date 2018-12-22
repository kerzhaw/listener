using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace listener.Processors
{
    public interface IProcessor
    {
        string ProcessorType { get; }

        Task<IActionResult> Process(CancellationToken ct);
    }
}