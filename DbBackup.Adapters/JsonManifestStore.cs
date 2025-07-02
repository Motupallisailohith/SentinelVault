using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using DbBackup.Core;

namespace DbBackup.Adapters
{
    public class JsonManifestStore
    {
        private readonly string _manifestPath;
        private readonly JsonSerializerOptions _options;

        public JsonManifestStore(string manifestPath)
        {
            _manifestPath = manifestPath;
            _options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter() }
            };

            Directory.CreateDirectory(Path.GetDirectoryName(_manifestPath)!);
        }

        public async Task SaveAsync(string profileId, BackupManifest manifest)
        {
            var path = Path.Combine(_manifestPath, $"{profileId}.json");
            var manifests = await LoadAsync(profileId);
            manifests.Add(manifest);
            await File.WriteAllTextAsync(path, JsonSerializer.Serialize(manifests, _options));
        }

        public async Task<List<BackupManifest>> LoadAsync(string profileId)
        {
            var path = Path.Combine(_manifestPath, $"{profileId}.json");
            if (!File.Exists(path)) return new List<BackupManifest>();

            var json = await File.ReadAllTextAsync(path);
            return JsonSerializer.Deserialize<List<BackupManifest>>(json) ?? new List<BackupManifest>();
        }

        public async Task DeleteAsync(string profileId)
        {
            var path = Path.Combine(_manifestPath, $"{profileId}.json");
            if (File.Exists(path))
                await Task.Run(() => File.Delete(path));
        }

        public BackupManifest? FromFileName(string fileName)
        {
            var manifests = LoadAsync(Path.GetFileNameWithoutExtension(fileName))
                .GetAwaiter().GetResult();
            return manifests.FirstOrDefault(m => m.FileName == fileName);
        }

        public BackupManifest? GetLatestFull(string database)
        {
            var manifests = LoadAsync(database)
                .GetAwaiter().GetResult();
            return manifests
                .Where(m => m.Type == BackupType.Full)
                .OrderByDescending(m => m.UtcTimestamp)
                .FirstOrDefault();
        }

        public IEnumerable<BackupManifest> GetIncrementals(string database, long start, long end)
        {
            var manifests = LoadAsync(database)
                .GetAwaiter().GetResult();
            return manifests
                .Where(m => m.Type == BackupType.Incremental &&
                           m.BinlogStart >= start &&
                           m.BinlogEnd <= end);
        }
    }
}
