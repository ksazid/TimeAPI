using System.Text.Encodings.Web;
using System.Threading.Tasks;
using TimeAPI.API.Services;

namespace TimeAPI.API.Extensions
{
    public static class SmsSenderExtensions
    {
        public static Task SendSmsConfirmationAsync(this ISmsSender smsSender, string phone, string link)
        {
            return smsSender.SendSmsAsync(phone, $"Please confirm your accountby clicking this link:", HtmlEncoder.Default.Encode(link));
        }

        public static Task SendSetupPasswordAsync(this ISmsSender smsSender, string phone, string link)
        {
            return smsSender.SendSmsAsync(phone, $"Please setup password your account by clicking this link:",  HtmlEncoder.Default.Encode(link));
        }
    }
}
