using CRICXI.Models;
using MongoDB.Driver;

namespace CRICXI.Services
{
    public class LeaderboardService
    {
        private readonly IMongoCollection<FantasyTeam> _fantasyTeams;
        private readonly IMongoCollection<User> _users;

        public LeaderboardService(IMongoDatabase db)
        {
            _fantasyTeams = db.GetCollection<FantasyTeam>("FantasyTeams");
            _users = db.GetCollection<User>("Users");
        }

        public async Task<List<LeaderboardEntry>> GenerateLeaderboard(string contestId, Dictionary<string, int> playerPoints, Dictionary<string, (bool isCaptain, bool isViceCaptain)> roleMap, int totalPrize)
        {
            var teams = await _fantasyTeams.Find(ft => ft.ContestId == contestId).ToListAsync();

            var leaderboard = new List<LeaderboardEntry>();

            foreach (var team in teams)
            {
                int total = 0;
                foreach (var playerId in team.PlayerIds)
                {
                    var (isCaptain, isVice) = roleMap.ContainsKey(playerId) ? roleMap[playerId] : (false, false);
                    int basePts = playerPoints.ContainsKey(playerId) ? playerPoints[playerId] : 0;

                    if (isCaptain) basePts *= 2;
                    else if (isVice) basePts = (int)(basePts * 1.5);

                    total += basePts;
                }

                var user = await _users.Find(u => u.Id == team.UserId).FirstOrDefaultAsync();
                leaderboard.Add(new LeaderboardEntry
                {
                    FantasyTeamId = team.Id,
                    UserId = team.UserId,
                    Username = user?.Username ?? "Unknown",
                    TotalPoints = total
                });
            }

            // Rank + Prize Distribution
            leaderboard = leaderboard.OrderByDescending(e => e.TotalPoints).ToList();
            for (int i = 0; i < leaderboard.Count; i++)
            {
                leaderboard[i].Rank = i + 1;
                leaderboard[i].PrizeAmount = DistributePrize(i + 1, leaderboard.Count, totalPrize);
            }

            return leaderboard;
        }

        private int DistributePrize(int rank, int totalPlayers, int totalPrize)
        {
            // Example prize logic (top 3)
            if (rank == 1) return (int)(totalPrize * 0.5);
            if (rank == 2) return (int)(totalPrize * 0.3);
            if (rank == 3) return (int)(totalPrize * 0.2);
            return 0;
        }
    }
}
