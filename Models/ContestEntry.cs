using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CRICXI.Models
{
    public class ContestEntry
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;
        public string ContestId { get; set; } = null!;
        public string MatchId { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string TeamId { get; set; } = null!;
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public int Score { get; set; } = 0;

    }
}
