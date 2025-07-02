using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DbBackup.Core;
using Microsoft.Extensions.Logging;
using ZstdNet; // for optional compression/decompression

namespace DbBackup.Adapters
{
    /// <summary>
    /// Backs up a SQLite database by copying its entire file.
    /// Incremental backups are not supported here.
    /// </summary>
    public sealed class SqliteAdapter : DbBackup.Core.IDatabaseAdapter
    {
        public string EngineName => "sqlite";
        private readonly ILogger<SqliteAdapter> _log;

        public SqliteAdapter(ILogger<SqliteAdapter> log)
        {
            _log = log;
        }

        public async Task<Stream> CreateBackupAsync(BackupRequest req, CancellationToken ct)
        {
            if (req.Type != BackupType.Full)
            {
                throw new NotSupportedException("SQLite only supports full backups.");
            }

            // We expect req.Database to be the path to the .db file on disk:
            string sqliteFilePath = req.Database;
            if (!File.Exists(sqliteFilePath))
            {
                throw new FileNotFoundException($"SQLite file not found: {sqliteFilePath}");
            }

            _log.LogInformation("Creating full SQLite backup from {File}…", sqliteFilePath);

            // Simply copy the entire .db file into memory
            var mem = new MemoryStream();
            await using (var fs = File.OpenRead(sqliteFilePath))
            {
                await fs.CopyToAsync(mem, ct);
            }

            mem.Position = 0;
            return new MemoryStream(mem.ToArray());
        }

        public async Task RestoreAsync(RestoreRequest req, CancellationToken ct)
        {
            // Expecting req.SourcePath is the path to a .zst‐compressed SQLite backup
            string sqliteFilePath = req.Database; // where we want to restore the .db
            string backupZstPath = req.SourcePath;

            if (!File.Exists(backupZstPath))
                throw new FileNotFoundException($"Backup file not found: {backupZstPath}");

            _log.LogInformation("Restoring SQLite database to {File} from {Src}…", sqliteFilePath, backupZstPath);
            byte[] compressed = await File.ReadAllBytesAsync(backupZstPath, ct);
             byte[] raw = await Task.Run(() =>
        {
            using var decompressor = new Decompressor();
            return decompressor.Unwrap(compressed);
        }, ct);

            // 1) Decompress .zst backup to a temporary file
            string tempDb = Path.GetTempFileName();
        await File.WriteAllBytesAsync(tempDb, raw, ct);

            // 2) Overwrite the target SQLite file
            //    (If it already exists, delete or overwrite)
            if (File.Exists(sqliteFilePath))
            {
                File.Delete(sqliteFilePath);
            }

            File.Move(tempDb, sqliteFilePath);
            _log.LogInformation("SQLite restore complete.");
        }

        /// <summary>
        /// Decompress a .zst file into a temporary path (raw .db bytes).
        /// </summary>
        private static string DecompressZstdToTemp(string zstPath)
        {
            using var decompressor = new Decompressor();
            byte[] raw = decompressor.Unwrap(File.ReadAllBytes(zstPath));
            string tmpFile = Path.GetTempFileName();
            File.WriteAllBytes(tmpFile, raw);
            return tmpFile;
        }
    }
}
