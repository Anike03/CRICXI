using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CRICXI.Models
{
    public class Player
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string? CricbuzzPlayerId { get; set; }
        public string? CricbuzzMatchId { get; set; }
        public string? Name { get; set; }
        public string? Team { get; set; }
        public string? Role { get; set; }
    }
}
