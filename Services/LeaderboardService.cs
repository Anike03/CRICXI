// Services/LeaderboardService.cs
using CRICXI.Models;
using MongoDB.Driver;

namespace CRICXI.Services
{
    public class LeaderboardService
    {
        private readonly IMongoCollection<ContestEntry> _entries;
        private readonly IMongoCollection<User> _users;

        public LeaderboardService(IMongoDatabase db)
        {
            _entries = db.GetCollection<ContestEntry>("ContestEntries");
            _users = db.GetCollection<User>("Users");
        }

        public async Task<List<LeaderboardEntry>> GetLeaderboard()
        {
            var entries = await _entries.Find(_ => true).ToListAsync();

            var grouped = entries
                .GroupBy(e => e.Username)
                .Select(async g =>
                {
                    var user = await _users.Find(u => u.Username == g.Key).FirstOrDefaultAsync();
                    return new LeaderboardEntry
                    {
                        Username = g.Key,
                        Email = user?.Email ?? "(unknown)",
                        JoinedContests = g.Count()
                    };
                });

            var leaderboardTasks = await Task.WhenAll(grouped);
            return leaderboardTasks.OrderByDescending(e => e.JoinedContests).ToList();
        }
    }
}
