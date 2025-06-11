using Microsoft.AspNetCore.Mvc;
using CRICXI.Services;

namespace CRICXI.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserService _userService;

        public HomeController(UserService userService)
        {
            _userService = userService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetWalletBalance()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return Json("0");
            }

            var user = await _userService.GetByUsername(username);
            if (user == null)
            {
                return Json("0");
            }

            // Format properly for dollar display
            return Json(user.WalletBalance.ToString("0.00"));
        }
    }
}
