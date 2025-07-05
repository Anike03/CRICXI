using Microsoft.AspNetCore.Mvc;
using CRICXI.Services;
using CRICXI.Models;

namespace CRICXI.Controllers
{
    public class LeaderboardController : Controller
    {
        private readonly ContestEntryService _entryService;
        private readonly UserService _userService;

        public LeaderboardController(ContestEntryService entryService, UserService userService)
        {
            _entryService = entryService;
            _userService = userService;
        }

        // 🔹 Razor view: /Leaderboard (admin view)
        [HttpGet("/Leaderboard")]
        public async Task<IActionResult> Index()
        {
            var leaderboard = await BuildLeaderboard();
            ViewBag.Leaderboard = leaderboard;
            return View();
        }

        // 🔹 API JSON: /api/leaderboard (frontend consumption)
        [HttpGet("/api/leaderboard")]
        public async Task<IActionResult> GetLeaderboardData()
        {
            var leaderboard = await BuildLeaderboard();

            // Convert to anonymous object with Rank
            var ranked = leaderboard
                .OrderByDescending(e => e.JoinedContests)
                .Select((e, i) => new
                {
                    Rank = i + 1,
                    e.Username,
                    e.Email,
                    e.JoinedContests
                })
                .ToList();

            return Ok(ranked);
        }

        // 🔁 Shared logic
        private async Task<List<LeaderboardEntry>> BuildLeaderboard()
        {
            var users = await _userService.GetAllUsers();
            var entries = await _entryService.GetAll();

            var leaderboard = users
                .Select(u => new LeaderboardEntry
                {
                    Username = u.Username,
                    Email = u.Email,
                    JoinedContests = entries.Count(e => e.Username == u.Username)
                })
                .OrderByDescending(x => x.JoinedContests)
                .ToList();

            return leaderboard;
        }
    }
}
