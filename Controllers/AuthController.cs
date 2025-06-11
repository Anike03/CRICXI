using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using CRICXI.Models;
using CRICXI.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

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

        // ✅ Standard Login (username + password)
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = await _userService.Authenticate(username, password);
            if (user == null)
            {
                ViewBag.Error = "Invalid username or password.";
                return View();
            }

            if (!user.IsEmailConfirmed)
            {
                ViewBag.Error = "Please verify your email before logging in.";
                return View();
            }

            HttpContext.Session.SetString("UserId", user.Id);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Role", user.Role);

            return RedirectToAction("Index", "Home");
        }

        // ✅ Registration (only for Users)
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
            user.Role = "User";

            var success = await _userService.Register(user, password);
            if (!success)
            {
                ViewBag.Error = "Username or Email already in use.";
                return View();
            }

            var verificationUrl = Url.Action("VerifyEmail", "Auth", new { token = token }, Request.Scheme);
            string htmlContent = $@"
                <h3>Welcome to CRICXI!</h3>
                <p>Click the link below to verify your email:</p>
                <p><a href='{verificationUrl}'>Verify My Email</a></p>";

            await _emailService.SendEmailAsync(user.Email, "Email Verification", htmlContent);

            ViewBag.Message = "Registration successful. Please check your email to verify your account.";
            return View("VerifyNotice");
        }

        // ✅ Email Verification
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

        // ✅ External Login Entry Point (Google, Facebook, X, Instagram)
        [HttpGet]
        public IActionResult ExternalLogin(string provider)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Auth");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, provider);
        }

        // ✅ External Login Callback
        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (!result.Succeeded)
                return RedirectToAction("Login");

            var externalEmail = result.Principal.FindFirstValue(ClaimTypes.Email);
            var externalName = result.Principal.Identity.Name;

            var user = await _userService.GetByEmail(externalEmail);
            if (user == null)
            {
                user = new User
                {
                    Username = externalName,
                    Email = externalEmail,
                    IsEmailConfirmed = true, // ✅ No verification needed for OAuth
                    Role = "User"
                };
                await _userService.RegisterWithoutPassword(user);
            }

            HttpContext.Session.SetString("UserId", user.Id);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Role", user.Role);

            return RedirectToAction("Index", "Home");
        }

        // ✅ Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
