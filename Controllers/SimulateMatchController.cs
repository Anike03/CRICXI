using CRICXI.Models;
using CRICXI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CRICXI.Controllers
{
    public class SimulateMatchController : Controller
    {
        private readonly LeaderboardService _leaderboardService;

        public SimulateMatchController(LeaderboardService leaderboardService)
        {
            _leaderboardService = leaderboardService;
        }

        public async Task<IActionResult> Run(string contestId)
        {
            // Simulated match stats — these would come from the API in real use
            var playerPoints = new Dictionary<string, int>
            {
                { "1", 75 }, { "2", 30 }, { "3", 60 }, { "4", 25 },
                { "5", 80 }, { "6", 10 }, { "7", 35 }, { "8", 15 },
                { "9", 40 }, { "10", 50 }, { "11", 55 }
            };

            // You would dynamically assign captain/vice roles per team;
            // here is a basic placeholder role map
            var roleMap = new Dictionary<string, (bool isCaptain, bool isViceCaptain)>
            {
                { "5", (true, false) },
                { "3", (false, true) }
            };

            int totalPrize = 1000;

            var leaderboard = await _leaderboardService.GenerateLeaderboard(contestId, playerPoints, roleMap, totalPrize);

            return View("SimulationResult", leaderboard);
        }
    }
}
