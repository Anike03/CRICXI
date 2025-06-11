using CRICXI.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CRICXI.Services
{
    public class FantasyTeamService
    {
        private readonly IMongoCollection<FantasyTeam> _teams;

        public FantasyTeamService(IMongoDatabase db)
        {
            _teams = db.GetCollection<FantasyTeam>("FantasyTeams");
        }

        // ✅ Get teams by user (all matches)
        public async Task<List<FantasyTeam>> GetByUser(string username)
        {
            return await _teams.Find(t => t.Username == username).ToListAsync();
        }

        // ✅ Get teams by user for specific match
        public async Task<List<FantasyTeam>> GetByUserAndMatch(string username, string matchId)
        {
            return await _teams.Find(t => t.Username == username && t.MatchId == matchId).ToListAsync();
        }

        // ✅ Create fantasy team
        public async Task Create(FantasyTeam team)
        {
            await _teams.InsertOneAsync(team);
        }

        // ✅ Get all fantasy teams for specific match (used by leaderboard)
        public async Task<List<FantasyTeam>> GetAllTeamsByMatch(string matchId)
        {
            return await _teams.Find(t => t.MatchId == matchId).ToListAsync();
        }

        // ✅ FULL ADMIN: Get all fantasy teams (for Admin Panel)
        public async Task<List<FantasyTeam>> GetAllTeams()
        {
            return await _teams.Find(_ => true).ToListAsync();
        }
    }
}
