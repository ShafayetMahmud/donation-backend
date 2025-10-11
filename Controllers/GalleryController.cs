using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Mvc;
using Azure.Storage.Blobs.Models;

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

    [HttpGet("list-blobs")]
    public async Task<IActionResult> ListAllBlobs()
    {
        try
        {
            var containerClient = new BlobContainerClient(
                new Uri($"https://{accountName}.blob.core.windows.net/{containerName}"),
                new StorageSharedKeyCredential(accountName, accountKey)
            );

            var urls = new List<string>();
            await foreach (var blobItem in containerClient.GetBlobsAsync())
            {
                // Only include files that are not in subdomain folders
                if (!blobItem.Name.Contains("/"))
                {
                    urls.Add($"https://{accountName}.blob.core.windows.net/{containerName}/{blobItem.Name}");
                }
            }

            return Ok(urls);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("{subdomain}/get-sas")]
    public IActionResult GetSas(string subdomain, string blobName)
    {
        var credential = new StorageSharedKeyCredential(accountName, accountKey);

        // Prefix blob name with subdomain
        var fullBlobName = $"{subdomain}/{blobName}";

        // Create a SAS token valid for 1 hour
        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = containerName,
            BlobName = fullBlobName,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)
        };

        // Allow read, write, create
        sasBuilder.SetPermissions(BlobSasPermissions.Read | BlobSasPermissions.Write | BlobSasPermissions.Create);

        var sasToken = sasBuilder.ToSasQueryParameters(credential).ToString();
        var url = $"https://{accountName}.blob.core.windows.net/{containerName}/{fullBlobName}?{sasToken}";

        return Ok(new { url });
    }

    [HttpGet("{subdomain}")]
    public async Task<IActionResult> ListSubdomainBlobs(string subdomain)
    {
        try
        {
            var containerClient = new BlobContainerClient(
                new Uri($"https://{accountName}.blob.core.windows.net/{containerName}"),
                new StorageSharedKeyCredential(accountName, accountKey)
            );

            var urls = new List<string>();
            // For subdomains, list files in their specific folder
            await foreach (var blobItem in containerClient.GetBlobsAsync(prefix: $"{subdomain}/"))
            {
                urls.Add($"https://{accountName}.blob.core.windows.net/{containerName}/{blobItem.Name}");
            }

            return Ok(urls);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = "No file was uploaded." });
        }

        try
        {
            var containerClient = new BlobContainerClient(
                new Uri($"https://{accountName}.blob.core.windows.net/{containerName}"),
                new StorageSharedKeyCredential(accountName, accountKey)
            );

            // Ensure container exists
            await containerClient.CreateIfNotExistsAsync(publicAccessType: PublicAccessType.Blob);

            // Generate unique blob name for main domain
            var blobName = $"{DateTime.UtcNow:yyyyMMddHHmmss}-{Path.GetFileName(file.FileName)}";
            var blobClient = containerClient.GetBlobClient(blobName);

            // Upload the file
            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = file.ContentType
                    }
                });
            }

            return Ok(new { url = blobClient.Uri.ToString() });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("{subdomain}/upload")]
    public async Task<IActionResult> UploadSubdomainImage(string subdomain, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = "No file was uploaded." });
        }

        try
        {
            var containerClient = new BlobContainerClient(
                new Uri($"https://{accountName}.blob.core.windows.net/{containerName}"),
                new StorageSharedKeyCredential(accountName, accountKey)
            );

            // Ensure container exists
            await containerClient.CreateIfNotExistsAsync(publicAccessType: PublicAccessType.Blob);

            // Generate unique blob name with subdomain prefix
            var blobName = $"{subdomain}/{DateTime.UtcNow:yyyyMMddHHmmss}-{Path.GetFileName(file.FileName)}";
            var blobClient = containerClient.GetBlobClient(blobName);

            // Upload the file
            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = file.ContentType
                    }
                });
            }

            return Ok(new { url = blobClient.Uri.ToString() });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpDelete("{subdomain}/{blobName}")]
    public async Task<IActionResult> DeleteImage(string subdomain, string blobName)
    {
        try
        {
            var containerClient = new BlobContainerClient(
                new Uri($"https://{accountName}.blob.core.windows.net/{containerName}"),
                new StorageSharedKeyCredential(accountName, accountKey)
            );

            var fullBlobName = $"{subdomain}/{blobName}";
            var blobClient = containerClient.GetBlobClient(fullBlobName);

            await blobClient.DeleteIfExistsAsync();

            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
