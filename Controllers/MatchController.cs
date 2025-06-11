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

        // ✅ This is purely for debug/testing purposes:
        public async Task<IActionResult> Recent()
        {
            var data = await _api.GetRecentMatchesAsync();
            ViewBag.Json = data;
            return View();
        }

        // ✅ This now simply loads upcoming matches from your synced database
        public async Task<IActionResult> Upcoming()
        {
            var matchList = await _matchService.GetAll();
            return View(matchList);
        }

        // ✅ LiveScore (optional)
        public async Task<IActionResult> LiveScore(string id)
        {
            var data = await _api.GetLiveScoreAsync(id);
            ViewBag.Json = data;
            return View("LiveScore");
        }
    }
}
