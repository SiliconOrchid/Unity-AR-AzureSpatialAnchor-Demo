using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;

namespace BackendFunctions.Services
{
    public interface IStorageService
    {
        Task<CloudBlockBlob> SetupStorageBlob_CurrentCloudAnchorId();
        Task<CloudBlockBlob> SetupStorageBlob_CurrentSceneryDefinition();
    }
}