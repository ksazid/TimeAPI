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
            IEmailSender emailSender, IOptions<ApplicationSettings> AppSettings, IConfiguration configuration)
        {
            _emailSender = emailSender;
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
                Role role = null;
                string _userName = "";
                if (!string.IsNullOrEmpty(employeeViewModel.workemail)
                    || !string.IsNullOrWhiteSpace(employeeViewModel.workemail) || employeeViewModel.workemail != "")
                {
                    _userName = employeeViewModel.workemail;
                }
                else if (!string.IsNullOrEmpty(employeeViewModel.phone)
                    || !string.IsNullOrWhiteSpace(employeeViewModel.phone) || employeeViewModel.phone != "")
                {
                    if (employeeViewModel.phone.Contains("+"))
                        _userName = employeeViewModel.phone.Substring(1);
                    else
                        _userName = employeeViewModel.phone;
                }

                if (employeeViewModel.role_id != null)
                {
                    role = _unitOfWork.RoleRepository.Find(employeeViewModel.role_id);
                    if (role.NormalizedName == "ADMIN")
                        employeeViewModel.is_admin = true;
                    else
                    {
                        employeeViewModel.is_admin = false;
                        employeeViewModel.is_superadmin = false;
                    }
                }

                var user = new ApplicationUser()
                {
                    UserName = _userName,
                    Email = employeeViewModel.workemail,
                    FullName = employeeViewModel.first_name + " " + employeeViewModel.last_name,
                    FirstName = employeeViewModel.first_name,
                    LastName = employeeViewModel.last_name,
                    Role = role.Name,
                    PhoneNumber = employeeViewModel.phone,
                    isSuperAdmin = false
                };

                oDataTable _oDataTable = new oDataTable();
                string password = "P@ssw0rd123";// _oDataTable.GeneratePassword() + "@";

                var result = await _userManager.CreateAsync(user, password).ConfigureAwait(true);
                if (result.Succeeded)
                {
                    var xRest = await _userManager.AddToRoleAsync(user, role.Name).ConfigureAwait(true);
                    _logger.LogInformation("User created a new account with password.");

                    if (user.Email != "")
                    {
                        var code1 = await _userManager.GenerateEmailConfirmationTokenAsync(user).ConfigureAwait(true);
                        //code1 = System.Web.HttpUtility.UrlEncode(code1);
                        var callbackUrl1 = Url.EmailConfirmationLink(user.Id, code1, Request.Scheme);
                        await _emailSender.SendEmailConfirmationAsync(user.Email, callbackUrl1).ConfigureAwait(true);

                        var code = await _userManager.GeneratePasswordResetTokenAsync(user).ConfigureAwait(true);
                        //code = System.Web.HttpUtility.UrlEncode(code);
                        var callbackUrl = Url.PasswordLink(user.Id, code, Request.Scheme);
                        await _emailSender.SendSetupPasswordAsync(user.Email, callbackUrl).ConfigureAwait(true);
                    }
                    else
                    {
                        // check if its a phone 
                    }

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
                _unitOfWork.Commit();

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
                _unitOfWork.Commit();

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
                _unitOfWork.Commit();

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
        #endregion
    }
}
