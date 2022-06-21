using System;
using System.Threading.Tasks;
using DeliveryOrderProcessor31.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos;

namespace DeliveryOrderProcessor31
{
    public static class Function1
    {
        private static readonly string _endpointUri = "https://dmkomov-cosmosdb.documents.azure.com:443/";
        private static readonly string _primaryKey = "SjlbdkmWFtzYv7LLesH90WDpS4203fYWMHZrUb9lrl1jm3zuIeJWaWz6EQgzVrZpT0x2nYmOKS6bgWIbHil58w==";

        [FunctionName("DeliveryOrderProcessor31")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("DeliveryOrderProcessor function received a request.");

            var order = RequestParsingExtensions.GetOrder(req.Body);

            using (CosmosClient client = new CosmosClient(_endpointUri, _primaryKey))
            {
                var databaseResponse = await client.CreateDatabaseIfNotExistsAsync("eshop");
                IndexingPolicy indexingPolicy = new IndexingPolicy
                {
                    IndexingMode = IndexingMode.Consistent,
                    Automatic = true,
                    IncludedPaths =
                    {
                        new IncludedPath
                        {
                            Path = "/*"
                        }
                    }
                };

                var containerProperties = new ContainerProperties("orders", "/id")
                {
                    IndexingPolicy = indexingPolicy
                };
                var containerResponse = await databaseResponse.Database.CreateContainerIfNotExistsAsync(containerProperties);

                try
                {
                    log.LogInformation($"Order: {JsonConvert.SerializeObject(order)}");
                    await containerResponse.Container.CreateItemAsync<Order>(order, new PartitionKey(order.Id));
                }
                catch (Exception ex)
                {
                    log.LogError($"Order wasn't created: {ex.Message}.");
                    return new BadRequestResult();
                }

            }

            return new OkObjectResult($"Order {order.Id} has been uploaded to the CosmosDB.");
        }
    }
}
