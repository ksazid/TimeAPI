using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace TimeAPI.API.Services
{
    public static class EmailSenderExtensions
    {
        public static Task SendEmailConfirmationAsync(this IEmailSender emailSender, string email, string link)
        {
            return emailSender.SendEmailAsync(email, "Verify your email",
                $"Please confirm your account by clicking this link: <a href='{HtmlEncoder.Default.Encode(link)}'>Link</a>");
        }

        public static Task SendSetupPasswordAsync(this IEmailSender emailSender, string email, string link)
        {
            return emailSender.SendEmailAsync(email, "Setup Password For Login",
                $"Please setup password your account by clicking this link: <a href='{HtmlEncoder.Default.Encode(link)}'>Link</a>");
        }
    }
}
