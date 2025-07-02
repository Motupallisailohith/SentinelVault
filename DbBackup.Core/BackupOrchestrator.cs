// DbBackup.Core/BackupOrchestrator.cs
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DbBackup.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DbBackup.Core
{
    public interface IStorageProvider
    {
        Task<string> WriteAsync(Stream data, BackupRequest req, CancellationToken ct);
        Task<Stream> ReadLogAsync(string runId, CancellationToken ct);
        Task<IEnumerable<BackupManifest>> ListBackupsAsync(string profileId, CancellationToken ct);
        Task<Stream> ReadAsync(string fileName, CancellationToken ct);
    }
}

namespace DbBackup.Core
{
    public class BackupOrchestrator
    {
        private readonly IReadOnlyDictionary<string, IDatabaseAdapter> _adapters;
        private readonly ICompressor            _compress;
        private readonly IStorageProvider _store;
        private readonly ILogger<BackupOrchestrator> _log;
        private readonly IProfileConfigStore    _profileConfig;
        private readonly BackupOptions          _opts;

        public BackupOrchestrator(
            IEnumerable<IDatabaseAdapter> adapters,
            ICompressor compress,
            IStorageProvider store,
            IProfileConfigStore profileConfig,
            IOptions<BackupOptions> opts,
            ILogger<BackupOrchestrator> log)
        {
            _adapters      = adapters.ToDictionary(a => a.EngineName, StringComparer.OrdinalIgnoreCase);
            _compress      = compress;
            _store         = store;
            _profileConfig = profileConfig;
            _opts          = opts.Value;
            _log           = log;
        }

        public async Task<BackupResult> RunAsync(BackupRequest req, CancellationToken ct)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                _log.LogInformation("Starting backup of {Db} ({Engine})…",
                                     req.Database, req.Engine);

                if (!_adapters.TryGetValue(req.Engine, out var adapter))
                    throw new NotSupportedException($"Engine '{req.Engine}' not supported");

                await using var dump       = await adapter.CreateBackupAsync(req, ct);
                await using var compressed = await _compress.CompressAsync(dump, "zstd", ct);
                var path = await _store.WriteAsync(compressed, req, ct);

                var result = new BackupResult(
                    Success: true,
                    FilePath: path,
                    BytesWritten: compressed.Length,
                    Duration: sw.Elapsed,
                    Error: null);

                _log.LogInformation("Backup of {Db} completed in {Duration:hh::mm::ss}, " +
                                   "stored at {Path} (zstd)",
                                   req.Database, sw.Elapsed, path);

                return result;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Backup failed: {Message}", ex.Message);
                return new BackupResult(
                    Success: false,
                    FilePath: "",
                    BytesWritten: 0,
                    Duration: sw.Elapsed,
                    Error: ex.Message);
            }
        }

        public async Task RestoreAsync(RestoreRequest req, CancellationToken ct)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                _log.LogInformation("Starting restore of {Db} from {Src} ({Engine})…",
                                     req.Database, req.SourcePath, req.Engine);

                if (!_adapters.TryGetValue(req.Engine, out var adapter))
                    throw new NotSupportedException($"Engine '{req.Engine}' not supported");

                await adapter.RestoreAsync(req, ct);

                sw.Stop();
                _log.LogInformation("Restore finished ({Ms} ms)", sw.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                sw.Stop();
                _log.LogError(ex, "Restore failed after {Ms} ms", sw.ElapsedMilliseconds);
                throw;
            }
        }

        // ─── NEW: list all .zst files under profileId ───────────────────────────────
        public async Task<List<BackupManifest>> ListBackupsAsync(string profileId, CancellationToken ct)
        {
            var manifests = await _store.ListBackupsAsync(profileId, ct);
            return manifests.ToList();
        }

        // ─── NEW: start by profile directly ─────────────────────────────────────────
        public Task<BackupResult> StartFullAsync(string profileId, CancellationToken ct)
            => StartAsync(profileId, BackupType.Full, ct);

        public Task<BackupResult> StartIncrementalAsync(string profileId, CancellationToken ct)
            => StartAsync(profileId, BackupType.Incremental, ct);

        private async Task<BackupResult> StartAsync(string profileId, BackupType type, CancellationToken ct)
        {
            var cfg = await _profileConfig.GetAsync(profileId, ct)
                ?? throw new InvalidOperationException($"Unknown profile '{profileId}'");

            var req = new BackupRequest
            {
                Engine = cfg.Engine,
                Database = cfg.Database,
                Host = cfg.Host,
                Port = cfg.Port,
                User = cfg.Username,
                Password = cfg.Password,
                ProfileId = cfg.Id.ToString()
            };

            return await RunAsync(req, ct);
        }
    }
}
