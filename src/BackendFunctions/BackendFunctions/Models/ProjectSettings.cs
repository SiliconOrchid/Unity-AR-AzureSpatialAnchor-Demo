namespace BackendFunctions.Models
{
    public class ProjectSettings
    {
        public string AzureStorageAccountConnectionString { get; set; }
        public string AzureStorageAccountContainerName { get; set; }
        public string CurrentAnchorIdBlobName { get; set; }
        public string CurrentSceneryDefinitionBlobName { get; set; }
    }
}
