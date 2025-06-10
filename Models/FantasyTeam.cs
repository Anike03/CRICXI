using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CRICXI.Models
{
    public class FantasyTeam
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string UserId { get; set; }

        public string MatchId { get; set; } // Match from external API

        public string ContestId { get; set; } // Contest the user joined

        public List<string> PlayerIds { get; set; } = new List<string>();

        public string CaptainId { get; set; }

        public string ViceCaptainId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
