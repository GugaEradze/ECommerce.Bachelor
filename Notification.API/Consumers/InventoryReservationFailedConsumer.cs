using MassTransit;
using ECommerce.Shared;
using MongoDB.Driver;

namespace Notification.API.Consumers
{
    public class InventoryReservationFailedConsumer : BaseNotificationConsumer, IConsumer<InventoryReservationFailedEvent>
    {
        public InventoryReservationFailedConsumer(IMongoDatabase db) : base(db) { }

        public async Task Consume(ConsumeContext<InventoryReservationFailedEvent> context) =>
            await SaveLog(context.Message.OrderId, $"საწყობის შეცდომა! მიზეზი: {context.Message.Reason}");
    }
}