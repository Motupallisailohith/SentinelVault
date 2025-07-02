using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DbBackup.WebApi.Models;
using DbBackup.WebApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DbBackup.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfilesController : ControllerBase
    {
        private readonly BackupConfigContext _db;

        public ProfilesController(BackupConfigContext db) => _db = db;

        // GET /api/profiles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BackupProfile>>> GetAll() =>
            await _db.BackupProfiles.AsNoTracking().ToListAsync();

        // POST /api/profiles
        [HttpPost]
        public async Task<ActionResult<BackupProfile>> Create(BackupProfile p)
        {
            p.Id        = Guid.NewGuid();
            p.CreatedAt = p.UpdatedAt = DateTime.UtcNow;

            _db.BackupProfiles.Add(p);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOne), new { id = p.Id }, p);
        }

        // GET /api/profiles/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<BackupProfile>> GetOne(string id)
        {
            var p = await _db.BackupProfiles.FindAsync(id);
            return p is null ? NotFound() : p;
        }

        // PUT /api/profiles/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, BackupProfile patch)
        {
            var p = await _db.BackupProfiles.FindAsync(id);
            if (p is null) return NotFound();

            p.Name        = patch.Name;
            p.Description = patch.Description;
            p.Engine      = patch.Engine;
            p.Host        = patch.Host;
            p.Port        = patch.Port;
            p.Username    = patch.Username;
            p.Password    = patch.Password;
            p.Database    = patch.Database;
            p.OutPath     = patch.OutPath;
            p.Compression = patch.Compression;
            p.UpdatedAt   = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // DELETE /api/profiles/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var p = await _db.BackupProfiles.FindAsync(id);
            if (p is null) return NotFound();

            _db.BackupProfiles.Remove(p);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
