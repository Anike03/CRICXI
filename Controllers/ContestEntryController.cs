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

        public ContestEntryController(
            ContestEntryService entryService,
            ContestService contestService,
            UserService userService)
        {
            _entryService = entryService;
            _contestService = contestService;
            _userService = userService;
        }

        // ✅ Join a contest using Firebase UID
        [HttpPost("join")]
        public async Task<IActionResult> JoinContest([FromBody] ContestEntryRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.TeamId))
                return BadRequest(new { success = false, message = "Invalid join request." });

            // Fetch contest
            var contest = await _contestService.GetById(request.ContestId);
            if (contest == null)
                return NotFound(new { success = false, message = "Contest not found." });

            // Fetch user via Firebase UID
            var user = await _userService.GetByUid(request.UserId);
            if (user == null)
                return NotFound(new { success = false, message = "User not found." });

            // Check if already joined
            var alreadyJoined = await _entryService.HasUserJoined(request.ContestId, user.Username);
            if (alreadyJoined)
                return BadRequest(new { success = false, message = "User already joined this contest." });

            // Check wallet balance
            if (user.WalletBalance < contest.EntryFee)
                return BadRequest(new
                {
                    success = false,
                    message = "Insufficient wallet balance.",
                    currentBalance = user.WalletBalance
                });

            // Deduct wallet balance
            var success = await _userService.UpdateWallet(user.Id, contest.EntryFee, addFunds: false);
            if (!success)
                return BadRequest(new { success = false, message = "Failed to deduct balance." });

            // Add entry (TeamId is Firebase team ID)
            var entry = new ContestEntry
            {
                ContestId = contest.Id,
                MatchId = contest.MatchId,
                Username = user.Username, // Username from MongoDB
                TeamId = request.TeamId,
                JoinedAt = DateTime.UtcNow,
                Score = 0
            };
            await _entryService.Add(entry);

            return Ok(new
            {
                success = true,
                newBalance = user.WalletBalance - contest.EntryFee,
                message = "Successfully joined contest"
            });
        }

        public class ContestEntryRequest
        {
            public string ContestId { get; set; } = null!;
            public string TeamId { get; set; } = null!;
            public string UserId { get; set; } = null!; // Firebase UID
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
                .Select((entry, index) =>
                {
                    entry.Rank = index + 1;
                    return entry;
                }).ToList();

            return Ok(leaderboard);
        }
    }
}
