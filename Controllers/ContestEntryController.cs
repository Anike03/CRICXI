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

        [HttpPost("join")]
        public async Task<IActionResult> JoinContest([FromBody] JoinRequest request)
        {
            // Validate wallet balance, entry fees, existing join status etc
            decimal entryFee = 50; // This should be dynamic from your contest data

            var user = await _userService.GetByUsername(request.Username);
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
    }

    public class JoinRequest
    {
        public string Username { get; set; }
        public string MatchId { get; set; }
        public string ContestId { get; set; }
        public string TeamId { get; set; }
    }

}
