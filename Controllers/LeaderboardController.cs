using Microsoft.AspNetCore.Mvc;
using CRICXI.Services;

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

        public async Task<IActionResult> Index()
        {
            var users = await _userService.GetAllUsers();
            var entries = await _entryService.GetAll();

            var leaderboard = users.Select(u => new
            {
                u.Username,
                u.Email,
                JoinedContests = entries.Count(e => e.Username == u.Username)
            })
            .OrderByDescending(x => x.JoinedContests)
            .Select((x, i) => new
            {
                Rank = i + 1,
                x.Username,
                x.Email,
                x.JoinedContests
            }).ToList();

            ViewBag.Leaderboard = leaderboard;
            return View();
        }
        [HttpGet("/api/leaderboard")]
        public async Task<IActionResult> GetLeaderboardData()
        {
            var users = await _userService.GetAllUsers();
            var entries = await _entryService.GetAll();

            var leaderboard = users.Select(u => new
            {
                u.Username,
                u.Email,
                JoinedContests = entries.Count(e => e.Username == u.Username)
            })
            .OrderByDescending(x => x.JoinedContests)
            .Select((x, i) => new
            {
                Rank = i + 1,
                x.Username,
                x.Email,
                x.JoinedContests
            }).ToList();

            return Ok(leaderboard);
        }

    }
}
