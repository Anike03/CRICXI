using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace CRICXI.Controllers
{
    public class AuthController : Controller
    {
        public AuthController() { }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            if (email == "aniketsharma9360@gmail.com" && password == "Aaniket#00")
            {
                HttpContext.Session.SetString("Username", "Aniket");
                HttpContext.Session.SetString("Role", "Admin");

                TempData["LoginSuccess"] = "Login successful!";
                return RedirectToAction("LoginSuccess");
            }

            ViewBag.Error = "Invalid Admin Credentials!";
            return View();
        }

        [HttpGet]
        public IActionResult LoginSuccess()
        {
            if (TempData["LoginSuccess"] == null)
                return RedirectToAction("Login");

            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
