using CRICXI.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;

public class ScoreCalculatorService
{
    private readonly HttpClient _httpClient;
    private readonly IMongoCollection<FantasyTeam> _fantasyTeams;
    private readonly IMongoCollection<ContestEntry> _contestEntries;

    public ScoreCalculatorService(IConfiguration config, IOptions<MongoDBSettings> settings)
    {
        _httpClient = new HttpClient();
        var client = new MongoClient(settings.Value.ConnectionString);
        var db = client.GetDatabase(settings.Value.DatabaseName);
        _fantasyTeams = db.GetCollection<FantasyTeam>("FantasyTeams");
        _contestEntries = db.GetCollection<ContestEntry>("ContestEntries");
    }

    // ✅ View-only mode (does not save to DB)
    public async Task<Dictionary<string, int>> CalculateMatchScores(string matchId)
    {
        var json = await FetchScorecardJson(matchId);
        var playerScores = ExtractPlayerScores(json);

        var fantasyTeams = await _fantasyTeams.Find(t => t.MatchId == matchId).ToListAsync();
        var result = new Dictionary<string, int>();

        foreach (var team in fantasyTeams)
        {
            int total = 0;

            foreach (var player in team.Players)
            {
                var playerId = player.PlayerId?.ToString();
                if (string.IsNullOrEmpty(playerId)) continue;

                int score = playerScores.GetValueOrDefault(playerId, 0);

                if (playerId == team.CaptainId) score *= 2;
                else if (playerId == team.ViceCaptainId) score = (int)(score * 1.5);

                total += score;
            }

            result[team.Id] = total;
        }

        return result; // teamId => total score
    }

    // ✅ Calculate and store in ContestEntry.Score
    public async Task<Dictionary<string, int>> CalculateAndSaveScores(string matchId)
    {
        var scores = await CalculateMatchScores(matchId);

        foreach (var kvp in scores)
        {
            var filter = Builders<ContestEntry>.Filter.And(
                Builders<ContestEntry>.Filter.Eq(e => e.TeamId, kvp.Key),
                Builders<ContestEntry>.Filter.Eq(e => e.MatchId, matchId)
            );

            var update = Builders<ContestEntry>.Update.Set(e => e.Score, kvp.Value);
            await _contestEntries.UpdateManyAsync(filter, update);
        }

        return scores;
    }

    // ✅ Fetch and parse Cricbuzz scorecard
    private async Task<JObject> FetchScorecardJson(string matchId)
    {
        var url = $"https://Cricbuzz-Official-Cricket-API.proxy-production.allthingsdev.co/match/{matchId}/scorecard";
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("x-apihub-key", "T7-xiJYNyjX581-Zl-84gr4Z8hXo6H8Z7Ci9LeYd6E0fYJKqar");
        request.Headers.Add("x-apihub-host", "Cricbuzz-Official-Cricket-API.allthingsdev.co");
        request.Headers.Add("x-apihub-endpoint", "5f260335-c228-4005-9eec-318200ca48d6");

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            throw new Exception("Failed to fetch scorecard from Cricbuzz");

        var content = await response.Content.ReadAsStringAsync();
        return JObject.Parse(content);
    }

    // ✅ Apply Dream11-style scoring logic
    private Dictionary<string, int> ExtractPlayerScores(JObject json)
    {
        var scores = new Dictionary<string, int>();

        foreach (var inning in json["scoreCard"] ?? new JArray())
        {
            foreach (var batter in inning["batTeamDetails"]?["batsmenData"] ?? new JArray())
            {
                var id = batter["batId"]?.ToString();
                if (id == null) continue;

                int runs = batter["runs"]?.Value<int>() ?? 0;
                int fours = batter["fours"]?.Value<int>() ?? 0;
                int sixes = batter["sixes"]?.Value<int>() ?? 0;
                bool notOut = batter["outDesc"]?.ToString().ToLower() == "not out";

                int score = runs + (fours * 1) + (sixes * 2);
                if (runs == 0 && !notOut) score -= 4; // duck penalty

                scores[id] = scores.GetValueOrDefault(id, 0) + score;
            }

            foreach (var bowler in inning["bowlTeamDetails"]?["bowlersData"] ?? new JArray())
            {
                var id = bowler["bowlerId"]?.ToString();
                if (id == null) continue;

                int wickets = bowler["wickets"]?.Value<int>() ?? 0;
                int maidens = bowler["maidens"]?.Value<int>() ?? 0;

                int score = (wickets * 25) + (maidens * 12);
                scores[id] = scores.GetValueOrDefault(id, 0) + score;
            }
        }

        return scores;
    }
}
