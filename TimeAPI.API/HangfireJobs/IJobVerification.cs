using AutoMapper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TimeAPI.API.HangfireJobs
{
    interface IJobVerification
    {
        Task<object> SendVerficationEmailConfirmationAsync();
    }
}
