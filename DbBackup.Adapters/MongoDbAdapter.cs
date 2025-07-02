using System.Diagnostics;
using DbBackup.Core;
using Microsoft.Extensions.Logging;
using ZstdNet; // for decompression
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DbBackup.Adapters
{
    /// <summary>
    /// Uses mongodump / mongorestore to back up and restore a MongoDB database.
    /// - Full backup ⇒ mongodump --archive (BSON)
    /// - Restore      ⇒ mongorestore --archive
    /// </summary>
    public sealed class MongoDbAdapter : DbBackup.Core.IDatabaseAdapter
    {
         public string EngineName => "mongodb";
        private readonly ILogger<MongoDbAdapter> _log;

        public MongoDbAdapter(ILogger<MongoDbAdapter> log)
        {
            _log = log;
        }

        public async Task<Stream> CreateBackupAsync(BackupRequest req, CancellationToken ct)
        {
            if (req.Type != BackupType.Full)
            {
                throw new NotSupportedException("MongoDB only supports full backups at this time.");
            }

            _log.LogInformation("Running mongodump for MongoDB {Db}…", req.Database);

            // Prepare mongodump to write a single‐file archive to stdout
            // We assume `mongodump` is on PATH or resolved via MONGODUMP_PATH env var
            var psi = new ProcessStartInfo
            {
                FileName = ResolveTool("mongodump"),
                ArgumentList =
                {
                    $"--host={req.Host}",
                    $"--port={req.Port}",
                    $"--username={req.User}",
                    $"--password={req.Password}",
                    $"--db={req.Database}",
                    "--archive"         // write BSON archive to stdout
                },
                RedirectStandardOutput = true,
                RedirectStandardError  = true
            };

            using var proc = Process.Start(psi)
                ?? throw new InvalidOperationException("Failed to launch mongodump");

            // Copy everything from stdout into a MemoryStream
            var mem = new MemoryStream();
            await proc.StandardOutput.BaseStream.CopyToAsync(mem, ct);
            string err = await proc.StandardError.ReadToEndAsync(ct);
            await proc.WaitForExitAsync(ct);

            if (proc.ExitCode != 0)
            {
                mem.Dispose();
                throw new InvalidOperationException($"mongodump exited {proc.ExitCode}: {err}");
            }

            mem.Position = 0;
            return new MemoryStream(mem.ToArray()); // hand caller its own copy
        }

        public async Task RestoreAsync(RestoreRequest req, CancellationToken ct)
        {
            _log.LogInformation("Restoring MongoDB {Db} from {Src}…", req.Database, req.SourcePath);

            // We expect req.SourcePath to point to a .zst archive produced earlier.
            // 1) Decompress the .zst to a temporary BSON archive
            string tempArchive = DecompressZstdToTemp(req.SourcePath);

            // 2) Run mongorestore --archive=<tempArchive> --db=<db> --drop
            var psi = new ProcessStartInfo
            {
                FileName = ResolveTool("mongorestore"),
                ArgumentList =
                {
                    $"--host={req.Host}",
                    $"--port={req.Port}",
                    $"--username={req.User}",
                    $"--password={req.Password}",
                    $"--db={req.Database}",
                    "--archive=" + tempArchive,
                    "--drop"            // drop existing data before restoring
                },
                RedirectStandardOutput = true,
                RedirectStandardError  = true
            };

            using var proc = Process.Start(psi)
                ?? throw new InvalidOperationException("Failed to launch mongorestore");

            string stdOut = await proc.StandardOutput.ReadToEndAsync(ct);
            string stdErr = await proc.StandardError.ReadToEndAsync(ct);
            await proc.WaitForExitAsync(ct);

            // Clean up temp file
            File.Delete(tempArchive);

            if (proc.ExitCode != 0)
            {
                throw new InvalidOperationException($"mongorestore exited {proc.ExitCode}: {stdErr}");
            }

            _log.LogInformation("MongoDB restore completed.");
        }

        /// <summary>
        /// Resolves an executable name via environment‐variable override
        /// (e.g. MONGODUMP_PATH) or falls back to the plain binary name.
        /// </summary>
        private static string ResolveTool(string exe)
            => Environment.GetEnvironmentVariable(exe.ToUpperInvariant() + "_PATH") ?? exe;

        /// <summary>
        /// Decompress a .zst file into a temporary path and return that .archive path.
        /// </summary>
        private static string DecompressZstdToTemp(string zstPath)
        {
            using var decompressor = new Decompressor();
            byte[] rawBson = decompressor.Unwrap(File.ReadAllBytes(zstPath));
            string temp = Path.GetTempFileName();
            // Overwrite the temp filename so it has no extension conflicts:
            File.WriteAllBytes(temp, rawBson);
            return temp;
        }
    }
}
