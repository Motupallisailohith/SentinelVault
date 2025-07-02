using System.Threading;
using System.Threading.Tasks;
using DbBackup.Core;
using DbBackup.WebApi.Data;
using DbBackup.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DbBackup.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RestoresController : ControllerBase
    {
        private readonly BackupConfigContext _db;
        private readonly BackupOrchestrator _orchestrator;

        public RestoresController(
            BackupConfigContext db,
            BackupOrchestrator orchestrator)
        {
            _db           = db;
            _orchestrator = orchestrator;
        }

        // POST /api/restores
        [HttpPost]
        public async Task<IActionResult> Restore(RestoreDto dto, CancellationToken ct)
        {
            var p = await _db.BackupProfiles.AsNoTracking().FirstOrDefaultAsync(
                x => x.Id.ToString() == dto.ProfileId, ct);
            
            if (p is null) return NotFound();

            var req = new RestoreRequest(
                Engine:      p.Engine,
                Host:        p.Host,
                Port:        p.Port,
                User:        p.Username,
                Password:    p.Password,
                Database:    p.Database,
                SourcePath:  dto.SourceFileName
            );

            try
            {
                await _orchestrator.RestoreAsync(req, ct);
                return Ok(new { message = "Restore completed successfully" });
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message);
            }
        }
    }
}
