using System.IO;
using DeliveryOrderProcessor31.Model;
using Newtonsoft.Json;

namespace DeliveryOrderProcessor31
{
    internal static class RequestParsingExtensions
    {
        public static string GetNewOrderName(Stream requestBody)
        {
            var request = new StreamReader(requestBody).ReadToEnd();
            var order = JsonConvert.DeserializeObject<Order>(request);

            return $"order-{order.Id}.json";
        }

        public static Order GetOrder(Stream requestBody)
        {
            var request = new StreamReader(requestBody).ReadToEnd();
            return JsonConvert.DeserializeObject<Order>(request);
        }
    }
}
