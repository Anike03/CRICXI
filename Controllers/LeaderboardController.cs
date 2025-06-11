using CRICXI.Models;
using CRICXI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CRICXI.Controllers
{
    public class LeaderboardController : Controller
    {
        private readonly LeaderboardService _leaderboardService;
        private readonly ContestService _contestService;
        private readonly MatchService _matchService;

        public LeaderboardController(
            LeaderboardService leaderboardService,
            ContestService contestService,
            MatchService matchService)
        {
            _leaderboardService = leaderboardService;
            _contestService = contestService;
            _matchService = matchService;
        }

        public async Task<IActionResult> Contest(string contestId)
        {
            // Fetch contest info
            var contest = await _contestService.GetById(contestId);
            var match = await _matchService.GetById(contest.MatchId);
            ViewBag.Match = match;
            ViewBag.Contest = contest;

            // TODO: Replace this with real API fetched data when we hook live Cricbuzz data:
            var matchStats = new Dictionary<string, PlayerPerformance>
            {
                { "1", new PlayerPerformance { Runs = 50, Fours = 5, Sixes = 2, BallsFaced = 30, Wickets = 1, OversBowled = 2, RunsConceded = 20, Catches = 1 } },
                { "2", new PlayerPerformance { Runs = 20, Fours = 2, Sixes = 1, BallsFaced = 12, Wickets = 2, OversBowled = 4, RunsConceded = 24, Catches = 0 } },
                { "3", new PlayerPerformance { Runs = 0, Fours = 0, Sixes = 0, BallsFaced = 1, Wickets = 3, OversBowled = 3, RunsConceded = 18, Catches = 1 } }
            };

            // Generate full leaderboard
            var leaderboard = await _leaderboardService.GenerateLeaderboard(
                contest.MatchId, matchStats, contest.TotalPrize
            );

            return View(leaderboard);
        }
    }
}
