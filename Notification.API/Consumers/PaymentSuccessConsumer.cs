using MassTransit;
using ECommerce.Shared;
using MongoDB.Driver;

namespace Notification.API.Consumers
{
    public class PaymentSuccessConsumer : BaseNotificationConsumer, IConsumer<PaymentSuccessEvent>
    {
        public PaymentSuccessConsumer(IMongoDatabase db) : base(db) { }
        public async Task Consume(ConsumeContext<PaymentSuccessEvent> context) =>
            await SaveLog(context.Message.OrderId, "გადახდა წარმატებით შესრულდა. შეკვეთა დასრულებულია!");
    }
}