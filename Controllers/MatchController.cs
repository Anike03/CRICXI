using CRICXI.Models;
using CRICXI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace CRICXI.Controllers
{
    public class MatchController : Controller
    {
        private readonly CricbuzzApiService _api;
        private readonly MatchService _matchService;

        public MatchController(CricbuzzApiService api, MatchService matchService)
        {
            _api = api;
            _matchService = matchService;
        }

        public async Task<IActionResult> Recent()
        {
            var data = await _api.GetRecentMatchesAsync();
            ViewBag.Json = data;
            return View();
        }

        public async Task<IActionResult> Upcoming()
        {
            var json = await _api.GetUpcomingMatchesAsync();
            var matchList = new List<Match>();

            using (JsonDocument doc = JsonDocument.Parse(json))
            {
                if (doc.RootElement.TryGetProperty("typeMatches", out var typeMatches))
                {
                    foreach (var type in typeMatches.EnumerateArray())
                    {
                        if (type.TryGetProperty("seriesMatches", out var seriesMatches))
                        {
                            foreach (var series in seriesMatches.EnumerateArray())
                            {
                                if (series.TryGetProperty("seriesAdWrapper", out var wrapper) &&
                                    wrapper.TryGetProperty("matches", out var matchArray))
                                {
                                    foreach (var m in matchArray.EnumerateArray())
                                    {
                                        if (m.TryGetProperty("matchInfo", out var matchInfo))
                                        {
                                            var team1 = matchInfo.GetProperty("team1").GetProperty("teamName").GetString();
                                            var team2 = matchInfo.GetProperty("team2").GetProperty("teamName").GetString();
                                            var matchDesc = matchInfo.GetProperty("matchDesc").GetString();
                                            var startDate = matchInfo.GetProperty("startDate").GetString();
                                            var cricbuzzMatchId = matchInfo.GetProperty("matchId").ToString();

                                            var match = new Match
                                            {
                                                CricbuzzMatchId = cricbuzzMatchId,
                                                TeamA = team1,
                                                TeamB = team2,
                                                MatchDesc = matchDesc,
                                                StartDate = startDate,
                                                Status = "Upcoming"
                                            };

                                            await _matchService.SaveIfNotExists(match);
                                            matchList.Add(match);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return View(matchList);
        }

        public async Task<IActionResult> LiveScore(string id)
        {
            var data = await _api.GetLiveScoreAsync(id);
            ViewBag.Json = data;
            return View("LiveScore");
        }
    }
}
