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
            Console.WriteLine($"Received UID: {uid}"); // Check your server logs
            var user = await _userService.GetByUid(uid);
            if (user == null)
            {
                Console.WriteLine("No user found with this UID"); // Additional logging
                return NotFound("User not found");
            }

            return Ok(new { balance = user.WalletBalance, from = "UserController" });
        }
    }
}
