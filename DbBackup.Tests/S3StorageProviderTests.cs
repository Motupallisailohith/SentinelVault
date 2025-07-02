/*
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using DbBackup.Storage;
using DbBackup.Core;

namespace DbBackup.Tests
{
    public class S3StorageProviderTests
    {
        [Fact]
        public async Task WriteAsync_CallsPutObject_WithCorrectBucketAndKey()
        {
            // Arrange
            var bucketName = "my-test-bucket";
            var inMemory = new System.Collections.Generic.Dictionary<string, string>
            {
                { "S3:Bucket", bucketName }
            };
            IConfiguration config = new ConfigurationBuilder().AddInMemoryCollection(inMemory).Build();

            // Mock the IAmazonS3 client
            var s3Mock = new Mock<IAmazonS3>();
            s3Mock
                .Setup(x => x.PutObjectAsync(
                    It.IsAny<PutObjectRequest>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new PutObjectResponse { HttpStatusCode = System.Net.HttpStatusCode.OK }))
                .Verifiable();

            var provider = new S3StorageProvider(s3Mock.Object, config);

            byte[] content = System.Text.Encoding.UTF8.GetBytes("hello s3");
            using var ms = new MemoryStream(content);

            var backupReq = new BackupRequest(
                Engine      : "mysql",
                Host        : "dummy",
                Port        : 3306,
                User        : "u",
                Password    : "p",
                Database    : "mydb",
                Type        : BackupType.Full,
                Compression : "none",
                OutPath     : "")
            {
                UtcTimestamp = new System.DateTime(2025, 01, 02, 03, 04, 05, DateTimeKind.Utc)
            };

            // Act
            string returnedKey = await provider.WriteAsync(ms, backupReq, CancellationToken.None);

            // Assert
            // 1) Ensure PutObjectAsync was called exactly once
            s3Mock.Verify(
                x => x.PutObjectAsync(
                    It.IsAny<PutObjectRequest>(), 
                    It.IsAny<CancellationToken>()),
                Times.Once);

            // 2) Check that the returned key matches expected pattern
            Assert.Equal("mydb/20250102_030405_full.zst", returnedKey);

            // 3) Inspect the actual PutObjectRequest that was passed into PutObjectAsync
            s3Mock.Verify(x => x.PutObjectAsync(
                It.Is<PutObjectRequest>(req =>
                    req.BucketName == bucketName &&
                    req.Key == "mydb/20250102_030405_full.zst" &&
                    req.InputStream.Length == content.Length
                ), CancellationToken.None),
                Times.Once);
        }
    }
}
*/
