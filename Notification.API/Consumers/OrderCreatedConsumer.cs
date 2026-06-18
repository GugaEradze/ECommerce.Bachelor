using MassTransit;
using ECommerce.Shared;
using MongoDB.Driver;

namespace Notification.API.Consumers
{
    public class OrderCreatedConsumer : BaseNotificationConsumer, IConsumer<OrderCreatedEvent>
    {
        public OrderCreatedConsumer(IMongoDatabase db) : base(db) { }
        public async Task Consume(ConsumeContext<OrderCreatedEvent> context) =>
            await SaveLog(context.Message.OrderId, $"შეკვეთა დარეგისტრირდა სისტემაში. პროდუქტი: {context.Message.ProductId}, რაოდენობა: {context.Message.Quantity}");
    }
}