using CRICXI.Models;
using CRICXI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CRICXI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResultController : ControllerBase
    {
        private readonly LeaderboardService _leaderboardService;
        private readonly ContestService _contestService;
        private readonly MatchService _matchService;

        public ResultController(
            LeaderboardService leaderboardService,
            ContestService contestService,
            MatchService matchService)
        {
            _leaderboardService = leaderboardService;
            _contestService = contestService;
            _matchService = matchService;
        }

        // ✅ Admin uploads full match result (all player performances)
        [HttpPost("generate")]
        public async Task<IActionResult> GenerateLeaderboard([FromBody] ResultRequest request)
        {
            // Fetch contest to get total prize & matchId
            var contest = await _contestService.GetById(request.ContestId);
            if (contest == null)
                return NotFound("Contest not found");

            // Call leaderboard service to generate
            var leaderboard = await _leaderboardService.GenerateLeaderboard(
                contest.MatchId, request.MatchStats, contest.TotalPrize
            );

            return Ok(leaderboard);
        }
    }

    // ✅ Admin input DTO
    public class ResultRequest
    {
        public string ContestId { get; set; }
        public Dictionary<string, PlayerPerformance> MatchStats { get; set; }
    }
}
