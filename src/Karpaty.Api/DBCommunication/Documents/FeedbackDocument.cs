using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Karpaty.Api.DBCommunication.Documents
{
    public class FeedbackDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;

        [BsonElement("rating")]
        public int Rating { get; set; }

        [BsonElement("message")]
        public string Message { get; set; } = string.Empty;

        [BsonElement("createdat")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
