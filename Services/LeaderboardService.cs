using CRICXI.Models;
using MongoDB.Driver;

namespace CRICXI.Services
{
    public class LeaderboardService
    {
        private readonly FantasyTeamService _fantasyTeamService;
        private readonly UserService _userService;

        public LeaderboardService(FantasyTeamService fantasyTeamService, UserService userService)
        {
            _fantasyTeamService = fantasyTeamService;
            _userService = userService;
        }

        public async Task<List<LeaderboardEntry>> GenerateLeaderboard(string matchId, Dictionary<string, PlayerPerformance> matchStats, int totalPrize)
        {
            var teams = await _fantasyTeamService.GetAllTeamsByMatch(matchId);
            var scoring = new ScoringService();
            var leaderboard = new List<LeaderboardEntry>();

            foreach (var team in teams)
            {
                int total = 0;

                foreach (var p in team.Players)
                {
                    var playerPerf = matchStats.ContainsKey(p.PlayerId) ? matchStats[p.PlayerId] : new PlayerPerformance();
                    bool isCaptain = p.PlayerId == team.CaptainId;
                    bool isViceCaptain = p.PlayerId == team.ViceCaptainId;

                    int pts = scoring.CalculatePoints(playerPerf, isCaptain, isViceCaptain);
                    total += pts;
                }

                leaderboard.Add(new LeaderboardEntry
                {
                    TeamId = team.Id,
                    Username = team.Username,
                    TotalPoints = total
                });
            }

            // Sort leaderboard by points (highest first)
            leaderboard = leaderboard.OrderByDescending(e => e.TotalPoints).ToList();

            // Assign rank and prize
            for (int i = 0; i < leaderboard.Count; i++)
            {
                leaderboard[i].Rank = i + 1;
                leaderboard[i].PrizeAmount = CalculatePrize(i + 1, totalPrize);
            }

            // ✅ Automatically credit winnings to user wallet
            foreach (var entry in leaderboard)
            {
                if (entry.PrizeAmount > 0)
                {
                    await _userService.UpdateWalletByUsername(entry.Username, entry.PrizeAmount);
                }
            }

            return leaderboard;
        }

        // ✅ Prize calculation based on rank
        private int CalculatePrize(int rank, int totalPrize)
        {
            if (rank == 1) return (int)(totalPrize * 0.5);   // 50% for 1st
            if (rank == 2) return (int)(totalPrize * 0.3);   // 30% for 2nd
            if (rank == 3) return (int)(totalPrize * 0.2);   // 20% for 3rd
            return 0; // No payout for lower ranks
        }
    }
}
