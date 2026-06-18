using MassTransit;
using ECommerce.Shared;
using MongoDB.Driver;

namespace Notification.API.Consumers
{
    public class PaymentFailedConsumer : BaseNotificationConsumer, IConsumer<PaymentFailedEvent>
    {
        public PaymentFailedConsumer(IMongoDatabase db) : base(db) { }
        public async Task Consume(ConsumeContext<PaymentFailedEvent> context) =>
            await SaveLog(context.Message.OrderId, $"გადახდა ჩავარდა! მიზეზი: {context.Message.Reason}");
    }
}