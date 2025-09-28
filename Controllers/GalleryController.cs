using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class GalleryController : ControllerBase
{
    private readonly string accountName = "mudhammataanstorage";
    private readonly string accountKey;
    private readonly string containerName = "gallery";

    public GalleryController(IConfiguration config)
    {
        accountKey = config["AZURE_STORAGE_KEY"] 
                     ?? throw new InvalidOperationException("AZURE_STORAGE_KEY is not set in environment variables.");
    }


    [HttpGet("get-sas")]
    public IActionResult GetSas(string blobName)
    {
        var credential = new StorageSharedKeyCredential(accountName, accountKey);

        // Create a SAS token valid for 1 hour
        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = containerName,
            BlobName = blobName,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)
        };

        // Allow read, write, create
        sasBuilder.SetPermissions(BlobSasPermissions.Read | BlobSasPermissions.Write | BlobSasPermissions.Create);

        var sasToken = sasBuilder.ToSasQueryParameters(credential).ToString();
        var url = $"https://{accountName}.blob.core.windows.net/{containerName}/{blobName}?{sasToken}";

        return Ok(new { url });
    }

    [HttpGet("list-blobs")]
    public async Task<IActionResult> ListBlobs()
    {
        var containerClient = new BlobContainerClient(
            new Uri($"https://{accountName}.blob.core.windows.net/{containerName}"),
            new StorageSharedKeyCredential(accountName, accountKey)
        );

        var urls = new List<string>();
        await foreach (var blobItem in containerClient.GetBlobsAsync())
        {
            // var url = $"https://{accountName}.blob.core.windows.net/{containerName}/{blobItem.Name}";
            var url = $"https://{accountName}.blob.core.windows.net/{containerName}/{Uri.EscapeDataString(blobItem.Name)}";

            urls.Add(url);
        }

        return Ok(urls);
    }
}
