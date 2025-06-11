namespace CRICXI.Models
{
    public class PlayerPerformance
    {
        public string PlayerId { get; set; }
        public int Runs { get; set; }
        public int Fours { get; set; }
        public int Sixes { get; set; }
        public int BallsFaced { get; set; }
        public int Wickets { get; set; }
        public int OversBowled { get; set; }
        public int RunsConceded { get; set; }
        public int Catches { get; set; }
        public int Stumpings { get; set; }
        public int RunOutsDirect { get; set; }
        public int RunOutsIndirect { get; set; }
    }

}
