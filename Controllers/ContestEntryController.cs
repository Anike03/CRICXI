using CRICXI.Models;
using CRICXI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CRICXI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContestEntryController : ControllerBase
    {
        private readonly ContestEntryService _entryService;
        private readonly ContestService _contestService;
        private readonly UserService _userService;
        private readonly FantasyTeamService _teamService;

        public ContestEntryController(
            ContestEntryService entryService,
            ContestService contestService,
            UserService userService,
            FantasyTeamService teamService)
        {
            _entryService = entryService;
            _contestService = contestService;
            _userService = userService;
            _teamService = teamService;
        }

        // ✅ Join a contest with team–match validation
        [HttpPost("join")]
        public async Task<IActionResult> JoinContest([FromBody] ContestEntryRequest request)
        {
            var contest = await _contestService.GetById(request.ContestId);
            if (contest == null) return NotFound("Contest not found");

            var user = await _userService.GetByUsername(request.Username);
            if (user == null) return NotFound("User not found");

            var team = await _teamService.GetByIdAsync(request.TeamId);
            if (team == null) return BadRequest("Fantasy team not found");

            // Deduct balance
            var (success, newBalance) = await _userService.DeductBalance(
                user.Id,
                contest.EntryFee,
                $"Contest entry: {contest.Name} (Team: {team.TeamName})");

            if (!success)
            {
                return BadRequest(new
                {
                    message = "Insufficient balance",
                    currentBalance = newBalance
                });
            }

            // Create entry
            var entry = new ContestEntry
            {
                ContestId = contest.Id,
                MatchId = contest.MatchId,
                Username = user.Username,
                TeamId = team.Id,
                EntryDate = DateTime.UtcNow
            };

            await _entryService.Add(entry);

            return Ok(new
            {
                success = true,
                newBalance,
                message = "Successfully joined contest"
            });
        }

        public class ContestEntryRequest
        {
            public string ContestId { get; set; }
            public string TeamId { get; set; }
            public string Username { get; set; }
        }







        // ✅ Get all entries for a contest
        [HttpGet("by-contest/{contestId}")]
        public async Task<IActionResult> GetEntriesByContest(string contestId)
        {
            var entries = await _entryService.GetByContest(contestId);
            var result = new List<object>();

            foreach (var entry in entries)
            {
                var user = await _userService.GetByUsername(entry.Username);
                result.Add(new
                {
                    entryId = entry.Id,
                    username = entry.Username,
                    email = user?.Email ?? "(unknown)",
                    teamId = entry.TeamId
                });
            }

            return Ok(result);
        }

        // ✅ Delete a contest entry
        [HttpDelete("{entryId}")]
        public async Task<IActionResult> DeleteEntry(string entryId)
        {
            await _entryService.RemoveEntry(entryId);
            return Ok(new { message = "Entry removed." });
        }

        // ✅ Leaderboard (based on joined contests)
        [HttpGet("/api/leaderboard")]
        public async Task<IActionResult> GetLeaderboard()
        {
            var joinCounts = await _entryService.GetContestJoinCountsPerUser();
            var leaderboard = new List<LeaderboardEntry>();

            foreach (var (username, count) in joinCounts)
            {
                var user = await _userService.GetByUsername(username);
                leaderboard.Add(new LeaderboardEntry
                {
                    Username = username,
                    Email = user?.Email ?? "(unknown)",
                    JoinedContests = count
                });
            }

            leaderboard = leaderboard
                .OrderByDescending(x => x.JoinedContests)
                .Select((entry, index) => {
                    entry.Rank = index + 1;
                    return entry;
                }).ToList();

            return Ok(leaderboard);
        }
    }
}
