using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace CRICXI.Models
{
    public class FantasyTeam
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string MatchId { get; set; }
        public string TeamName { get; set; }
        public string Username { get; set; }
        public List<PlayerSelection> Players { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class PlayerSelection
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public string TeamId { get; set; }
        public bool IsCaptain { get; set; }
        public bool IsViceCaptain { get; set; }
    }
}
