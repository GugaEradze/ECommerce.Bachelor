using MongoDB.Driver;
using Notification.API.Models;

namespace Notification.API.Consumers
{
    public abstract class BaseNotificationConsumer
    {
        protected readonly IMongoCollection<NotificationLog> _collection;

        protected BaseNotificationConsumer(IMongoDatabase database)
        {
            _collection = database.GetCollection<NotificationLog>("Logs");
        }

        protected async Task SaveLog(Guid orderId, string message)
        {
            var log = new NotificationLog
            {
                OrderId = orderId,
                Message = message,
                Timestamp = DateTime.UtcNow
            };
            await _collection.InsertOneAsync(log);
        }
    }
}