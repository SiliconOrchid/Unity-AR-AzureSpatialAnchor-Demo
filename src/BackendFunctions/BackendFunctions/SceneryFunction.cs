using System;
using System.Threading.Tasks;
using System.IO;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage.Blob;

using BackendFunctions.Services;

namespace BackendFunctions
{
    public class SceneryFunction
    {
        private readonly IStorageService _storageHandler;

        public SceneryFunction(IStorageService storageHandler)
        {
            _storageHandler = storageHandler ?? throw new ArgumentException();
        }


        [FunctionName("Scenery")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req)
        {
            CloudBlockBlob cloudBlockBlob = await _storageHandler.SetupStorageBlob_CurrentSceneryDefinition();




            if (req.Method == "POST")
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                await cloudBlockBlob.UploadTextAsync(requestBody);
                return (ActionResult)new OkObjectResult("");
            }

            // otherwise, drop through to a default "GET" behaviour
            string currentSceneryJson = await cloudBlockBlob.DownloadTextAsync();
            return (ActionResult)new OkObjectResult(currentSceneryJson);
        }
    }
}

