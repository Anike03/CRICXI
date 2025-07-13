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
        public async Task<IActionResult> JoinContest([FromBody] ContestEntry entry)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.TeamId))
                return BadRequest("Invalid entry data.");

            var contest = await _contestService.GetById(entry.ContestId);
            if (contest == null)
                return NotFound("Contest not found");

            var alreadyJoined = await _entryService.HasUserJoined(entry.ContestId, entry.Username);
            if (alreadyJoined)
                return BadRequest("User already joined this contest");

            var joinedCount = await _entryService.GetJoinedCount(entry.ContestId);
            if (joinedCount >= contest.MaxParticipants)
                return BadRequest("Contest is full");

            var user = await _userService.GetByUsername(entry.Username);
            if (user == null || string.IsNullOrEmpty(user.Id))
                return NotFound("User not found");

            if (user.WalletBalance < contest.EntryFee)
                return BadRequest("Insufficient wallet balance");

            // Fetch the team and validate match ID
            var team = await _teamService.GetByIdAsync(entry.TeamId);
            if (team == null)
                return BadRequest("Fantasy team not found.");

            if (team.MatchId != contest.MatchId)
                return BadRequest("You cannot join this contest with a team from a different match.");

            // Deduct balance and join
            var success = await _userService.UpdateWallet(user.Id, contest.EntryFee, addFunds: false);
            if (!success)
                return BadRequest("Failed to deduct balance");

            await _entryService.Add(entry);
            return Ok("Joined successfully");
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
