using System;
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
using Newtonsoft.Json;
using TimeAPI.API.Extensions;
using TimeAPI.API.Models;
using TimeAPI.API.Models.EmployeeViewModels;
using TimeAPI.API.Services;
using TimeAPI.Domain;
using TimeAPI.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace TimeAPI.API.Controllers
{
    //[EnableCors("CorsPolicy")]
    [Route("[controller]")]
    //[Authorize]
    public class EmployeeController : Controller
    {
        private readonly IEmailSender _emailSender;
        private readonly ISmsSender _smsSender;
        private readonly ILogger _logger;
        private readonly ApplicationSettings _appSettings;
        private readonly IUnitOfWork _unitOfWork;
        private IConfiguration _configuration;
        private static string _userName = string.Empty;
        private readonly UserManager<ApplicationUser> _userManager;
        public EmployeeController(IUnitOfWork unitOfWork, ILogger<EmployeeController> logger,
                                  UserManager<ApplicationUser> userManager, IEmailSender emailSender, ISmsSender smsSender, 
                                  IOptions<ApplicationSettings> AppSettings, IConfiguration configuration)
        {
            _emailSender = emailSender;
            _smsSender = smsSender;
            _logger = logger;
            _appSettings = AppSettings.Value;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
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

                //Determine if username is email or phone
                SetUserName(employeeViewModel);
                var user = GetUserProperty(employeeViewModel);
                string password = UserHelpers.GeneratePassword();

                var result = await _userManager.CreateAsync(user, password).ConfigureAwait(true);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Employee").ConfigureAwait(true);
                    _logger.LogInformation("User created a new account with password.");

                    var modal = SetEmployeeProperty(employeeViewModel, user);
                    _unitOfWork.EmployeeRepository.Add(modal);

                    if (_unitOfWork.Commit())
                        await UserVerification(user).ConfigureAwait(true);

                    return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Employee registered succefully." }).ConfigureAwait(false);
                }
                else
                {
                    AddErrors(result);
                    string _Code = "", _Description = "";
                    if (result.Errors != null)
                    {
                        foreach (var error in result.Errors)
                        {
                            _Code = error.Code;
                            _Description = error.Description;
                        }
                    }

                    return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = _Code, Desc = _Description });
                }

                #endregion User
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

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FetchGridDataEmployeeByOrgID")]
        public async Task<object> FetchGridDataEmployeeByOrgID([FromBody] UtilsOrgID UtilsOrgID, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (UtilsOrgID == null)
                    throw new ArgumentNullException(nameof(UtilsOrgID.OrgID));

                oDataTable _oDataTable = new oDataTable();
                var results = _unitOfWork.EmployeeRepository.FetchGridDataEmployeeByOrgID(UtilsOrgID.OrgID);
                var xResult = _oDataTable.ToDataTable(results);
                _unitOfWork.Commit();

                return await System.Threading.Tasks.Task.FromResult<object>(JsonConvert.SerializeObject(xResult, Formatting.Indented)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return System.Threading.Tasks.Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindEmployeeListByDesignationID")]
        public async Task<object> FindEmployeeListByDesignationID([FromBody] Utils utils, CancellationToken cancellationToken)
        {

            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (utils == null)
                    throw new ArgumentNullException(nameof(utils.ID));

                oDataTable _oDataTable = new oDataTable();
                var results = _unitOfWork.EmployeeRepository.FindEmployeeListByDesignationID(utils.ID);
                var xResult = _oDataTable.ToDataTable(results);

                return await System.Threading.Tasks.Task.FromResult<object>(JsonConvert.SerializeObject(results, Formatting.Indented)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindEmployeeListByDepartmentID")]
        public async Task<object> FindEmployeeListByDepartmentID([FromBody] Utils utils, CancellationToken cancellationToken)
        {

            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (utils == null)
                    throw new ArgumentNullException(nameof(utils.ID));

                oDataTable _oDataTable = new oDataTable();
                var results = _unitOfWork.EmployeeRepository.FindEmployeeListByDepartmentID(utils.ID);
                var xResult = _oDataTable.ToDataTable(results);

                return await System.Threading.Tasks.Task.FromResult<object>(JsonConvert.SerializeObject(results, Formatting.Indented)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindEmpDepartDesignByEmpID")]
        public async Task<object> FindEmpDepartDesignByEmpID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                oDataTable _oDataTable = new oDataTable();
                var results = _unitOfWork.EmployeeRepository.FindEmpDepartDesignByEmpID(Utils.ID);

                return await System.Threading.Tasks.Task.FromResult<object>(JsonConvert.SerializeObject(results, Formatting.Indented)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return System.Threading.Tasks.Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("GetAllOutsourcedEmpByOrgID")]
        public async Task<object> GetAllOutsourcedEmpByOrgID([FromBody] UtilsOrgID UtilsOrgID, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (UtilsOrgID == null)
                    throw new ArgumentNullException(nameof(UtilsOrgID.OrgID));

                oDataTable _oDataTable = new oDataTable();
                var results = _unitOfWork.EmployeeRepository.GetAllOutsourcedEmpByOrgID(UtilsOrgID.OrgID);
                var xResult = _oDataTable.ToDataTable(results);
                _unitOfWork.Commit();

                return await System.Threading.Tasks.Task.FromResult<object>(JsonConvert.SerializeObject(xResult, Formatting.Indented)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("GetAllFreelancerEmpByOrgID")]
        public async Task<object> GetAllFreelancerEmpByOrgID([FromBody] UtilsOrgID UtilsOrgID, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (UtilsOrgID == null)
                    throw new ArgumentNullException(nameof(UtilsOrgID.OrgID));

                oDataTable _oDataTable = new oDataTable();
                var results = _unitOfWork.EmployeeRepository.GetAllFreelancerEmpByOrgID(UtilsOrgID.OrgID);
                var xResult = _oDataTable.ToDataTable(results);
                _unitOfWork.Commit();

                return await System.Threading.Tasks.Task.FromResult<object>(JsonConvert.SerializeObject(xResult, Formatting.Indented)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }


        [HttpPost]
        [Route("FindEmpDepartDesignByTeamID")]
        public async Task<object> FindEmpDepartDesignByTeamID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                oDataTable _oDataTable = new oDataTable();
                var results = _unitOfWork.EmployeeRepository.FindEmpDepartDesignByTeamID(Utils.ID);
                var xResult = _oDataTable.ToDataTable(results);

                return await System.Threading.Tasks.Task.FromResult<object>(JsonConvert.SerializeObject(results, Formatting.Indented)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return System.Threading.Tasks.Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }


        #region Helpers
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }

        private ApplicationUser GetUserProperty(EmployeeViewModel employeeViewModel)
        {
            return new ApplicationUser()
            {
                UserName = _userName,
                Email = employeeViewModel.workemail,
                FullName = employeeViewModel.first_name + " " + employeeViewModel.last_name,
                FirstName = employeeViewModel.first_name,
                LastName = employeeViewModel.last_name,
                Role = "Employee",
                PhoneNumber = employeeViewModel.phone,
                isSuperAdmin = false
            };
        }

        private static string SetUserName(EmployeeViewModel employeeViewModel)
        {
            if (employeeViewModel.workemail == null || string.IsNullOrEmpty(employeeViewModel.workemail)
                    && string.IsNullOrWhiteSpace(employeeViewModel.workemail) && employeeViewModel.workemail == "")
            {
                if (!string.IsNullOrEmpty(employeeViewModel.phone) || !string.IsNullOrWhiteSpace(employeeViewModel.phone) || employeeViewModel.phone != "")
                {
                    _userName = UserHelpers.IsPhoneValid(employeeViewModel.phone);
                }
            }
            else
            {
                _userName = employeeViewModel.workemail;
            }

            return _userName;
        }

        private static Employee SetEmployeeProperty(EmployeeViewModel employeeViewModel, ApplicationUser user)
        {

            var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<EmployeeViewModel, Employee>());
            var mapper = config.CreateMapper();
            var modal = mapper.Map<Employee>(employeeViewModel);

            modal.id = Guid.NewGuid().ToString();
            modal.created_date = DateTime.Now.ToString(CultureInfo.CurrentCulture);
            modal.is_deleted = false;
            modal.user_id = user.Id;
            modal.is_admin = false;
            modal.is_superadmin = false;
            modal.mobile = UserHelpers.IsPhoneValid(employeeViewModel.mobile);

            return modal;
        }

        private async Task UserVerification(ApplicationUser user)
        {
            if ((user.Email != null) && (!string.IsNullOrEmpty(user.Email) || !string.IsNullOrWhiteSpace(user.Email) || user.Email != ""))
            {
                var ResetCode = await _userManager.GenerateEmailConfirmationTokenAsync(user).ConfigureAwait(true);
                var xResetCode = Base64UrlEncoder.Encode(ResetCode);
                var callbackUrl = Url.EmailConfirmationLink(user.Id, xResetCode, Request.Scheme);
                await _emailSender.SendEmailConfirmationAsync(user.Email, callbackUrl).ConfigureAwait(true);
            }
            else if ((user.PhoneNumber != null) && (!string.IsNullOrEmpty(user.PhoneNumber) || !string.IsNullOrWhiteSpace(user.PhoneNumber) || (user.PhoneNumber) != ""))
            {
                var ResetCode = await _userManager.GenerateEmailConfirmationTokenAsync(user).ConfigureAwait(true);
                var xResetCode = Base64UrlEncoder.Encode(ResetCode);
                var callbackUrl = Url.EmailConfirmationLink(user.Id, xResetCode, Request.Scheme);
                await _smsSender.SendSmsConfirmationAsync(user.PhoneNumber, callbackUrl).ConfigureAwait(true);
            }
        }

        #endregion
    }
}
