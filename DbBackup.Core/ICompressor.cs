using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DbBackup.Core;

public interface ICompressor
{
    Task<Stream> CompressAsync(Stream input, string algorithm, CancellationToken ct);
}
