using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TimeAPI.API.Models;
using TimeAPI.API.Models.EmployeeViewModels;
using TimeAPI.API.Services;
using TimeAPI.Domain;
using TimeAPI.Domain.Entities;

namespace TimeAPI.API.Controllers
{
    [EnableCors("CorsPolicy")]
    [Route("[controller]")]
    public class OrganizationController : Controller
    {
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly ApplicationSettings _appSettings;
        private readonly IUnitOfWork _unitOfWork;
        public OrganizationController(IUnitOfWork unitOfWork, ILogger<EmployeeController> logger,
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
        public async Task<object> AddOrganization([FromBody] EmployeeViewModel employeeViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (employeeViewModel == null)
                    throw new ArgumentNullException(nameof(employeeViewModel));

                employeeViewModel.id = Guid.NewGuid().ToString();
                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<EmployeeViewModel, Employee>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Employee>(employeeViewModel);


                _unitOfWork.EmployeeRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Employee registered succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }
    }
}
