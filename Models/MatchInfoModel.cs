namespace CRICXI.Models
{
    public class MatchInfoModel
    {
        public string MatchDesc { get; set; } = string.Empty;
        public string Venue { get; set; } = string.Empty;
        public string Toss { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string TeamA { get; set; } = string.Empty;
        public string TeamB { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }

        // Updated computed properties to match the Match class
        public string TorontoTime =>
     TimeZoneInfo.ConvertTimeFromUtc(StartDate,
         TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"))
         .ToString("hh:mm:ss tt");

        public string TorontoDate =>
            TimeZoneInfo.ConvertTimeFromUtc(StartDate,
                TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"))
                .ToString("yyyy-MM-dd");
    }
}