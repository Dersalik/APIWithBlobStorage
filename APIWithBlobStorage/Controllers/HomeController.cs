using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using APIWithBlobStorage.DTO;
using Azure.Storage.Blobs.Models;
using Azure;

namespace APIWithBlobStorage.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly BlobServiceClient _blobServiceClient;

        public HomeController(BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;
        }

        [HttpGet]
        public async Task<IActionResult> Get(string containerName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient("mycontainer");
            if (! await containerClient.ExistsAsync())
            {
                return NotFound("container doesnt exist");
            }
            var blobs = containerClient.GetBlobs().ToList();
            return Ok(blobs.Select(b => b.Name));

        }

        [HttpGet]
        [Route("{containerName}/{blobName}")]
        public async Task<IActionResult> GetBlobMetadata(string blobName, string containerName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            if (!await containerClient.ExistsAsync())
            {
                return NotFound("container doesnt exist");
            }
            var blobClient = containerClient.GetBlobClient(blobName);
            if (!await blobClient.ExistsAsync())
            {
                return NotFound("blob doesnt exist");
            }
            var blobProperties = await blobClient.GetPropertiesAsync();
            return Ok(blobProperties.Value.Metadata);
        }

        [HttpPost]
        [Route("{containerName}/upload/{blobName}")]
        public async Task<IActionResult> UploadBlobWithMetadata(string blobName, string containerName, [FromForm] IFormFile formFile)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync();

            var blobClient = containerClient.GetBlobClient(blobName);

            var blobUploadOptions = new BlobUploadOptions
            {
                Metadata = new Dictionary<string, string>
        {
            { "uploadedBy", "user" },
            { "uploadedOn", DateTime.UtcNow.ToString("o") }
        }
            };

            await blobClient.UploadAsync(formFile.OpenReadStream(), blobUploadOptions);

            return Ok();
        }
        [HttpGet]
        [Route("{containerName}/download/{blobName}")]
        public async Task<IActionResult> DownloadBlob(string blobName, string containerName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient("mycontainer");
            if (!await containerClient.ExistsAsync())
            {
                return NotFound("container doesnt exist");
            }
            var blobClient = containerClient.GetBlobClient(blobName);

            if (! await blobClient.ExistsAsync())
            {
                return NotFound("blob doesnt exist");
            }
            var data = await blobClient.OpenReadAsync();
            Stream blobContent = data;

            var content = await blobClient.DownloadContentAsync();

            string name = blobName;
            string contentType = content.Value.Details.ContentType;
            var file = new BlobDto { Content = blobContent, Name = name, ContentType = contentType };
            return File(file.Content,file.ContentType,file.Name);
        }

        [HttpDelete]
        [Route("{containerName}/delete/{blobName}")]
        public async Task<IActionResult> DeleteBlob(string blobName, string containerName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient("mycontainer");
            if (!await containerClient.ExistsAsync())
            {
                return NotFound("container doesnt exist");
            }
            var blobClient = containerClient.GetBlobClient(blobName);

            if (!await blobClient.ExistsAsync())
            {
                return NotFound("blob doesnt exist");
            }
            var response = await blobClient.DeleteAsync();

            return Ok();
        }
        

    }
}
