namespace CRICXI.Models
{
    public class ContestWithEntriesDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string MatchId { get; set; }
        public string TeamA { get; set; }
        public string TeamB { get; set; }
        public DateTime? StartDate { get; set; }
        public decimal EntryFee { get; set; }
        public int MaxParticipants { get; set; }
        public decimal TotalPrize { get; set; }
        public int JoinedCount { get; set; } // ✅ new
    }
}
