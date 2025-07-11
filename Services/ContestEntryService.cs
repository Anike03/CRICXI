﻿using CRICXI.Models;
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

        public async Task<List<ContestEntry>> GetByContest(string contestId) =>
            await _entries.Find(e => e.ContestId == contestId).ToListAsync();

        public async Task<bool> HasUserJoined(string contestId, string username) =>
            await _entries.Find(e => e.ContestId == contestId && e.Username == username).AnyAsync();

        public async Task Add(ContestEntry entry) =>
            await _entries.InsertOneAsync(entry);

        public async Task<int> GetJoinedCount(string contestId) =>
            (int)await _entries.CountDocumentsAsync(e => e.ContestId == contestId);

        public async Task RemoveEntry(string entryId) =>
            await _entries.DeleteOneAsync(e => e.Id == entryId);

        public async Task<Dictionary<string, int>> GetContestJoinCountsPerUser()
        {
            var allEntries = await _entries.Find(_ => true).ToListAsync();

            return allEntries
                .GroupBy(e => e.Username)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        public async Task<List<ContestEntry>> GetAll() =>
            await _entries.Find(_ => true).ToListAsync();
    }
}
