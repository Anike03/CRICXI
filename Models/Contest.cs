using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CRICXI.Models
{
    public class Contest
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string MatchId { get; set; } // This comes from the external API match

        public string Name { get; set; }
        public string TeamA { get; set; }
        public string TeamB { get; set; }
        public DateTime? StartDate { get; set; }

        public int EntryFee { get; set; } // Amount user pays to enter

        public int MaxParticipants { get; set; }

        public int TotalPrize { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
