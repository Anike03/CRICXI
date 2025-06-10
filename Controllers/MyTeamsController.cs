using CRICXI.Models;
using CRICXI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CRICXI.Controllers
{
    public class MyTeamsController : Controller
    {
        private readonly FantasyTeamService _teamService;

        public MyTeamsController(FantasyTeamService teamService)
        {
            _teamService = teamService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Auth");

            var teams = await _teamService.GetByUser(userId);
            return View(teams);
        }
    }
}
