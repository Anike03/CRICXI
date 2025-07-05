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

        // 🔹 Razor view: /Leaderboard
        [HttpGet("/Leaderboard")]
        public async Task<IActionResult> Index()
        {
            var leaderboard = await BuildLeaderboard();
            ViewBag.Leaderboard = leaderboard;
            return View();
        }

        // 🔹 API JSON endpoint: /api/leaderboard
        [HttpGet("/api/leaderboard")]
        public async Task<IActionResult> GetLeaderboardData()
        {
            var leaderboard = await BuildLeaderboard();
            return Ok(leaderboard);
        }

        // 🔁 Shared logic for computing leaderboard
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

            // ✅ Add ranks
            for (int i = 0; i < leaderboard.Count; i++)
            {
                leaderboard[i].Rank = i + 1;
            }

            return leaderboard;
        }
    }
}
