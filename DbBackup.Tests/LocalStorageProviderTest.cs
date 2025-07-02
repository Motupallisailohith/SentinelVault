/*
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Xunit;
using DbBackup.Storage;
using DbBackup.Core;

namespace DbBackup.Tests
{
    
    public class LocalStorageProviderTests : IDisposable
    {
        private readonly string _tempFolder;
        private readonly IConfiguration _config;

        public LocalStorageProviderTests()
        {
            // Set up a temporary directory for testing
            _tempFolder = Path.Combine(Path.GetTempPath(), $"DbBackupTest_{Guid.NewGuid()}");
            Directory.CreateDirectory(_tempFolder);

            // Build a minimal IConfiguration that points Local:BasePath to our temp folder
            var inMemory = new System.Collections.Generic.Dictionary<string, string>
            {
                { "Local:BasePath", _tempFolder }
            };
            _config = new ConfigurationBuilder().AddInMemoryCollection(inMemory).Build();
        }

        public void Dispose()
        {
            // Cleanup
            if (Directory.Exists(_tempFolder))
                Directory.Delete(_tempFolder, recursive: true);
        }

        [Fact]
        public async Task WriteAsync_CreatesFileInCorrectLocationAndReturnsPath()
        {
            // Arrange
            var provider = new LocalStorageProvider(_config);
            byte[] content = System.Text.Encoding.UTF8.GetBytes("test data");
            using var ms = new MemoryStream(content);

            var backupReq = new BackupRequest(
                Engine: "mysql",
                Host: "dummy",
                Port: 3306,
                User: "u",
                Password: "p",
                Database: "testdb",
                Type: BackupType.Full,
                Compression: "none",
                OutPath: _tempFolder)
            {
                UtcTimestamp = DateTime.UtcNow
            };

            // Act
            string resultPath = await provider.WriteAsync(ms, backupReq, CancellationToken.None);

            // Assert
            Assert.True(File.Exists(resultPath), "Backup file should exist on disk");
            string fileName = Path.GetFileName(resultPath);
            Assert.Contains("testdb", fileName);
            Assert.EndsWith(".zst", fileName);

            // Check contents
            byte[] onDisk = await File.ReadAllBytesAsync(resultPath);
            Assert.Equal(content, onDisk);
        }
    }

}
*/
