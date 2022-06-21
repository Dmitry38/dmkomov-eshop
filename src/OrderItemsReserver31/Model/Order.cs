using System;
using System.Linq;
using Newtonsoft.Json;

namespace OrderItemsReserver.Model
{
    public class Order
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public string BuyerId { get; set; }
        public DateTime OrderDate { get; set; }
        public Address ShipToAddress { get; set; }
        public Item[] OrderItems { get; set; }

        public double TotalPrice => OrderItems.Select(x => x.UnitPrice * x.Units).Sum();
    }
}
