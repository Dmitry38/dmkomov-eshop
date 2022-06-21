using System;
using Ardalis.GuardClauses;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Specifications;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.eShopWeb.ApplicationCore.Extensions;
using Newtonsoft.Json;

namespace Microsoft.eShopWeb.ApplicationCore.Services
{
    public class OrderService : IOrderService
    {
        private readonly IRepository<Order> _orderRepository;
        private readonly IUriComposer _uriComposer;
        private readonly IRepository<Basket> _basketRepository;
        private readonly IRepository<CatalogItem> _itemRepository;
        private readonly HttpClient _httpClient;

        public OrderService(
            IRepository<Basket> basketRepository,
            IRepository<CatalogItem> itemRepository,
            IRepository<Order> orderRepository,
            IUriComposer uriComposer,
            HttpClient httpClient)
        {
            _orderRepository = orderRepository;
            _uriComposer = uriComposer;
            _basketRepository = basketRepository;
            _itemRepository = itemRepository;
            _httpClient = httpClient;
        }

        public async Task CreateOrderAsync(int basketId, Address shippingAddress)
        {
            var basketSpec = new BasketWithItemsSpecification(basketId);
            var basket = await _basketRepository.GetBySpecAsync(basketSpec);

            Guard.Against.NullBasket(basketId, basket);
            Guard.Against.EmptyBasketOnCheckout(basket.Items);

            var catalogItemsSpecification = new CatalogItemsSpecification(basket.Items.Select(item => item.CatalogItemId).ToArray());
            var catalogItems = await _itemRepository.ListAsync(catalogItemsSpecification);

            var items = basket.Items.Select(basketItem =>
            {
                var catalogItem = catalogItems.First(c => c.Id == basketItem.CatalogItemId);
                var itemOrdered = new CatalogItemOrdered(catalogItem.Id, catalogItem.Name, _uriComposer.ComposePicUri(catalogItem.PictureUri));
                var orderItem = new OrderItem(itemOrdered, basketItem.UnitPrice, basketItem.Quantity);
                return orderItem;
            }).ToList();

            var order = new Order(basket.BuyerId, shippingAddress, items);

            await _orderRepository.AddAsync(order);

            // Send order details to Azure Function Order Items Reserver that stores a record in blob storage
            var serviceBusConnectionString = Environment.GetEnvironmentVariable("ServiceBusConnectionString");
            var serviceBusQueueName = Environment.GetEnvironmentVariable("ServiceBusQueueName");
            await ServiceBusExtensions.SendMessage(serviceBusConnectionString, serviceBusQueueName, JsonConvert.SerializeObject(order));

            // Http client sends a POST request to Azure Function Delivery Order Processor that creates a record in CosmosDB
            var funcUri = Environment.GetEnvironmentVariable("DeliveryOrderProcessorUri");
            await _httpClient.PostAsync(funcUri, new StringContent(JsonConvert.SerializeObject(order)));
        }
    }
}
