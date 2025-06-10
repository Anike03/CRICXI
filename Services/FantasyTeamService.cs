using CRICXI.Models;
using MongoDB.Driver;

namespace CRICXI.Services
{
    public class FantasyTeamService
    {
        private readonly IMongoCollection<FantasyTeam> _teams;

        public FantasyTeamService(IMongoDatabase db)
        {
            _teams = db.GetCollection<FantasyTeam>("FantasyTeams");
        }

        public async Task<List<FantasyTeam>> GetByUser(string userId) =>
            await _teams.Find(t => t.UserId == userId).ToListAsync();

        public async Task<FantasyTeam> GetById(string id) =>
            await _teams.Find(t => t.Id == id).FirstOrDefaultAsync();

        public async Task Create(FantasyTeam team) =>
            await _teams.InsertOneAsync(team);

        public async Task<List<FantasyTeam>> GetByContest(string contestId) =>
            await _teams.Find(t => t.ContestId == contestId).ToListAsync();
    }
}
