using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using DbBackup.Core;
using DbBackup.Storage;
using Microsoft.Extensions.Logging;

namespace DbBackup.Adapters
{
    public sealed class MySqlAdapter : DbBackup.Core.IDatabaseAdapter
    {
        public string EngineName => "mysql";
        private readonly ILogger<MySqlAdapter> _log;
        private readonly JsonManifestStore      _manifests;
        private readonly IStorageProvider _store;

        public MySqlAdapter(
            ILogger<MySqlAdapter> log,
            JsonManifestStore manifests,
            IStorageProvider store
        )
        {
            _log = log;
            _manifests = manifests;
            _store = store;
        }

        public async Task<Stream> CreateBackupAsync(DbBackup.Core.BackupRequest req, CancellationToken ct)
        {
            switch (req.Type)
            {
                case DbBackup.Core.BackupType.Full:
                    return await DumpFullAsync(req, ct);
                case DbBackup.Core.BackupType.Incremental:
                    return await DumpIncrementalAsync(req, ct);
                default:
                    throw new NotSupportedException($"Backup type {req.Type} not supported"); // req.Type is already DbBackup.Core.BackupType
            }
        }

    //────────────────────────  FULL  ────────────────────────
    private async Task<Stream> DumpFullAsync(DbBackup.Core.BackupRequest req, CancellationToken ct)
    {
        _log.LogInformation("Running full mysqldump for {Db}", req.Database);

        var psi = new ProcessStartInfo
        {
            FileName = ResolveTool("mysqldump"),
            Arguments = $"--host={req.Host} --port={req.Port} --user={req.User} --password={req.Password} --single-transaction --routines {req.Database}",
            RedirectStandardOutput = true,
            RedirectStandardError  = true
        };

        using var proc = Process.Start(psi)
            ?? throw new InvalidOperationException("mysqldump launch failed");

        await using var mem = new MemoryStream();
        await proc.StandardOutput.BaseStream.CopyToAsync(mem, ct);
        string err = await proc.StandardError.ReadToEndAsync(ct);
        await proc.WaitForExitAsync(ct);

        if (proc.ExitCode != 0)
            throw new($"mysqldump exited {proc.ExitCode}: {err}");

        string currBinlogFile = await QueryCurrentBinlogFileAsync(req, ct);
        long   endPos         = await QueryCurrentBinlogPositionAsync(req, ct);

        var fileName = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") + ".zst";
        var sizeBytes = mem.Length;
        
        _manifests.SaveAsync(req.ProfileId, new DbBackup.Core.BackupManifest(
            Engine:       req.Engine,
            Database:     req.Database,
            Type:         DbBackup.Core.BackupType.Full,
            UtcTimestamp: DateTime.UtcNow,
            BinlogStart:  4,       // binlogs start at position 4 by convention
            BinlogEnd:    endPos,
            ParentFile:   "",      // full has no parent
            FileName:     fileName,
            SizeBytes:    sizeBytes
        )).Wait();

        mem.Position = 0;
        return new MemoryStream(mem.ToArray());
    }
        //──────────────────────  INCREMENTAL  ───────────────────
        private async Task<Stream> DumpIncrementalAsync(DbBackup.Core.BackupRequest req, CancellationToken ct)
        {
            var parent = _manifests.GetLatestFull(req.Database)
                ?? throw new InvalidOperationException(
                    "No full backup manifest found; cannot generate incremental.");

            _log.LogInformation(
                "Running incremental mysqlbinlog for {Db} starting at {Pos}",
                req.Database, parent.BinlogEnd);

            // Get current binlog file name
            string binlogFile = await QueryCurrentBinlogFileAsync(req, ct);
            if (string.IsNullOrWhiteSpace(binlogFile))
                throw new InvalidOperationException("Could not determine current binary log file.");

            var psi = new ProcessStartInfo
            {
                FileName = ResolveTool("mysqlbinlog"),
                Arguments = $"--read-from-remote-server --host={req.Host} --port={req.Port} --user={req.User} --password={req.Password} --start-position={parent.BinlogEnd} {binlogFile}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            using var proc = Process.Start(psi) ?? throw new InvalidOperationException("mysqlbinlog launch failed");

            using var mem = new MemoryStream();
            proc.StandardOutput.BaseStream.CopyTo(mem);
            string err = proc.StandardError.ReadToEnd();
            proc.WaitForExit();

            if (proc.ExitCode != 0)
                throw new Exception($"mysqlbinlog exited {proc.ExitCode}: {err}");

            long endPos = ExtractLastPosition(mem);

            var fileName = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") + ".zst";
            var sizeBytes = mem.Length;
            
            await _manifests.SaveAsync(req.ProfileId, new DbBackup.Core.BackupManifest(
                req.Engine,
                req.Database,
                DbBackup.Core.BackupType.Incremental,
                DateTime.UtcNow,
                parent.BinlogEnd,
                endPos,
                parent.FileName,
                fileName,
                sizeBytes
            ));

            mem.Position = 0;
            return new MemoryStream(mem.ToArray());
        }
        //──────────────────────  helpers  ───────────────────────
        private static string ResolveTool(string exe)
            => Environment.GetEnvironmentVariable(exe.ToUpperInvariant() + "_PATH") ?? exe;

        /// <summary>
        /// Verifies that we can connect to MySQL by doing a simple “SELECT 1”.
        /// Returns true if exit code == 0, false otherwise.
        /// </summary>
        public static async Task<bool> CanConnectAsync(
            string host, int port, string user, string pwd, string db)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = ResolveTool("mysql"),
                    ArgumentList =
                    {
                        $"--host={host}",
                        $"--port={port}",
                        $"--user={user}",
                        $"--password={pwd}",
                        "-e", "SELECT 1",
                        db
                    },
                    RedirectStandardOutput = true,
                    RedirectStandardError  = true
                };
                using var proc = Process.Start(psi)!;
                await proc.WaitForExitAsync();
                return proc.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Runs “SHOW BINARY LOG STATUS” on the server and returns the current binlog filename.
        /// </summary>
        private static async Task<string> QueryCurrentBinlogFileAsync(
            DbBackup.Core.BackupRequest req, CancellationToken ct)
        {
            var psi = new ProcessStartInfo
            {
                FileName = ResolveTool("mysql"),
                ArgumentList =
                {
                    $"--host={req.Host}",
                    $"--port={req.Port}",
                    $"--user={req.User}",
                    $"--password={req.Password}",
                    "-N",                            // skip column headers
                    "-e", "SHOW BINARY LOG STATUS"   // returns “File<TAB>Position<…>”
                },
                RedirectStandardOutput = true,
                RedirectStandardError  = true
            };

            using var proc = Process.Start(psi)!;
            string output = await proc.StandardOutput.ReadToEndAsync(ct);
            string error  = await proc.StandardError.ReadToEndAsync(ct);
            await proc.WaitForExitAsync(ct);

            if (proc.ExitCode != 0)
                throw new InvalidOperationException(
                    $"SHOW BINARY LOG STATUS failed ({proc.ExitCode}): {error}");

            // Parse just the first column of the first line:
            var lines = output
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length == 0) return "";

