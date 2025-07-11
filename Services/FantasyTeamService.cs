using CRICXI.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace CRICXI.Services
{
    public class FantasyTeamService
    {
        private readonly IMongoCollection<FantasyTeam> _teams;

        public FantasyTeamService(IConfiguration config)
        {
            var client = new MongoClient(config.GetConnectionString("MongoDb"));
            var database = client.GetDatabase("CRICXI");
            _teams = database.GetCollection<FantasyTeam>("FantasyTeams");
        }

        public async Task<List<FantasyTeam>> GetByUserAndMatch(string username, string matchId)
        {
            return await _teams.Find(t => t.Username == username && t.MatchId == matchId).ToListAsync();
        }

        public async Task CreateTeamAsync(FantasyTeam team)
        {
            await _teams.InsertOneAsync(team);
        }

        public async Task<List<FantasyTeam>> GetAllTeams()
        {
            return await _teams.Find(_ => true).ToListAsync();
        }
        public async Task<bool> CanCreateTeam(string username, string matchId)
        {
            var count = await _teams.CountDocumentsAsync(t => t.Username == username && t.MatchId == matchId);
            return count < 3;
        }

    }
}
