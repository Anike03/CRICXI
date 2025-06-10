namespace CRICXI.Models
{
    public class LeaderboardEntry
    {
        public string FantasyTeamId { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; }
        public int TotalPoints { get; set; }
        public int Rank { get; set; }
        public int PrizeAmount { get; set; }
    }
}
