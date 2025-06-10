namespace CRICXI.Models
{
    public class Player
    {
        public string Id { get; set; }           // Unique ID (could match API ID)
        public string Name { get; set; }         // Player name (e.g., Virat Kohli)
        public string Role { get; set; }         // Role: Batsman, Bowler, All-Rounder, Wicket-Keeper
    }
}
