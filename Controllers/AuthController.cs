using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using CRICXI.Models;
using CRICXI.Services;

namespace CRICXI.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserService _userService;
        private readonly EmailService _emailService;

        public AuthController(UserService userService, EmailService emailService)
        {
            _userService = userService;
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(User user, string password)
        {
            if (string.IsNullOrWhiteSpace(user.Email) || string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "All fields are required.";
                return View();
            }

            user.Email = user.Email.Trim();
            user.Username = user.Username.Trim();
            var token = Guid.NewGuid().ToString();
            user.EmailVerificationToken = token;
            user.IsEmailConfirmed = false;

            var success = await _userService.Register(user, password);
            if (!success)
            {
                ViewBag.Error = "Username or Email already in use!";
                return View();
            }

            var verificationUrl = Url.Action("VerifyEmail", "Auth", new { token = token }, Request.Scheme);
            string htmlContent = $@"
                <h3>Welcome to CRICXI!</h3>
                <p>Thank you for registering.</p>
                <p>Please verify your email by clicking the link below:</p>
                <p><a href='{verificationUrl}'>Verify My Email</a></p>";

            await _emailService.SendEmailAsync(user.Email, "CRICXI Email Verification", htmlContent);

            ViewBag.Message = "Registration successful. Please check your email to verify your account.";
            return View("VerifyNotice");
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Username and Password are required.";
                return View();
            }

            var user = await _userService.Authenticate(username.Trim(), password);
            if (user == null)
            {
                ViewBag.Error = "Invalid credentials.";
                return View();
            }

            if (!user.IsEmailConfirmed)
            {
                ViewBag.Error = "Please verify your email before logging in.";
                return View();
            }

            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Role", user.Role);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        [HttpGet]
        public async Task<IActionResult> VerifyEmail(string token)
        {
            var user = await _userService.GetByVerificationToken(token);
            if (user == null)
                return NotFound("Invalid or expired verification token.");

            user.IsEmailConfirmed = true;
            user.EmailVerificationToken = null;
            await _userService.Update(user);

            return View("VerifySuccess");
        }
    }
}
