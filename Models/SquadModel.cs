namespace CRICXI.Models
{
    // Represents a single player in the squad with basic info
    public class SquadPlayer
    {
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    // Represents a team's squad (Playing XI or Full Squad)
    public class TeamSquad
    {
        public string TeamName { get; set; } = string.Empty;
        public List<SquadPlayer> Players { get; set; } = new();
    }
}
