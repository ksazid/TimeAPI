using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimeAPI.Data;
using TimeAPI.Data.Repositories;
using TimeAPI.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using TimeAPI.API.Services;
using System.Security.Policy;
using Microsoft.IdentityModel.Tokens;

namespace TimeAPI.API.HangfireJobs
{
    public class JobVerification : IJobVerification
    {
        private IConfiguration _configuration;
        private readonly IEmailSender _emailSender;
        public JobVerification(IConfiguration configuration, IEmailSender emailSender)
        {
            _configuration = configuration;
            _emailSender = emailSender;
        }
        public async Task<object> SendVerficationEmailConfirmationAsync()
        {
            DateTime dateTime = InternetTime.GetCurrentTimeFromTimeZone().Value.DateTime;
            using (var unitOfWork = new DapperUnitOfWork(_configuration.GetConnectionString("DefaultConnection")))
            {
                List<AttendedEmployee> AttendedEmployeeList = new List<AttendedEmployee>();
                var xResult = unitOfWork.AdminDashboardRepository
                    .GetAllSingleCheckInEmployeesForHangFireJobs("44919b38-176e-45ce-9b12-db5faef620d6",
                                                                dateTime.ToString(), dateTime.ToString());

                AttendedEmployeeList.AddRange(xResult);
                var REST = AttendedEmployeeList.Cast<AttendedEmployee>().ToList();

                for (int i = 0; i < REST.Count; i++)
                {
                    DateTime workingHours =  Convert.ToDateTime(REST[i].check_in).AddHours(9).AddMinutes(-30);

                    if ((DateTime.Now.Hour == workingHours.Hour && DateTime.Now.Minute == workingHours.Minute) || DateTime.Now == workingHours)
                    {
                        //send mail
                        var code = "stsdfgsdgf";
                        var xcode = Base64UrlEncoder.Encode(code);
                        return System.Threading.Tasks.Task.FromResult<object>(_emailSender.SendEmailAsync(REST[i].groupid, "Reset Password", $"Please reset your password by clicking here: <a href='{xcode}'>link</a>")).ConfigureAwait(true);
                    }
                }
                //var data = System.Threading.Tasks.Task.FromResult<object>(JsonConvert.SerializeObject(xResult, Formatting.Indented)).ConfigureAwait(false);
                return null;
            }
        }




    }
}
