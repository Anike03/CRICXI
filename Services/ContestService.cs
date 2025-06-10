using CRICXI.Models;
using MongoDB.Driver;

namespace CRICXI.Services
{
    public class ContestService
    {
        private readonly IMongoCollection<Contest> _contests;

        public ContestService(IMongoDatabase db)
        {
            _contests = db.GetCollection<Contest>("Contests");
        }

        public async Task<List<Contest>> GetAll() =>
            await _contests.Find(_ => true).ToListAsync();

        public async Task<Contest> GetById(string id) =>
            await _contests.Find(c => c.Id == id).FirstOrDefaultAsync();

        public async Task Create(Contest contest) =>
            await _contests.InsertOneAsync(contest);

        public async Task Update(string id, Contest updated) =>
            await _contests.ReplaceOneAsync(c => c.Id == id, updated);

        public async Task Delete(string id) =>
            await _contests.DeleteOneAsync(c => c.Id == id);
    }
}
