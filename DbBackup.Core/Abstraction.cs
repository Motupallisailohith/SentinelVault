namespace DbBackup.Core;

public interface IDatabaseAdapter
{
     string EngineName { get; }
    Task<Stream> CreateBackupAsync(BackupRequest request, CancellationToken ct);
     Task RestoreAsync(RestoreRequest request, CancellationToken ct);
}



