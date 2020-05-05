using System.Threading.Tasks;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

using BackendFunctions.Models;

namespace BackendFunctions.Services
{
    public class StorageService : IStorageService
    {
        private ProjectSettings _projectSettings;

        public StorageService(ProjectSettings projectSettings)
        {
            _projectSettings = projectSettings;
        }

        public async Task<CloudBlockBlob> SetupStorageBlob_CurrentCloudAnchorId()
        {
            CloudBlobContainer blobContainer = await SetupBlobContainer();
            CloudBlockBlob blob = blobContainer.GetBlockBlobReference(_projectSettings.CurrentAnchorIdBlobName);
            blob.Properties.ContentType = "text/plain";
            return blob;
        }

        public async Task<CloudBlockBlob> SetupStorageBlob_CurrentSceneryDefinition()
        {
            CloudBlobContainer blobContainer = await SetupBlobContainer();
            CloudBlockBlob blob = blobContainer.GetBlockBlobReference(_projectSettings.CurrentSceneryDefinitionBlobName);
            blob.Properties.ContentType = "application/json";
            return blob;
        }

        private async Task<CloudBlobContainer> SetupBlobContainer()
        {
            var containerName = _projectSettings.AzureStorageAccountContainerName;
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_projectSettings.AzureStorageAccountConnectionString);
            CloudBlobClient client = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = client.GetContainerReference(containerName);
            await container.CreateIfNotExistsAsync();
            await container.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
            return container;
        }
    }
}
