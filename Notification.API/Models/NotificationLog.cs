using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Notification.API.Models
{
    public class NotificationLog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("orderId")]
        [BsonRepresentation(BsonType.String)]
        public Guid OrderId { get; set; }

        [BsonElement("message")]
        public string Message { get; set; } = string.Empty;

        [BsonElement("timestamp")]
        public DateTime Timestamp { get; set; }
    }
}