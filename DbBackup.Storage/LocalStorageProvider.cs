using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DbBackup.Core;

namespace DbBackup.Storage
{
    public class LocalStorageProvider : IStorageProvider
    {
        private readonly string _rootPath;

        public LocalStorageProvider(string rootPath)
        {
            _rootPath = rootPath;
            Directory.CreateDirectory(_rootPath);
        }

        public async Task<string> WriteAsync(Stream data, DbBackup.Core.BackupRequest req, CancellationToken ct)
        {
            var path = Path.Combine(
                _rootPath,
                req.Engine,
                req.Database,
                $"{req.Database}_full_{DateTime.UtcNow:yyyyMMdd_HHmmss}.zst"
            );

            Directory.CreateDirectory(Path.GetDirectoryName(path)!);

            await using var file = File.Create(path);
            await data.CopyToAsync(file, ct);

            return path;
        }

        public async Task<Stream> ReadLogAsync(string runId, CancellationToken ct)
        {
            var path = Path.Combine(_rootPath, "logs", $"{runId}.log");
            return await Task.Run(() => File.OpenRead(path), ct);
        }

        public async Task<IEnumerable<BackupManifest>> ListBackupsAsync(string profileId, CancellationToken ct)
        {
            var path = Path.Combine(_rootPath, profileId);
            if (!Directory.Exists(path)) return Enumerable.Empty<BackupManifest>();

            return await Task.Run(() =>
            {
                return Directory.EnumerateFiles(path, "*.zst")
                    .Select(p =>
                    {
                        var filename = Path.GetFileName(p);
                        var parts = filename.Split('_');
                        
                        if (parts.Length >= 3)
                        {
                            var timestamp = parts[1];
                            var typeStr = parts[2].Replace(".zst", "");
                            var utcTimestamp = DateTime.ParseExact(
                                timestamp, 
                                "yyyyMMdd_HHmmss", 
                                null,
                                System.Globalization.DateTimeStyles.None);

                            var type = typeStr switch
                            {
                                "full"        => BackupType.Full,
                                "incremental" => BackupType.Incremental,
                                "differential" => BackupType.Differential,
                                _ => throw new InvalidOperationException($"Unknown backup type: {typeStr}")
                            };

                            return new BackupManifest(
                                Engine: parts[0],
                                Database: parts[0],
                                Type: (DbBackup.Core.BackupType)type,
                                UtcTimestamp: utcTimestamp,
                                BinlogStart: null,
                                BinlogEnd: null,
                                ParentFile: "",
                                FileName: filename,
                                SizeBytes: new FileInfo(p).Length
                            );
                        }
                        return null;
                    })
                    .Where(x => x != null)
                    .Select(x => x!)
                    .ToList();
            });
        }

        public Task<Stream> ReadAsync(string fileName, CancellationToken ct)
        {
            var path = Path.Combine(_rootPath, fileName);
            return Task.FromResult<Stream>(File.OpenRead(path));
        }
    }
}
