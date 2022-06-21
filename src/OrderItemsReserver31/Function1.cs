using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OrderItemsReserver;
using OrderItemsReserver.Model;

namespace OrderItemsReserver31
{
    public class Function1
    {
        private const string BlobConnectionString = "DefaultEndpointsProtocol=https;AccountName=dmkomoveshopstorage;AccountKey=MtIkdYpeV5NKRQqzqiJw79Mn4+A48QArr/sbBbMTk9bsnitmsqYWGWMePxtY3fn0ysUPaAEH2QMs+AStHefMjg==;EndpointSuffix=core.windows.net";
        private const string BlobContainerName = "orders";

        [FunctionName("OrderItemsReserver31")]
        [FixedDelayRetry(2, "00:00:05")]
        public async Task Run(
            [ServiceBusTrigger("orders", Connection = "ServiceBusConnectionString")] string myQueueItem,
            ILogger log)
        {
            log.LogInformation($"OrderItemsReserver function received a new message: {myQueueItem}");

            try
            {
                var order = JsonConvert.DeserializeObject<Order>(myQueueItem);

                await BlobExtensions.UploadBlob(BlobConnectionString, BlobContainerName, $"order-{order.Id}",
                    myQueueItem);
            }
            catch (Azure.RequestFailedException ex)
            {
                log.LogError("BlobClient exception occurred: {Message}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                log.LogError("Unexpected exception. Blob document wasn't created.", ex);
                throw;
            }
        }
    }
}
