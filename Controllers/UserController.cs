using CRICXI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace CRICXI.Controllers
{
    [ApiController]
    [Route("api/users")]
    [EnableCors("AllowReact")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet("{uid}/balance")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserBalanceByUid(string uid)
        {
            Console.WriteLine($"Received UID: {uid}");
            var user = await _userService.GetByUid(uid);
            if (user == null)
                return NotFound("User not found");

            return Ok(new { balance = user.WalletBalance, from = "UserController" });
        }

        [HttpPost("deduct-balance")]
        public async Task<IActionResult> DeductBalance([FromBody] DeductBalanceRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.UserId))
                return BadRequest("User ID required");

            if (request.Amount <= 0)
                return BadRequest("Amount must be positive");

            var (success, _) = await _userService.DeductBalance(
                request.UserId,
                request.Amount,
                request.Description ?? "Contest entry fee");

            if (!success)
                return BadRequest("Failed to deduct balance - insufficient funds or user not found");

            var updatedUser = await _userService.GetById(request.UserId);
            if (updatedUser == null)
                return NotFound("User not found after update");

            return Ok(new
            {
                success = true,
                newBalance = updatedUser.WalletBalance
            });
        }

        public class DeductBalanceRequest
        {
            public string UserId { get; set; } = null!;
            public decimal Amount { get; set; }
            public string? Description { get; set; }
        }
    }
}
