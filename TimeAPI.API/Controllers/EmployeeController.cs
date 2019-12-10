﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using ailogica.Azure.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TimeAPI.API.Models;
using TimeAPI.API.Models.EmployeeProfileViewModels;
using TimeAPI.API.Models.EmployeeViewModels;
using TimeAPI.API.Services;
using TimeAPI.Domain;
using TimeAPI.Domain.Entities;

namespace TimeAPI.API.Controllers
{
    //[EnableCors("CorsPolicy")]
    [Route("[controller]")]
    //[Authorize]
    public class EmployeeController : Controller
    {
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly ApplicationSettings _appSettings;
        private readonly IUnitOfWork _unitOfWork;
        public IConfiguration _configuration;
        //public IHostingEnvironment _hostingEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;
        public EmployeeController(IUnitOfWork unitOfWork, ILogger<EmployeeController> logger, UserManager<ApplicationUser> userManager,
            IEmailSender emailSender,
           IOptions<ApplicationSettings> AppSettings, IConfiguration configuration,
           IHostingEnvironment hostingEnvironment)
        {
            _emailSender = emailSender;
            _logger = logger;
            _appSettings = AppSettings.Value;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            //_hostingEnvironment = hostingEnvironment;
            _userManager = userManager;
        }


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

                #region User
                string _userName = "";
                Role role = null;
                if (employeeViewModel.email != null)
                {
                    _userName = employeeViewModel.email;
                }
                if (employeeViewModel.phone != null)
                {
                    _userName = employeeViewModel.phone;
                }
                if (employeeViewModel.role_id != null)
                {
                    role = _unitOfWork.RoleRepository.Find(employeeViewModel.role_id.ToString());
                    if (role.NormalizedName == "ADMIN")
                        employeeViewModel.is_admin = true;
                }

                var user = new ApplicationUser()
                {
                    UserName = _userName,
                    Email = employeeViewModel.email,
                    FirstName = employeeViewModel.first_name,
                    LastName = employeeViewModel.last_name,
                    FullName = employeeViewModel.full_name,
                    Role = role.Name,
                    Phone = employeeViewModel.phone
                };

                string password = GeneratePassword();

                var result = await _userManager.CreateAsync(user, password).ConfigureAwait(true);
                var xRest = await _userManager.AddToRoleAsync(user, role.Name).ConfigureAwait(true);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    if (user.Email != null)
                    {
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user).ConfigureAwait(true);
                        var callbackUrl = Url.EmailConfirmationLink(user.Id, code, Request.Scheme);
                        await _emailSender.SendEmailConfirmationAsync(user.Email, callbackUrl, password).ConfigureAwait(true);
                    }
                    else
                    {
                        // check if its a phone 
                    }
                }

                #endregion User

                #region Employee

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<EmployeeViewModel, Employee>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Employee>(employeeViewModel);

                modal.id = Guid.NewGuid().ToString();
                modal.created_date = DateTime.Now.ToString(CultureInfo.CurrentCulture);
                modal.is_deleted = false;
                modal.user_id = user.Id;

                _unitOfWork.EmployeeRepository.Add(modal);

                #endregion

                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Employee registered succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }



        [HttpPatch]
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

                modal.modified_date = DateTime.Now.ToString(CultureInfo.CurrentCulture);

                _unitOfWork.EmployeeRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Employee updated succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }


        [HttpPost]
        [Route("RemoveEmployee")]
        public async Task<object> RemoveEmployee([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                _unitOfWork.EmployeeRepository.Remove(Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Employee removed succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }


        [HttpGet]
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


        [HttpPost]
        [Route("FindByEmpID")]
        public async Task<object> FindByEmpID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = _unitOfWork.EmployeeRepository.Find(Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }


        [HttpPost]
        [Route("FindByEmpName")]
        public async Task<object> FindByEmpName([FromBody] UtilsName UtilsName, CancellationToken cancellationToken)
        {

            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (UtilsName == null)
                    throw new ArgumentNullException(nameof(UtilsName));

                var result = _unitOfWork.EmployeeRepository.FindByEmpName(UtilsName.FullName);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }


        [HttpPost]
        [Route("FindByOrgID")]
        public async Task<object> FindByOrgID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = _unitOfWork.EmployeeRepository.FindByOrgIDCode(Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }


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


        [HttpPost]
        [Route("FindByRoleName")]
        public async Task<object> FindByRoleName([FromBody] UtilsRole UtilsRole, CancellationToken cancellationToken)
        {

            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (UtilsRole == null)
                    throw new ArgumentNullException(nameof(UtilsRole.Role));

                var result = _unitOfWork.EmployeeRepository.FindByRoleName(UtilsRole.Role);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }


        private static string GeneratePassword()
        {
            return Guid.NewGuid()
                .ToString("N")
                .ToLower()
                .Replace("1", "")
                .Replace("o", "")
                .Replace("0", "")
                .Substring(0, 8);
        }
    }
}
