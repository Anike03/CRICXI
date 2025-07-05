using CRICXI.Models;
using MongoDB.Driver;

namespace CRICXI.Services
{
    public class ContestEntryService
    {
        private readonly IMongoCollection<ContestEntry> _entries;

        public ContestEntryService(IMongoDatabase db)
        {
            _entries = db.GetCollection<ContestEntry>("ContestEntries");
        }

        // ✅ Get all entries for a specific contest
        public async Task<List<ContestEntry>> GetByContest(string contestId) =>
            await _entries.Find(e => e.ContestId == contestId).ToListAsync();

        // ✅ Check if a user has already joined a contest
        public async Task<bool> HasUserJoined(string contestId, string username) =>
            await _entries.Find(e => e.ContestId == contestId && e.Username == username).AnyAsync();

        // ✅ Add a new entry
        public async Task Add(ContestEntry entry) =>
            await _entries.InsertOneAsync(entry);

        // ✅ Count how many users have joined
        public async Task<int> GetJoinedCount(string contestId) =>
            (int)await _entries.CountDocumentsAsync(e => e.ContestId == contestId);

        // ✅ Admin: Remove an entry by ID (used in Details.cshtml)
        public async Task RemoveEntry(string entryId) =>
            await _entries.DeleteOneAsync(e => e.Id == entryId);

        public async Task<Dictionary<string, int>> GetContestJoinCountsPerUser()
        {
            var allEntries = await _entries.Find(_ => true).ToListAsync();

            return allEntries
                .GroupBy(e => e.Username)
                .ToDictionary(g => g.Key, g => g.Count());
        }

    }
}
