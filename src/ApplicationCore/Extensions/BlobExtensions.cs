using Azure.Storage.Blobs;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.eShopWeb.ApplicationCore.Extensions
{
    public static class BlobExtensions
    {
        public static async Task TryUploadBlob(
            string connectionString,
            string container,
            string name,
            string document)
        {
            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(container);
            await containerClient.CreateIfNotExistsAsync();
            var blobClient = containerClient.GetBlobClient(name);

            // Upload to blob storage
            await using var ms = new MemoryStream(Encoding.UTF8.GetBytes(document));
            await blobClient.UploadAsync(ms);
        }
    }
}
