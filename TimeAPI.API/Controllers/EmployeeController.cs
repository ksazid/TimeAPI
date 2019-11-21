using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TimeAPI.API.Models;
using TimeAPI.API.Models.EmployeeViewModels;
using TimeAPI.API.Services;
using TimeAPI.Domain;

namespace TimeAPI.API.Controllers
{
    [EnableCors("CorsPolicy")]
    [Route("[controller]")]
    public class EmployeeController : Controller
    {
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly ApplicationSettings _appSettings;
        private readonly IUnitOfWork _unitOfWork;
        public EmployeeController(IUnitOfWork unitOfWork, ILogger<EmployeeController> logger, 
            IEmailSender emailSender,
            ApplicationSettings appSettings)
        {
            _emailSender = emailSender;
            _logger = logger;
            _appSettings = appSettings;
            _unitOfWork = unitOfWork;
        }


        [EnableCors("CorsPolicy")]
        [HttpPost]
        [Route("AddEmployee")]
        public async Task<object> AddEmployee(EmployeeViewModel employeeViewModel)
        {
            //_unitOfWork.EmployeeRepository.Add()
            _unitOfWork.Commit();
            return BadRequest(new { message = "OOP! Please enter a valid user and password." });
        }

    }
}