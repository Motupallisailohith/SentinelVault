// DbBackup.WebApi/Services/EfProfileConfigStore.cs
using DbBackup.Core;
using DbBackup.WebApi.Data;
using Microsoft.EntityFrameworkCore;
using DbBackup.WebApi.Extensions;

public sealed class EfProfileConfigStore : IProfileConfigStore
{
    private readonly BackupConfigContext _db;
    public EfProfileConfigStore(BackupConfigContext db) => _db = db;

    public BackupProfile? Get(Guid id)
    {
        var profile = _db.BackupProfiles
            .Where(p => p.Id == id)
            .FirstOrDefault();
        return profile?.ToCoreBackupProfile();
    }

    public async Task<BackupProfile?> GetAsync(string id, CancellationToken ct = default)
    {
        var profile = await _db.BackupProfiles.FindAsync(new object?[] { id }, ct);
        return profile?.ToCoreBackupProfile();
    }

    public async Task<IEnumerable<BackupProfile>> ListAsync(CancellationToken ct = default)
    {
        var profiles = await _db.BackupProfiles.AsNoTracking().ToListAsync(ct);
        return profiles.Select(p => p.ToCoreBackupProfile());
    }

    public async Task AddAsync(BackupProfile p, CancellationToken ct = default)
    {
        var profile = new DbBackup.WebApi.Models.BackupProfile
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Engine = p.Engine,
            Host = p.Host,
            Port = p.Port,
            Username = p.Username,
            Password = p.Password,
            Database = p.Database,
            OutPath = p.OutPath,
            Compression = p.Compression,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        };
        _db.BackupProfiles.Add(profile);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(BackupProfile p, CancellationToken ct = default)
    {
        var profile = await _db.BackupProfiles.FindAsync(new object?[] { p.Id.ToString() }, ct);
        if (profile == null) return;

        profile.Name = p.Name;
        profile.Description = p.Description;
        profile.Engine = p.Engine;
        profile.Host = p.Host;
        profile.Port = p.Port;
        profile.Username = p.Username;
        profile.Password = p.Password;
        profile.Database = p.Database;
        profile.OutPath = p.OutPath;
        profile.Compression = p.Compression;
        profile.UpdatedAt = p.UpdatedAt;

        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(BackupProfile p, CancellationToken ct = default)
    {
        var profile = await _db.BackupProfiles.FindAsync(new object?[] { p.Id.ToString() }, ct);
        if (profile == null) return;
        _db.BackupProfiles.Remove(profile);
        await _db.SaveChangesAsync(ct);
    }
}
