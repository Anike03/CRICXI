using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace CRICXI.Models
{
    public class Match
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string? CricbuzzMatchId { get; set; }
        public string? TeamA { get; set; }
        public string? TeamB { get; set; }
        public string? MatchDesc { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime StartDate { get; set; }
        public string? Venue { get; set; }
        public string? Status { get; set; }
        public int SeriesId { get; set; }

        // Toronto time display properties
        [BsonIgnore]
        public string TorontoTime =>
            TimeZoneInfo.ConvertTimeFromUtc(StartDate,
                TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"))
                .ToString("hh:mm:ss tt");

        [BsonIgnore]
        public string TorontoDate =>
            TimeZoneInfo.ConvertTimeFromUtc(StartDate,
                TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"))
                .ToString("yyyy-MM-dd");
    }
}