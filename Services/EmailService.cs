using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;
using System;

namespace CRICXI.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlContent)
        {
            var apiKey = _config["SendGrid:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                Console.WriteLine("❌ SendGrid API key is missing!");
                return;
            }

            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(_config["SendGrid:SenderEmail"], _config["SendGrid:SenderName"]);
            var to = new EmailAddress(toEmail);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, null, htmlContent);

            var response = await client.SendEmailAsync(msg);
            var body = await response.Body.ReadAsStringAsync();

            Console.WriteLine($"✅ SendGrid Status Code: {response.StatusCode}");
            Console.WriteLine($"📩 SendGrid Response Body: {body}");
        }
    }
}
