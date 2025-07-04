using CRICXI.Models;
using CRICXI.Services;
using Microsoft.AspNetCore.Mvc;

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

        // ✅ Razor View (for admin/debug)
        public async Task<IActionResult> Upcoming()
        {
            var allMatches = await _matchService.GetAll();

            var upcoming = allMatches
                .Where(m => m.StartDate > DateTime.UtcNow)
                .OrderBy(m => m.StartDate)
                .ToList();

            return View(upcoming);
        }

        // ✅ Public API: Return simplified match JSON for frontend
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
                    matchId = m.CricbuzzMatchId,
                    team1 = m.TeamA,
                    team2 = m.TeamB,
                    matchDesc = m.MatchDesc,
                    venue = m.Status, // Change to m.Venue if your model has a separate venue field
                    startDate = m.StartDate
                })
                .ToList();

            return Json(upcoming);
        }

        // ✅ Optional LiveScore (API passthrough)
        public async Task<IActionResult> LiveScore(string id)
        {
            var data = await _api.GetLiveScoreAsync(id);
            ViewBag.Json = data;
            return View("LiveScore");
        }

        // ✅ Optional Debug/Test endpoint for recent matches
        public async Task<IActionResult> Recent()
        {
            var data = await _api.GetRecentMatchesAsync();
            ViewBag.Json = data;
            return View();
        }
    }
}
