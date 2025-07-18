namespace CRICXI.Models
{
    public class JoinContestRequest
    {
        public string ContestId { get; set; }
        public string TeamId { get; set; }
        public string UserId { get; set; }  // Firebase UID
        public string UserEmail { get; set; } // Email for fallback
    }
}
