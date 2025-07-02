using Microsoft.Extensions.DependencyInjection;
using DbBackup.Core;
using Microsoft.Extensions.Configuration;
using Amazon.S3;

namespace DbBackup.Storage;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddStorage(this IServiceCollection svc, IConfiguration cfg)
    {
        if (cfg.GetValue<bool>("S3:Enabled"))
        {
            svc.AddAWSService<IAmazonS3>();
            svc.AddSingleton<IStorageProvider, S3StorageProvider>();
            return svc;
        }

        svc.AddSingleton<IStorageProvider, LocalStorageProvider>();
        return svc;
    }
}
