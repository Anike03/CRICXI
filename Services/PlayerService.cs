using CRICXI.Models;
using MongoDB.Driver;

namespace CRICXI.Services
{
    public class PlayerService
    {
        private readonly IMongoCollection<Player> _players;

        public PlayerService(IMongoDatabase db)
        {
            _players = db.GetCollection<Player>("Players");
        }

        public async Task SavePlayers(List<Player> players)
        {
            foreach (var player in players)
            {
                var exists = await _players.Find(p =>
                    p.CricbuzzPlayerId == player.CricbuzzPlayerId && p.CricbuzzMatchId == player.CricbuzzMatchId
                ).FirstOrDefaultAsync();

                if (exists == null)
                    await _players.InsertOneAsync(player);
            }
        }

        public async Task<List<Player>> GetPlayersByMatch(string matchId)
        {
            return await _players.Find(p => p.CricbuzzMatchId == matchId).ToListAsync();
        }

        public async Task UpdateRole(string playerId, string role)
        {
            var update = Builders<Player>.Update.Set(p => p.Role, role);
            await _players.UpdateOneAsync(p => p.Id == playerId, update);
        }
    }
}
