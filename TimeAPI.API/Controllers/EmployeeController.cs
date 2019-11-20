using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TimeAPI.API.Models;
using TimeAPI.API.Services;

namespace TimeAPI.API.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly ApplicationSettings _appSettings;
        public EmployeeController()
        {

        }
    }
}