using CRICXI.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Microsoft.Extensions.Options;

public class FantasyTeamService
{
    private readonly IMongoCollection<FantasyTeam> _teams;

    public FantasyTeamService(IOptions<MongoDBSettings> mongoSettings)
    {
        var client = new MongoClient(mongoSettings.Value.ConnectionString);
        var database = client.GetDatabase(mongoSettings.Value.DatabaseName);
        _teams = database.GetCollection<FantasyTeam>("FantasyTeams");
    }

    public async Task<FantasyTeam> GetByIdAsync(string id)
    {
        return await _teams.Find(t => t.Id == id).FirstOrDefaultAsync();
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
    public async Task<List<TeamSquad>> GetSquadsByMatch(string matchId)
    {
        var teams = await _teams.Find(t => t.MatchId == matchId).ToListAsync();

        var teamMap = new Dictionary<string, List<SquadPlayer>>();

        foreach (var team in teams)
        {
            foreach (var player in team.Players)
            {
                var teamName = player.Team ?? "Unknown";

                if (!teamMap.ContainsKey(teamName))
                    teamMap[teamName] = new List<SquadPlayer>();

                if (!teamMap[teamName].Any(p => p.Name == player.Name))
                {
                    teamMap[teamName].Add(new SquadPlayer
                    {
                        Name = player.Name,
                        Role = player.Role
                    });
                }
            }
        }

        var result = new List<TeamSquad>();
        foreach (var (teamName, players) in teamMap)
        {
            result.Add(new TeamSquad
            {
                TeamName = teamName,
                Players = players
            });
        }

        return result;
    }


}
