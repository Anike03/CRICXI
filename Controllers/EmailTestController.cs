using Microsoft.AspNetCore.Mvc;
using CRICXI.Services;

namespace CRICXI.Controllers
{
    public class EmailTestController : Controller
    {
        private readonly EmailService _emailService;

        public EmailTestController(EmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpGet("/email-test")]
        public async Task<IActionResult> SendTest()
        {
            await _emailService.SendEmailAsync("yourreceiver@gmail.com", "CRICXI Test Email", "<h2>This is a test email.</h2>");
            return Content("Email test triggered. Check your inbox or spam.");
        }
    }
}
