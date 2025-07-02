using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DbBackup.Core;
using DbBackup.WebApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DbBackup.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BackupsController : ControllerBase
    {
        private readonly BackupConfigContext _db;
        private readonly BackupOrchestrator  _orchestrator;
        private readonly IStorageProvider _storage;

        public BackupsController(
            BackupConfigContext db,
            BackupOrchestrator orchestrator,
            IStorageProvider storage)
        {
            _db           = db;
            _orchestrator = orchestrator;
            _storage      = storage;
        }

        // GET /api/backups/{profileId}
        [HttpGet("{profileId}")]
        public async Task<ActionResult<IEnumerable<BackupDto>>> List(string profileId, CancellationToken ct)
        {
            // Validate profile exists
            var profile = await _db.BackupProfiles.AsNoTracking()
                .FirstOrDefaultAsync(x => BackupConfigContext.CompareIds(x.IdString, profileId), ct);
            if (profile is null) return NotFound();

            // List backups
            var raw = await _storage.ListBackupsAsync(profileId, ct);

            // Convert to DTOs
            var dtos = raw.Select(r => new BackupDto
            {
                FileName     = r.FileName,
                UtcTimestamp = r.UtcTimestamp.ToString("O"),
                Type         = r.Type.ToString(),
                SizeBytes    = r.SizeBytes ?? 0
            });

            return Ok(dtos);
        }

        // GET /api/backups/{profileId}/history
        [HttpGet("{profileId}/history")]
        public async Task<ActionResult<IEnumerable<BackupHistoryDto>>> GetHistory(string profileId, CancellationToken ct)
        {
            var profile = await _db.BackupProfiles.AsNoTracking()
                .FirstOrDefaultAsync(x => BackupConfigContext.CompareIds(x.IdString, profileId), ct);
            if (profile is null) return NotFound();

            var raw = await _storage.ListBackupsAsync(profileId, ct);
            var history = raw
                .OrderByDescending(r => r.UtcTimestamp)
                .Select(r => new BackupHistoryDto
                {
                    Timestamp = r.UtcTimestamp,
                    Type = r.Type,
                    SizeBytes = r.SizeBytes ?? 0,
                    Success = true // We assume success since we have the backup
                });

            return Ok(history);
        }

        // POST /api/backups/{profileId}/full
        [HttpPost("{profileId}/full")]
        public async Task<IActionResult> RunFull(string profileId, CancellationToken ct)
        {
            var p = await _db.BackupProfiles.AsNoTracking().FirstOrDefaultAsync(x => BackupConfigContext.CompareIds(x.IdString, profileId), ct);
            if (p is null) return NotFound();

            var req = new BackupRequest(
                Engine:      p.Engine,
                Database:    p.Database,
                Host:        p.Host,
                Port:        p.Port,
                User:        p.Username,
                Password:    p.Password,
                ProfileId:   p.Id.ToString(),
                Type:        BackupType.Full
            );

            var result = await _orchestrator.RunAsync(req, ct);
            if (!result.Success)
                return Problem(detail: result.Error);

            return Ok(new { result.FilePath });
        }

        // POST /api/backups/{profileId}/incremental
        [HttpPost("{profileId}/incremental")]
        public async Task<IActionResult> RunIncremental(string profileId, CancellationToken ct)
        {
            var p = await _db.BackupProfiles.AsNoTracking().FirstOrDefaultAsync(x => BackupConfigContext.CompareIds(x.IdString, profileId), ct);
            if (p is null) return NotFound();

            var req = new BackupRequest(
                Engine:      p.Engine,
                Database:    p.Database,
                Host:        p.Host,
                Port:        p.Port,
                User:        p.Username,
                Password:    p.Password,
                ProfileId:   p.Id.ToString(),
                Type:        BackupType.Incremental
            );

            var result = await _orchestrator.RunAsync(req, ct);
            if (!result.Success)
                return Problem(detail: result.Error);

            return Ok(new { result.FilePath });
        }
    }

    public record BackupHistoryDto
    {
        public DateTime Timestamp { get; init; }
        public BackupType Type { get; init; }
        public long? SizeBytes { get; init; }
        public bool Success { get; init; }
    }

    public record BackupDto
    {
        public string   FileName     { get; init; } = default!;
        public string   UtcTimestamp { get; init; } = default!;
        public string   Type         { get; init; } = default!;
        public long     SizeBytes    { get; init; }
    }
}
