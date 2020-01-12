using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Threading.Tasks;
using TimeAPI.API.Models;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace TimeAPI.API.Services
{
    // This class is used by the application to send email for account confirmation and password reset.
    // For more details see https://go.microsoft.com/fwlink/?LinkID=532713
    public class SmsSender : ISmsSender
    {
        private IConfiguration _Configuration;
        public SmsSender(IConfiguration configuration)
        {
            _Configuration = configuration;
        }
        public Task SendSmsAsync(string phone,  string message, string url)
        {
            var apikey = _Configuration.GetValue<string>(key: "BitlyAPIKey");
            var Shorten = UserHelpers.ShortenAsync(url, apikey);
            Execute(phone, message, Shorten.Result);
            return Task.CompletedTask;
        }
        static void Execute(string phone, string message, string url)
        {
            phone = "+" + phone;
            const string accountSid = "ACc574d18f169071a8b477170e3b867b1c";
            const string authToken = "b46ffb4385b38a0502d25f26d6250ee2";

            TwilioClient.Init(accountSid, authToken);

            try
            {
                var _MessageResource = MessageResource.Create(
                                body: message + " " + url,
                                from: new Twilio.Types.PhoneNumber("+14157994530"),
                                statusCallback: new Uri("http://postb.in/1234abcd"),
                                to: new Twilio.Types.PhoneNumber(phone));

                var dx = _MessageResource.Sid;
            }
            catch (Exception ex)
            {
                throw;
            }

        }
    }
}
