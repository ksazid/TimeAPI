using System.Threading.Tasks;

namespace TimeAPI.API.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
