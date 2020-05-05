using System;
using System.Linq;

using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using BackendFunctions;
using BackendFunctions.Models;
using BackendFunctions.Services;

[assembly: FunctionsStartup(typeof(Startup))]
namespace BackendFunctions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            ProjectSettings projectSettings = new ProjectSettings
            {
                AzureStorageAccountConnectionString = Environment.GetEnvironmentVariable("AzureStorageAccountConnectionString"),
                AzureStorageAccountContainerName = Environment.GetEnvironmentVariable("AzureStorageAccountContainerName"),
                CurrentAnchorIdBlobName = Environment.GetEnvironmentVariable("CurrentAnchorIdBlobName"),
                CurrentSceneryDefinitionBlobName = Environment.GetEnvironmentVariable("CurrentSceneryDefinitionBlobName"),
            };

            builder.Services.AddSingleton(projectSettings);
            builder.Services.AddSingleton<IStorageService, StorageService>();
        }
    }
}