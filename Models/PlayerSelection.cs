namespace CRICXI.Models
{

    public class PlayerSelection
    {
        public string PlayerId { get; set; }  // ✅ ADD THIS LINE
        public string Name { get; set; }      // Required for display
        public string Role { get; set; }      // "Batter", "Bowler", etc.
        public string Team { get; set; }      // Team name or short name
        public bool IsCaptain { get; set; }
        public bool IsViceCaptain { get; set; }
    }




}
