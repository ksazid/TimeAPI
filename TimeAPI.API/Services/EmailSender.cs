using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Threading.Tasks;

namespace TimeAPI.API.Services
{
    // This class is used by the application to send email for account confirmation and password reset.
    // For more details see https://go.microsoft.com/fwlink/?LinkID=532713
    public class EmailSender : IEmailSender
    {
        private IConfiguration _Configuration;
        public EmailSender(IConfiguration configuration)
        {
            _Configuration = configuration;
        }
        public Task SendEmailAsync(string email, string subject, string message)
        {
            var apikey = _Configuration.GetValue<string>(key: "SendGridApiKey");
            Execute(apikey, email, subject, message).Wait();
            return Task.CompletedTask;
        }

        static async Task Execute(string apikey, string email, string subject, string message)
        {
            var client = new SendGridClient(apikey);
            var from = new EmailAddress("sazid@interfuture.ae", "Enforce");
            var sub = subject;
            var to = new EmailAddress(email);
            var plainTextContent = message;
            var htmlContent = "<strong>" + message + "</strong>";
            var msg = MailHelper.CreateSingleEmail(from, to, sub, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg).ConfigureAwait(true);
        }
    }
}
