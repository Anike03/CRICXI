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
            var user = await _userService.GetByUid(uid);
            if (user == null)
                return NotFound("User not found");

            return Ok(new { balance = user.WalletBalance, from = "UserController" });
        }
    }
}
