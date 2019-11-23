using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TimeAPI.API.Models;
using TimeAPI.API.Models.EmployeeViewModels;
using TimeAPI.API.Services;
using TimeAPI.Domain;
using TimeAPI.Domain.Entities;

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
           IOptions<ApplicationSettings> AppSettings)
        {
            _emailSender = emailSender;
            _logger = logger;
            _appSettings = AppSettings.Value;
            _unitOfWork = unitOfWork;
        }


        [EnableCors("CorsPolicy")]
        [HttpPost]
        [Route("AddEmployee")]
        public async Task<object> AddEmployee([FromBody] EmployeeViewModel employeeViewModel, CancellationToken cancellationToken)
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


        [EnableCors("CorsPolicy")]
        [HttpPost]
        [Route("UpdateEmployee")]
        public async Task<object> UpdateEmployee([FromBody] EmployeeViewModel employeeViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (employeeViewModel == null)
                    throw new ArgumentNullException(nameof(employeeViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<EmployeeViewModel, Employee>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Employee>(employeeViewModel);


                _unitOfWork.EmployeeRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Employee updated succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }


        [EnableCors("CorsPolicy")]
        [HttpPost]
        [Route("RemoveEmployee")]
        public async Task<object> RemoveEmployee([FromBody] Utils _Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (_Utils == null)
                    throw new ArgumentNullException(nameof(_Utils.ID));

                _unitOfWork.EmployeeRepository.Remove(_Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Employee removed succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }



        [EnableCors("CorsPolicy")]
        [HttpPost]
        [Route("FindByEmpName")]
        public async Task<object> FindByEmpName([FromBody] UtilsName _UtilsName, CancellationToken cancellationToken)
        {
         
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (_UtilsName == null)
                    throw new ArgumentNullException(nameof(_UtilsName));

                var result = _unitOfWork.EmployeeRepository.FindByEmpName(_UtilsName.FullName);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }


        [EnableCors("CorsPolicy")]
        [HttpPost]
        [Route("FindByOrgID")]
        public async Task<object> FindByOrgID([FromBody] Utils _Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (_Utils == null)
                    throw new ArgumentNullException(nameof(_Utils.ID));

                var result = _unitOfWork.EmployeeRepository.FindByOrgIDCode(_Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }


        [EnableCors("CorsPolicy")]
        [HttpPost]
        [Route("FindByEmpCode")]
        public async Task<object> FindByEmpCode([FromBody] UtilsCode _EmpCode, CancellationToken cancellationToken)
        {

            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (_EmpCode == null)
                    throw new ArgumentNullException(nameof(_EmpCode.Code));

                var result = _unitOfWork.EmployeeRepository.FindByEmpCode(_EmpCode.Code);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }


        [EnableCors("CorsPolicy")]
        [HttpPost]
        [Route("FindByRoleName")]
        public async Task<object> FindByRoleName([FromBody] UtilsRole _UtilsRole, CancellationToken cancellationToken)
        {
          
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (_UtilsRole == null)
                    throw new ArgumentNullException(nameof(_UtilsRole.Role));

                var result = _unitOfWork.EmployeeRepository.FindByRoleName(_UtilsRole.Role);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }


        [EnableCors("CorsPolicy")]
        [HttpPost]
        [Route("GetAllEmployees")]
        public async Task<object> GetAllEmployees(CancellationToken cancellationToken)
        {

            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.EmployeeRepository.All();
                _unitOfWork.Commit();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }
    }
}
