using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CRICXI.Models
{
    public class FantasyTeam
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string? Username { get; set; }
        public string? MatchId { get; set; }
        public string? TeamName { get; set; }

        public List<PlayerSelection> Players { get; set; } = new List<PlayerSelection>();

        public string? CaptainId { get; set; }
        public string? ViceCaptainId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public IEnumerable<string> PlayerIds => Players.Select(p => p.PlayerId ?? string.Empty);
    }

    public class PlayerSelection
    {
        public string? PlayerId { get; set; }
        public string? PlayerName { get; set; }
        public string? Role { get; set; }
        public string? Team { get; set; }
    }
}
