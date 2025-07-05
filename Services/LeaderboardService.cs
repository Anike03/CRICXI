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
            // Group entries by username and count how many contests they've joined
            var grouped = await _entries.Aggregate()
                .Group(e => e.Username, g => new { Username = g.Key, JoinedCount = g.Count() })
                .ToListAsync();

            var leaderboard = new List<LeaderboardEntry>();

            foreach (var item in grouped)
            {
                var user = await _users.Find(u => u.Username == item.Username).FirstOrDefaultAsync();

                leaderboard.Add(new LeaderboardEntry
                {
                    Username = item.Username,
                    Email = user?.Email ?? "(unknown)",
                    JoinedContests = item.JoinedCount
                });
            }

            // Sort by number of joined contests (descending)
            return leaderboard.OrderByDescending(e => e.JoinedContests).ToList();
        }
    }
}
