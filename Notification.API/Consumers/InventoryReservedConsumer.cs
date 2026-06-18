using MassTransit;
using ECommerce.Shared;
using MongoDB.Driver;

namespace Notification.API.Consumers
{
    public class InventoryReservedConsumer : BaseNotificationConsumer, IConsumer<InventoryReservedEvent>
    {
        public InventoryReservedConsumer(IMongoDatabase db) : base(db) { }
        public async Task Consume(ConsumeContext<InventoryReservedEvent> context) =>
            await SaveLog(context.Message.OrderId, "საწყობში პროდუქტი წარმატებით დარეზერვდა.");
    }
}