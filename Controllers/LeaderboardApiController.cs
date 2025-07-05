using Microsoft.AspNetCore.Mvc;
using CRICXI.Services;
using CRICXI.Models;

namespace CRICXI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeaderboardApiController : ControllerBase
    {
        private readonly ContestEntryService _entryService;
        private readonly UserService _userService;

        public LeaderboardApiController(ContestEntryService entryService, UserService userService)
        {
            _entryService = entryService;
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetLeaderboard()
        {
            var users = await _userService.GetAllUsers();
            var entries = await _entryService.GetAll();

            var leaderboard = users
                .Select(u => new
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
                })
                .ToList();

            return Ok(leaderboard);
        }
    }
}
