using Azure.Messaging.ServiceBus;
using System.Threading.Tasks;

namespace Microsoft.eShopWeb.ApplicationCore.Extensions
{
    public static class ServiceBusExtensions
    {
        public static async Task SendMessage(
            string connectionString,
            string queueName,
            string messageBody)
        {
            var client = new ServiceBusClient(connectionString);
            await using var sender = client.CreateSender(queueName);
            var message = new ServiceBusMessage(messageBody);
            await sender.SendMessageAsync(message);
        }
    }
}
