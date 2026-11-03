using Karpaty.Api.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Karpaty.Api.DBCommunication.Documents
{
    public class PlaceDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string id { get; set; }
        [BsonElement("name")]
        public string name { get; set; }
        [BsonElement("category")]
        public PlaceCategory category { get; set; }
        [BsonElement("region")]
        public PlaceRegion region { get; set; }
        [BsonElement("description")]
        public string description { get; set; }
        [BsonElement("difficulty")]
        public PlaceDifficulty difficulty { get; set; }
        [BsonElement("latitude")]
        public float latitude { get; set; }
        [BsonElement("longitude")]
        public float longitude { get; set; }
        [BsonElement("imageulr")]
        public string imageUrl { get; set; }
    }
}
