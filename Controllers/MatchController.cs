using CRICXI.Models;
using CRICXI.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace CRICXI.Controllers
{
    public class MatchController : Controller
    {
        private readonly CricbuzzApiService _api;
        private readonly MatchService _matchService;

        public MatchController(CricbuzzApiService api, MatchService matchService)
        {
            _api = api ?? throw new ArgumentNullException(nameof(api));
            _matchService = matchService ?? throw new ArgumentNullException(nameof(matchService));
        }

        // ✅ Razor View: Upcoming Matches
        public async Task<IActionResult> Upcoming()
        {
            var allMatches = await _matchService.GetAll();

            var upcoming = allMatches
                .Where(m => m.StartDate > DateTime.UtcNow)
                .OrderBy(m => m.StartDate)
                .ToList();

            return View(upcoming);
        }

        // ✅ API: Upcoming Matches
        [HttpGet]
        [Route("api/match/upcoming")]
        public async Task<IActionResult> GetUpcomingMatches()
        {
            var allMatches = await _matchService.GetAll();

            var upcoming = allMatches
                .Where(m => m.StartDate > DateTime.UtcNow)
                .OrderBy(m => m.StartDate)
                .Select(m => new
                {
                    matchId = m.Id,
                    cricbuzzId = m.CricbuzzMatchId,
                    team1 = m.TeamA,
                    team2 = m.TeamB,
                    matchDesc = m.MatchDesc,
                    venue = m.Venue,
                    status = m.Status,
                    startDate = m.StartDate
                })
                .ToList();

            return Json(upcoming);
        }

        // ✅ Razor View: Match Info
        public async Task<IActionResult> Info(string matchId)
        {
            var dbMatch = (await _matchService.GetAll())
                .FirstOrDefault(m => m.CricbuzzMatchId == matchId);

            var json = await _api.GetMatchInfoAsync(matchId);
            var jObj = JObject.Parse(json);

            DateTime startDate = dbMatch?.StartDate ?? DateTime.UtcNow;

            try
            {
                var timestamp = jObj["matchInfo"]?["startDate"]?.Value<long>() ?? 0;
                if (timestamp > 0)
                {
                    startDate = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).UtcDateTime;
                }
            }
            catch { }

            var info = new MatchInfoModel
            {
                MatchDesc = jObj["matchInfo"]?["matchDesc"]?.ToString() ?? dbMatch?.MatchDesc ?? "N/A",
                Venue = jObj["matchInfo"]?["venueInfo"]?["ground"]?.ToString() ?? dbMatch?.Venue ?? "N/A",
                Toss = jObj["matchInfo"]?["tossResults"]?["tossWinnerName"]?.ToString() ?? "TBD",
                Status = jObj["matchInfo"]?["status"]?.ToString() ?? dbMatch?.Status ?? "Pending",
                TeamA = jObj["matchInfo"]?["team1"]?["teamName"]?.ToString() ?? dbMatch?.TeamA ?? "Team A",
                TeamB = jObj["matchInfo"]?["team2"]?["teamName"]?.ToString() ?? dbMatch?.TeamB ?? "Team B",
                StartDate = startDate
            };

            return View(info);
        }

        // ✅ Razor View: Squads
        public async Task<IActionResult> Squads(string matchId)
        {
            try
            {
                var json = await _api.GetMatchSquadsAsync(matchId);
                var jObj = JObject.Parse(json);
                var squads = new List<TeamSquad>();

                if (jObj["team1"] != null)
                    squads.Add(ProcessTeamSquad(jObj["team1"]));
                if (jObj["team2"] != null)
                    squads.Add(ProcessTeamSquad(jObj["team2"]));

                return View(squads);
            }
            catch (Exception ex)
            {
                return View(new List<TeamSquad>
                {
                    new TeamSquad
                    {
                        TeamName = "Error",
                        Players = new List<SquadPlayer>
                        {
                            new SquadPlayer { Name = "Error", Role = ex.Message }
                        }
                    }
                });
            }
        }

        private TeamSquad ProcessTeamSquad(JToken teamToken)
        {
            var teamName = teamToken["team"]?["teamName"]?.ToString() ?? "Unknown Team";
            var players = new List<SquadPlayer>();

            if (teamToken["players"]?["playing XI"] is JArray playingXi)
            {
                foreach (var p in playingXi)
                {
                    players.Add(new SquadPlayer
                    {
                        Name = p["name"]?.ToString() ?? "Unnamed",
                        Role = p["role"]?.ToString() ?? "Unknown"
                    });
                }
            }

            if (teamToken["players"]?["bench"] is JArray benchPlayers)
            {
                foreach (var p in benchPlayers)
                {
                    players.Add(new SquadPlayer
                    {
                        Name = p["name"]?.ToString() ?? "Unnamed",
                        Role = $"{p["role"]?.ToString() ?? "Unknown"} (Bench)"
                    });
                }
            }

            return new TeamSquad
            {
                TeamName = teamName,
                Players = players
            };
        }

        // ✅ API: Match Info (JSON)
        [HttpGet]
        [Route("api/match/{matchId}/info")]
        public async Task<IActionResult> GetMatchInfo(string matchId)
        {
            try
            {
                var json = await _api.GetMatchInfoAsync(matchId);
                var matchInfo = JObject.Parse(json);

                await _matchService.SaveIfNotExists(new Match
                {
                    CricbuzzMatchId = matchId,
                    TeamA = matchInfo["matchInfo"]?["team1"]?["teamName"]?.ToString() ?? "",
                    TeamB = matchInfo["matchInfo"]?["team2"]?["teamName"]?.ToString() ?? "",
                    MatchDesc = matchInfo["matchInfo"]?["matchDesc"]?.ToString() ?? "",
                    StartDate = DateTime.UtcNow,
                    Status = matchInfo["matchInfo"]?["status"]?.ToString() ?? "",
                    Venue = matchInfo["matchInfo"]?["venueInfo"]?["ground"]?.ToString() ?? ""
                });

                return Ok(matchInfo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error fetching match info: {ex.Message}");
            }
        }

        // ✅ API: Squads (JSON)
        [HttpGet]
        [Route("api/match/{matchId}/squads")]
        public async Task<IActionResult> GetMatchSquads(string matchId)
        {
            try
            {
                var json = await _api.GetMatchSquadsAsync(matchId);
                var squads = JObject.Parse(json);
                return Ok(squads);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error fetching squads: {ex.Message}");
            }
        }

        // ✅ API Proxy: Live Score
        public async Task<IActionResult> LiveScore(string id)
        {
            var data = await _api.GetLiveScoreAsync(id);
            ViewBag.Json = data;
            return View("LiveScore");
        }

        // ✅ API Proxy: Recent Matches
        public async Task<IActionResult> Recent()
        {
            var data = await _api.GetRecentMatchesAsync();
            ViewBag.Json = data;
            return View();
        }

        // ✅ Resolve Cricbuzz ID by internal match ID
        [HttpGet]
        [Route("api/match/{id}/cricbuzz-id")]
        public async Task<IActionResult> GetCricbuzzMatchId(string id)
        {
            var match = await _matchService.GetById(id);
            if (match == null)
                return NotFound("Match not found");

            return Ok(new { cricbuzzId = match.CricbuzzMatchId });
        }
    }
}