            var parts = lines[0].Split('\t', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length > 0 ? parts[0] : "";
        }

        /// <summary>
        /// Runs “SHOW BINARY LOG STATUS” (no trailing “;”) on the server and returns the Position column.
        /// </summary>
        private static async Task<long> QueryCurrentBinlogPositionAsync(
            DbBackup.Core.BackupRequest req, CancellationToken ct)
        {
            var psi = new ProcessStartInfo
            {
                FileName = ResolveTool("mysql"),
                ArgumentList =
                {
                    $"--host={req.Host}",
                    $"--port={req.Port}",
                    $"--user={req.User}",
                    $"--password={req.Password}",
                    "-N",                           // skip column headers
                    "-e", "SHOW BINARY LOG STATUS"  // no semicolon
                },
                RedirectStandardOutput = true,
                RedirectStandardError  = true
            };

            using var proc = Process.Start(psi)!;
            string output = await proc.StandardOutput.ReadToEndAsync(ct);
            string error  = await proc.StandardError.ReadToEndAsync(ct);
            await proc.WaitForExitAsync(ct);

            if (proc.ExitCode != 0)
                throw new InvalidOperationException(
                    $"SHOW BINARY LOG STATUS failed ({proc.ExitCode}): {error}");

            var lines = output
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length == 0) return 0L;

            // Each line is “<File>\t<Position>\t…”. We want the second field
            var parts = lines[0].Split('\t', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2 || !long.TryParse(parts[1], out long pos))
                return 0L;

            return pos;
        }

        /// <summary>
        /// Reads “# at position N” markers from the last few KB of a binlog stream
        /// to find the final Position number.
        /// </summary>
        private static long ExtractLastPosition(Stream binlogStream)
        {
            binlogStream.Position = Math.Max(0, binlogStream.Length - 4096);
            using var reader = new StreamReader(binlogStream, leaveOpen: true);
            string tail = reader.ReadToEnd();
            var matches = Regex.Matches(tail, @"#\sat\sposition\s(\d+)");
            return matches.Count > 0
                ? long.Parse(matches[^1].Groups[1].Value)
                : 0;
        }

