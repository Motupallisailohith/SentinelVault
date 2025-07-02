using ZstdNet;
using System.Threading;
using System.Threading.Tasks;

namespace DbBackup.Core;

public sealed class ZstdStreamCompressor : ICompressor
{
    public async Task<Stream> CompressAsync(Stream input, string algorithm, CancellationToken ct)
    {
        if (algorithm is not ("zstd" or "ZSTD"))
            return input; // pass-through for "none"

        var compressor = new Compressor();                   // owns native context
        var output = new MemoryStream();

        await input.CopyToAsync(output, ct);                 // load into memory (simple for Sprint 1)
        output.Position = 0;
        byte[] compressed = compressor.Wrap(output.ToArray());

        return new MemoryStream(compressed);                 // caller disposes
    }
}
