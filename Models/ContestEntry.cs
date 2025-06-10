using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CRICXI.Models
{
    public class ContestEntry
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string ContestId { get; set; }
        public string MatchId { get; set; }
        public string Username { get; set; } // Who joined
        public string TeamId { get; set; }   // Their selected FantasyTeam Id
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}
