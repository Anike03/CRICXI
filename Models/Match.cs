using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CRICXI.Models
{
    public class Match
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string CricbuzzMatchId { get; set; } // Unique ID from API
        public string TeamA { get; set; }
        public string TeamB { get; set; }
        public string MatchDesc { get; set; }
        public string StartDate { get; set; }
        public string Status { get; set; } // Upcoming, Live, Completed
    }
}
