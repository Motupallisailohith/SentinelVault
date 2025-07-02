using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace DbBackup.Adapters
{
    public static class ProcessExtensions
    {
        public static Process WithRedirectedOutput(this Process process, ILogger logger, string prefix)
        {
            process.OutputDataReceived += (s, e) => logger.LogInformation($"{prefix}: {e.Data}");
            process.ErrorDataReceived += (s, e) => logger.LogError($"{prefix}: {e.Data}");
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            return process;
        }
    }
}
