using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DbBackup.Core;

namespace DbBackup.Adapters
{
    public interface IDatabaseAdapter
    {
        string EngineName { get; }
        Task<Stream> CreateBackupAsync(BackupRequest req, CancellationToken ct);
        Task RestoreAsync(RestoreRequest req, CancellationToken ct);
    }
}
