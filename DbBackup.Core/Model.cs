namespace DbBackup.Core;

public record BackupResult(
    bool   Success,
    string FilePath,
    long   BytesWritten,
    TimeSpan Duration,
    string? Error = null
);

public enum BackupType
{
    Full,
    Incremental,
    Differential
}

public record BackupManifest(
    string Engine,
    string Database,
    BackupType Type,
    DateTime UtcTimestamp,
    long? BinlogStart,
    long? BinlogEnd,
    string ParentFile,
    string FileName,
    long? SizeBytes
);

public interface IProfileConfigStore
{
    BackupProfile? Get(Guid id);
    Task<BackupProfile?> GetAsync(string id, CancellationToken ct = default);
    Task<IEnumerable<BackupProfile>> ListAsync(CancellationToken ct = default);
    Task AddAsync(BackupProfile p, CancellationToken ct = default);
    Task UpdateAsync(BackupProfile p, CancellationToken ct = default);
    Task DeleteAsync(BackupProfile p, CancellationToken ct = default);
}
