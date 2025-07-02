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
        private readonly UserService _userService;

        public ContestEntryController(ContestEntryService entryService, UserService userService)
        {
            _entryService = entryService;
            _userService = userService;
        }

        // ✅ API: Join a contest
        [HttpPost("join")]
        public async Task<IActionResult> JoinContest([FromBody] JoinRequest request)
        {
            // ✅ Wallet validation should be handled on frontend/backend jointly
            decimal entryFee = 50; // ideally should be passed or fetched from Contest

            var user = await _userService.GetByUsername(request.Username);
            if (user == null)
                return NotFound("User not found");

            if (user.WalletBalance < entryFee)
                return BadRequest("Insufficient balance.");

            await _userService.UpdateWallet(user.Id, entryFee, addFunds: false);
            await _entryService.Add(new ContestEntry
            {
                ContestId = request.ContestId,
                MatchId = request.MatchId,
                Username = request.Username,
                TeamId = request.TeamId
            });

            return Ok();
        }

        // ✅ API: Get all joined users for a specific contest (for Admin Details.cshtml)
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

        // ✅ API: Delete a user's entry from a contest
        [HttpDelete("{entryId}")]
        public async Task<IActionResult> DeleteEntry(string entryId)
        {
            await _entryService.RemoveEntry(entryId);
            return Ok(new { message = "Entry removed." });
        }
    }

    public class JoinRequest
    {
        public string Username { get; set; }
        public string MatchId { get; set; }
        public string ContestId { get; set; }
        public string TeamId { get; set; }
    }
}
