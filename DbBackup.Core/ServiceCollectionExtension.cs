using Microsoft.Extensions.DependencyInjection;

namespace DbBackup.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCore(this IServiceCollection services)
        => services
            .AddSingleton<ICompressor, ZstdStreamCompressor>()
            .AddSingleton<BackupOrchestrator>();
}
