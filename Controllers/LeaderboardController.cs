using CRICXI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CRICXI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeaderboardController : ControllerBase
    {
        private readonly LeaderboardService _leaderboardService;

        public LeaderboardController(LeaderboardService leaderboardService)
        {
            _leaderboardService = leaderboardService;
        }

        // ✅ Returns leaderboard of users sorted by number of contests joined
        [HttpGet]
        public async Task<IActionResult> GetJoinedLeaderboard()
        {
            var leaderboard = await _leaderboardService.GetLeaderboard();
            return Ok(leaderboard);
        }
    }
}
