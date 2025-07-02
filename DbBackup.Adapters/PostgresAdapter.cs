using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DbBackup.Core;
using Microsoft.Extensions.Logging;

namespace DbBackup.Adapters
{
    public sealed class PostgresAdapter : DbBackup.Core.IDatabaseAdapter
    {
        public string EngineName => "postgres";
        private readonly ILogger<PostgresAdapter> _log;
        private readonly JsonManifestStore             _manifests;
        /// <summary>
        /// Quick connection test: runs “SELECT 1” via the psql client.
        /// </summary>
        public PostgresAdapter(
            ILogger<PostgresAdapter> log,
            JsonManifestStore        manifests)
        {
            _log       = log;
            _manifests = manifests;
        }
        public async Task RestoreAsync(RestoreRequest req, CancellationToken ct)
        {
            _log.LogWarning("Postgres restore not yet implemented.");
            throw new NotImplementedException("Postgres restore not yet supported");
        }
        public static async Task<bool> CanConnectAsync(
            string host,
            int port,
            string user,
            string pwd,
            string database)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = ResolveTool("psql"),
                    ArgumentList =
                    {
                        "-h", host,
                        "-p", port.ToString(),
                        "-U", user,
                        "-d", database,
                        "-c", "SELECT 1;"
                    },
                    EnvironmentVariables =
                    {
                        ["PGPASSWORD"] = pwd
                    },
                    RedirectStandardOutput = true,
                    RedirectStandardError  = true,
                    UseShellExecute = false
                };

                var proc = Process.Start(psi)
                    ?? throw new InvalidOperationException("psql launch failed");

                string err = await proc.StandardError.ReadToEndAsync();
                await proc.WaitForExitAsync();

                if (proc.ExitCode != 0)
                    throw new Exception($"psql exited {proc.ExitCode}: {err}");

                return true;
            }
            catch
            {
                return false;
            }
        }
        public Task<Stream> CreateBackupAsync(BackupRequest req, CancellationToken ct)
            => DumpFullAsync(req, ct);

        // in DbBackup.Adapters/PostgresAdapter.cs

private async Task<Stream> DumpFullAsync(BackupRequest req, CancellationToken ct)
{
    _log.LogInformation("Running full pg_dump for {Db} inside Docker", req.Database);

    string container  = "pg_demo";    // your container name
    string insidePath = $"/tmp/{req.Database}_backup.dump";

    // 1) Run pg_dump inside the container (customizable compression with -Z 9)
    var execPsi = new ProcessStartInfo
    {
        FileName = "docker",
        ArgumentList =
        {
            "exec", container,
            "pg_dump",
            "-h", req.Host,
            "-p", req.Port.ToString(),
            "-U", req.User,
            "-F", "c",              // custom format (compressed)
            "-Z", "9",              // gzip-level 9
            "-f", insidePath,
            req.Database            // plain database name here
        },
        Environment = 
        {
            ["PGPASSWORD"] = req.Password
        },
        RedirectStandardError = true
    };

    var execProc = Process.Start(execPsi)
                   ?? throw new InvalidOperationException("docker exec pg_dump failed");
    string execErr = await execProc.StandardError.ReadToEndAsync(ct);
    await execProc.WaitForExitAsync(ct);
    if (execProc.ExitCode != 0)
        throw new Exception($"pg_dump in Docker failed ({execProc.ExitCode}): {execErr}");

    // 2) Copy the dump out to a host temp file
    string hostTemp = Path.GetTempFileName() + ".dump";
    var cpPsi = new ProcessStartInfo
    {
        FileName = "docker",
        ArgumentList =
        {
            "cp",
            $"{container}:{insidePath}",
            hostTemp
        },
        RedirectStandardError = true
    };
    var cpProc = Process.Start(cpPsi)
                 ?? throw new InvalidOperationException("docker cp pg_dump failed");
    string cpErr = await cpProc.StandardError.ReadToEndAsync(ct);
    await cpProc.WaitForExitAsync(ct);
    if (cpProc.ExitCode != 0)
        throw new Exception($"docker cp failed ({cpProc.ExitCode}): {cpErr}");

    // 3) Load into a MemoryStream
    var mem = new MemoryStream();
    await using (var fs = File.OpenRead(hostTemp))
        await fs.CopyToAsync(mem, ct);
    File.Delete(hostTemp);

    mem.Position = 0;
    var fileName = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") + ".zst";
    var sizeBytes = mem.Length;
    
    _manifests.SaveAsync(req.ProfileId, new DbBackup.Core.BackupManifest(
        req.Engine,
        req.Database,
        DbBackup.Core.BackupType.Full,
        DateTime.UtcNow,
        null,  // BinlogStart
        null,  // BinlogEnd
        "",    // ParentFile
        fileName,
        sizeBytes
    )).Wait();

    return mem;
}

 

        private Task<Stream> DumpIncrementalAsync(BackupRequest req, CancellationToken ct)
        {
            // Stub: For now, throw until WAL logic is added
            _log.LogWarning("Postgres incremental not yet implemented.");
            throw new NotImplementedException("Postgres incremental (WAL) is not implemented.");
        }

        private static string ResolveTool(string exe)
            => Environment.GetEnvironmentVariable(exe.ToUpperInvariant() + "_PATH") ?? exe;

        // We’ll add a helper here once we know how to read WAL LSN from server
    }
}
