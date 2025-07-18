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

        // ✅ Join a contest with Firebase UID and wallet check
        [HttpPost("join")]
        public async Task<IActionResult> JoinContest([FromBody] ContestEntryRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.TeamId))
                return BadRequest(new { success = false, message = "Invalid join request." });

            var contest = await _contestService.GetById(request.ContestId);
            if (contest == null)
                return NotFound(new { success = false, message = "Contest not found." });

            var user = await _userService.GetByUid(request.UserId);
            if (user == null && !string.IsNullOrEmpty(request.UserEmail))
            {
                user = await _userService.GetByEmail(request.UserEmail);
                if (user != null && string.IsNullOrEmpty(user.FirebaseUid))
                {
                    user.FirebaseUid = request.UserId;
                    await _userService.Update(user);
                }
            }

            if (user == null)
                return NotFound(new { success = false, message = "User not found." });

            // Check if user already joined
            if (await _entryService.HasUserJoined(request.ContestId, user.Username))
                return BadRequest(new { success = false, message = "User already joined this contest." });

            // Validate fantasy team
            var team = await _teamService.GetByIdAsync(request.TeamId);
            if (team == null || team.MatchId != contest.MatchId)
                return BadRequest(new { success = false, message = "Invalid team for this match." });

            // Wallet balance check
            if (user.WalletBalance < contest.EntryFee)
                return BadRequest(new
                {
                    success = false,
                    message = "Insufficient wallet balance.",
                    currentBalance = user.WalletBalance
                });

            // Deduct balance
            var success = await _userService.UpdateWallet(user.Id, contest.EntryFee, addFunds: false);
            if (!success)
                return BadRequest(new { success = false, message = "Failed to deduct balance." });

            // Add contest entry
            var entry = new ContestEntry
            {
                ContestId = contest.Id,
                MatchId = contest.MatchId,
                Username = user.Username,
                TeamId = request.TeamId,
                JoinedAt = DateTime.UtcNow,
                Score = 0
            };
            await _entryService.Add(entry);

            return Ok(new
            {
                success = true,
                newBalance = user.WalletBalance - contest.EntryFee,
                message = "Successfully joined contest."
            });
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

        // ✅ Leaderboard endpoint
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
                .Select((entry, index) =>
                {
                    entry.Rank = index + 1;
                    return entry;
                }).ToList();

            return Ok(leaderboard);
        }

        // ✅ Request model
        public class ContestEntryRequest
        {
            public string ContestId { get; set; } = null!;
            public string TeamId { get; set; } = null!;
            public string UserId { get; set; } = null!;
            public string? UserEmail { get; set; }
        }
    }
}
