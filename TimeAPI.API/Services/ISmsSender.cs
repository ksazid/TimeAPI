using System.Threading.Tasks;

namespace TimeAPI.API.Services
{
    public interface ISmsSender
    {
        Task SendSmsAsync(string phone, string url, string message);
    }
}
