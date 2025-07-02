using System.IO;
using Xunit;
using DbBackup.Core;

namespace DbBackup.Tests
{
    public class CompressorTests
    {
        [Fact]
        public void ZstdStreamCompressor_CompressAndDecompress_ReturnsOriginalBytes()
        {
            // Arrange
            var compressor = new ZstdStreamCompressor();
            byte[] original = new byte[1024];
            new Random(1234).NextBytes(original);
            using var input = new MemoryStream(original);

            // Act
            using var compressed = compressor.CompressAsync(input, "zstd", CancellationToken.None).Result;
            compressed.Position = 0;

            // Decompress on the fly so we can verify round‐trip
            using var decompressor = new ZstdNet.Decompressor();
            byte[] compressedBytes = new byte[compressed.Length];
            compressed.Read(compressedBytes, 0, compressedBytes.Length);
            byte[] roundtrip = decompressor.Unwrap(compressedBytes);

            // Assert
            Assert.Equal(original, roundtrip);
        }

        [Fact]
        public void ZstdStreamCompressor_PassThroughOnUnsupportedAlgorithm()
        {
            // Arrange
            var compressor = new ZstdStreamCompressor();
            byte[] original = System.Text.Encoding.UTF8.GetBytes("hello world");
            using var input = new MemoryStream(original);

            // Act
            using var output = compressor.CompressAsync(input, "none", CancellationToken.None).Result;
            output.Position = 0;
            byte[] result = new byte[original.Length];
            output.Read(result, 0, result.Length);

            // Assert → since algorithm “none”, the output should exactly match input
            Assert.Equal(original, result);
        }
    }
}
