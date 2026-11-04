using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Karpaty.Api.DBCommunication.Documents
{
    [BsonIgnoreExtraElements]
    public class TipDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string id { get; set; } = string.Empty;

        [BsonElement("title")]
        public string title { get; set; } = string.Empty;

        [BsonElement("category")]
        public string category { get; set; } = string.Empty;

        [BsonElement("text")]
        public string text { get; set; } = string.Empty;
    }
}