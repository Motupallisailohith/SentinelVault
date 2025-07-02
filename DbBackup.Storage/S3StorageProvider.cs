using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using DbBackup.Core;
using Microsoft.Extensions.Configuration;
using DbBackup.Storage;

namespace DbBackup.Storage
{
    public class S3StorageProvider : IStorageProvider
    {
        private readonly AmazonS3Client _s3;
        private readonly string _bucket;
        private readonly string _prefix;
        private readonly string _region;

        public S3StorageProvider(AmazonS3Client s3, string bucket, string prefix, string region)
        {
            _s3 = s3;
            _bucket = bucket;
            _prefix = prefix;
            _region = region;
        }

        public async Task<string> WriteAsync(Stream data, DbBackup.Core.BackupRequest req, CancellationToken ct)
        {
            var key = $"{_prefix}/{req.Engine}/{req.Database}/{req.Database}_{req.Type}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.zst";
            var putRequest = new PutObjectRequest
            {
                BucketName = _bucket,
                Key = key,
                InputStream = data,
                ContentType = "application/octet-stream",
                ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
            };

            await _s3.PutObjectAsync(putRequest, ct);
            return key;
        }

        public async Task<Stream> ReadLogAsync(string runId, CancellationToken ct)
        {
            var key = $"{_prefix}/logs/{runId}.log";
            var getRequest = new GetObjectRequest
            {
                BucketName = _bucket,
                Key = key
            };

            var response = await _s3.GetObjectAsync(getRequest, ct);
            return response.ResponseStream;
        }

        public async Task<Stream> ReadAsync(string fileName, CancellationToken ct)
        {
            var key = $"{_prefix}/{fileName}";
            var req = new GetObjectRequest
            {
                BucketName = _bucket,
                Key = key
            };

            var resp = await _s3.GetObjectAsync(req, ct);
            return resp.ResponseStream;
        }

        public async Task<IEnumerable<DbBackup.Core.BackupManifest>> ListBackupsAsync(string profileId, CancellationToken ct)
        {
            var req = new ListObjectsV2Request
            {
                BucketName = _bucket,
                Prefix = $"{_prefix}/{profileId}/"
            };

            var manifests = new List<BackupManifest>();
            do
            {
                var resp = await _s3.ListObjectsV2Async(req, ct);
                foreach (var item in resp.S3Objects)
                {
                    var filename = Path.GetFileName(item.Key);
                    if (filename?.EndsWith(".zst") != true)
                        continue;

                    var parts = filename.Split('_');
                    if (parts.Length < 3)
                        continue;

                    var type = parts[2].Replace(".zst", "") switch
                    {
                        "full" => DbBackup.Core.BackupType.Full,
                        "incremental" => DbBackup.Core.BackupType.Incremental,
                        "differential" => DbBackup.Core.BackupType.Differential,
                        _ => throw new InvalidOperationException($"Unknown backup type: {parts[2]}")
                    };

                    var utcTimestamp = DateTime.ParseExact(
                        parts[1],
                        "yyyyMMdd_HHmmss",
                        null,
                        System.Globalization.DateTimeStyles.None);

                    manifests.Add(new DbBackup.Core.BackupManifest(
                        Engine: parts[0],
                        Database: parts[0],
                        Type: type,
                        UtcTimestamp: utcTimestamp,
                        BinlogStart: null,
                        BinlogEnd: null,
                        ParentFile: "",
                        FileName: filename,
                        SizeBytes: item.Size));
                }

                req.ContinuationToken = resp.NextContinuationToken;
            } while (req.ContinuationToken != null);

            return manifests;
        }
    }
}
