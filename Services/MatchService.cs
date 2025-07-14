using CRICXI.Models;
using MongoDB.Driver;

namespace CRICXI.Services
{
    public class MatchService
    {
        private readonly IMongoCollection<Match> _matches;

        public MatchService(IMongoDatabase db)
        {
            _matches = db.GetCollection<Match>("Matches");
        }
        public async Task<List<Match>> GetBySeries(int seriesId)
        {
            // Implement this based on your data storage
            // Example for MongoDB:
            return await _matches.Find(m => m.SeriesId == seriesId).ToListAsync();
        }
        public async Task<List<Match>> GetAll() =>
            await _matches.Find(_ => true).ToListAsync();

        public async Task<Match> GetById(string id) =>
            await _matches.Find(m => m.Id == id).FirstOrDefaultAsync();

        public async Task<Match> GetByCricbuzzMatchId(string cricbuzzId) =>
            await _matches.Find(m => m.CricbuzzMatchId == cricbuzzId).FirstOrDefaultAsync();

        public async Task SaveIfNotExists(Match match)
        {
            var exists = await GetByCricbuzzMatchId(match.CricbuzzMatchId);
            if (exists == null)
            {
                await _matches.InsertOneAsync(match);
            }
        }
    }
}
