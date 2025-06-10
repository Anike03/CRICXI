using CRICXI.Models;
using CRICXI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CRICXI.Controllers
{
    public class LeaderboardController : Controller
    {
        private readonly LeaderboardService _leaderboardService;

        public LeaderboardController(LeaderboardService leaderboardService)
        {
            _leaderboardService = leaderboardService;
        }

        public async Task<IActionResult> Contest(string contestId)
        {
            // Dummy data — you’ll later fetch this dynamically from API/DB
            var playerPoints = new Dictionary<string, int>
            {
                { "1", 60 }, { "2", 30 }, { "3", 50 }, { "4", 40 },
                { "5", 70 }, { "6", 20 }, { "7", 15 }, { "8", 25 },
                { "9", 10 }, { "10", 30 }, { "11", 55 }
            };

            var roleMap = new Dictionary<string, (bool isCaptain, bool isViceCaptain)>
            {
                { "5", (true, false) },
                { "3", (false, true) }
            };

            var totalPrize = 1000;

            var leaderboard = await _leaderboardService.GenerateLeaderboard(contestId, playerPoints, roleMap, totalPrize);

            return View("ContestLeaderboard", leaderboard);
        }
    }
}