        //──────────────────────  RESTORE  ───────────────────────

        public async Task RestoreAsync(RestoreRequest req, CancellationToken ct)
        {
            // 1️⃣ Build ordered chain: full-dump first, then each incremental ≤ chosen file
            var chain = ResolveRestoreChain(req);

            _log.LogInformation("Restoring {Db} using {Count} file(s)…", req.Database, chain.Count);

            // 2️⃣ Apply full SQL dump
            await ApplySqlFileAsync(chain[0], req, ct);

            // 3️⃣ Apply each incremental binlog in order
            foreach (var inc in chain.Skip(1))
                await ApplyBinlogAsync(inc, req, ct);

            _log.LogInformation("Restore chain complete.");
        }

        async Task ApplySqlFileAsync(string sqlFile, RestoreRequest req, CancellationToken ct)
        {
            var psi = new ProcessStartInfo
            {
                FileName = ResolveTool("mysql"),
                Arguments = $"-h {req.Host} -P {req.Port} -u {req.User} -p{req.Password} {req.Database}",
                RedirectStandardInput = true
            };

            using var mysql = Process.Start(psi)!
                .WithRedirectedOutput(_log, "mysql");

            // 2) Read the full backup from storage:
            using var dump = await _store.ReadAsync(sqlFile, ct);
            await dump.CopyToAsync(mysql.StandardInput.BaseStream, ct);
            await mysql.StandardInput.BaseStream.FlushAsync(ct);
            mysql.StandardInput.Close();

            await mysql.WaitForExitAsync(ct);
            var exitCode = mysql.ExitCode;
            if (exitCode != 0)
                throw new InvalidOperationException($"mysql exited with {exitCode}");
        }

        List<string> ResolveRestoreChain(RestoreRequest req)
        {
            // 1) Load the manifest for the file the user passed
            var chosen = _manifests.FromFileName(req.SourcePath)
                ?? throw new InvalidOperationException("Manifest not found for " + req.SourcePath);

            // 2) Always need the parent full manifest first
            var full = chosen.Type == DbBackup.Core.BackupType.Full
                ? chosen
                : _manifests.GetLatestFull(chosen.Database)
                    ?? throw new InvalidOperationException("Parent full backup not found");

            var list = new List<string>
            {
                Path.ChangeExtension(full.UtcTimestampFileNameStem(), ".zst")
            };

            if (chosen.Type == DbBackup.Core.BackupType.Incremental)
            {
                // 3) Add any incrementals between full.BinlogEnd and chosen.BinlogEnd
                list.AddRange(_manifests
                    .GetIncrementals(chosen.Database,
                                     full.BinlogEnd!.Value,
                                     chosen.BinlogEnd!.Value)
                    .Select(x => x.FileName));
            }
            return list;
        }

        async Task ApplyBinlogAsync(string zst, RestoreRequest req, CancellationToken ct)
        {
            // 1) Decompress the .zst binlog to a temp file
            string tmp = await DecompressToTemp(zst, ct);

            // 2) Pipe binlog through mysqlbinlog into mysql
            var psi = new ProcessStartInfo
            {
                FileName = "cmd.exe", // on Linux use "/bin/bash"
                Arguments = $"/C mysqlbinlog \"{tmp}\" | mysql --host={req.Host} --port={req.Port} --user={req.User} --password={req.Password} {req.Database}",
                RedirectStandardError = true
            };

            using var proc = Process.Start(psi)!;
            string err = await proc.StandardError.ReadToEndAsync(ct);
            await proc.WaitForExitAsync(ct);

            File.Delete(tmp);
            if (proc.ExitCode != 0) throw new($"mysqlbinlog|mysql error: {err}");
        }

        async Task<string> DecompressToTemp(string zstPath, CancellationToken ct)
        {
            using var dec = new ZstdNet.Decompressor();
            using var stream = await _store.ReadAsync(zstPath, ct);
            byte[] raw = dec.Unwrap(await ReadAllBytesAsync(stream, ct));
            string tmp = System.IO.Path.GetTempFileName();
            await File.WriteAllBytesAsync(tmp, raw, ct);
            return tmp;
        }

        private static async Task<byte[]> ReadAllBytesAsync(Stream stream, CancellationToken ct)
        {
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms, ct);
            return ms.ToArray();
        }
    } // MySqlAdapter class
} // namespace DbBackup.Adapters
