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
        private readonly ILogger<UserController> _logger;

        public UserController(UserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpGet("{uid}/balance")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserBalanceByUid(string uid)
        {
            try
            {
                var user = await _userService.GetByUid(uid);

                if (user == null)
                {
                    if (!string.IsNullOrEmpty(HttpContext.Request.Query["email"]))
                    {
                        user = await _userService.GetByEmail(HttpContext.Request.Query["email"]);
                    }

                    if (user == null)
                        return NotFound("User not found");
                }

                return Ok(new { balance = user.WalletBalance });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching balance for UID: {Uid}", uid);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("deduct-balance")]
        public async Task<IActionResult> DeductBalance([FromBody] DeductBalanceRequest request)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during balance deduction for UserId: {UserId}", request.UserId);
                return StatusCode(500, "Internal server error during balance deduction");
            }
        }

        public class DeductBalanceRequest
        {
            public string UserId { get; set; } = null!;
            public decimal Amount { get; set; }
            public string? Description { get; set; }
        }
    }
}
