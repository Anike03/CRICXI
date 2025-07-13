using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace CRICXI.Models
{
    public class FantasyTeam
    {
        public string Id { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string MatchId { get; set; } = null!;
        public string TeamName { get; set; } = null!;

        public string CaptainId { get; set; } = null!; 
        public string ViceCaptainId { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<PlayerSelection> Players { get; set; } = new();
    }
 
}
