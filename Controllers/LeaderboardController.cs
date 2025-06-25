using CRICXI.Models;
using CRICXI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CRICXI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeaderboardController : ControllerBase
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

        // ✅ This API will calculate and return leaderboard for contest
        [HttpGet("contest/{contestId}")]
        public async Task<IActionResult> GetLeaderboard(string contestId)
        {
            // Fetch contest info
            var contest = await _contestService.GetById(contestId);
            var match = await _matchService.GetById(contest.MatchId);

            // ✅ This will be replaced with real fetched stats later when connected to live API:
            var matchStats = new Dictionary<string, PlayerPerformance>
            {
                { "1", new PlayerPerformance { Runs = 50, Fours = 5, Sixes = 2, BallsFaced = 30, Wickets = 1, OversBowled = 2, RunsConceded = 20, Catches = 1 } },
                { "2", new PlayerPerformance { Runs = 20, Fours = 2, Sixes = 1, BallsFaced = 12, Wickets = 2, OversBowled = 4, RunsConceded = 24, Catches = 0 } },
                { "3", new PlayerPerformance { Runs = 0, Fours = 0, Sixes = 0, BallsFaced = 1, Wickets = 3, OversBowled = 3, RunsConceded = 18, Catches = 1 } }
            };

            var leaderboard = await _leaderboardService.GenerateLeaderboard(
                contest.MatchId, matchStats, contest.TotalPrize
            );

            return Ok(leaderboard);
        }
    }
}
