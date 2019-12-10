using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace TimeAPI.API.Services
{
    public static class EmailSenderExtensions
    {
        public static Task SendEmailConfirmationAsync(this IEmailSender emailSender, string email, string link, string password)
        {
            return emailSender.SendEmailAsync(email, "Verify your email",
                $"Please confirm your account by clicking this link: <a href='{HtmlEncoder.Default.Encode(link)}'>Link</a> <br> Temporary Password : " + password + "");
        }
    }
}
