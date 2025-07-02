using System.Threading;
using System.Threading.Tasks;
using DbBackup.WebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace DbBackup.WebApi.Controllers
{
    [ApiController]
    [Route("api/ai")]
    public class AiController : ControllerBase
    {
        private readonly IAiExplanationService _ai;

        public AiController(IAiExplanationService ai) => _ai = ai;

        // GET /api/ai/explain/{runId}
        [HttpGet("explain/{runId}")]
        public async Task<IActionResult> Explain(string runId, CancellationToken ct)
        {
            var explanation = await _ai.ExplainLogAsync(runId, ct);
            return Ok(new { runId, explanation });
        }
    }
}
