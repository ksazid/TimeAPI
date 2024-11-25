﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TimeAPI.API.Extensions;
using TimeAPI.API.Models;
using TimeAPI.API.Models.EmployeeAppUsageViewModels;
using TimeAPI.API.Models.EmployeeLeaveViewModels;
using TimeAPI.API.Models.EmployeeViewModels;
using TimeAPI.API.Services;
using TimeAPI.Domain;
using TimeAPI.Domain.Entities;

namespace TimeAPI.API.Controllers
{
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
        private static DateTime _dateTime;

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
            _dateTime = InternetTime.GetCurrentTimeFromTimeZone().Value.DateTime;
        }

        #region Employee

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

                    return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Employee registered successfully." }).ConfigureAwait(false);
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
                modal.modified_date = _dateTime.ToString();

                _unitOfWork.EmployeeRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Employee updated successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("SetEmployeeInactiveByEmpID")]
        public async Task<object> SetEmployeeInactiveByEmpID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                await _unitOfWork.EmployeeRepository.SetEmployeeInactiveByEmpID(Utils.ID).ConfigureAwait(false);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Employee set to inactive." }).ConfigureAwait(false);
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

                int Result = await _unitOfWork.EmployeeRepository.RemoveEmployeeIfZeroActivity(Utils.ID).ConfigureAwait(false);

                if (Result > 0)
                {
                    return await Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = "Error", Desc = "Employee has being attended more than one project activity. Are you sure to remove all past history of the employee." }).ConfigureAwait(false);
                }

                var result = await _unitOfWork.EmployeeRepository.Find(Utils.ID).ConfigureAwait(false);

                await _unitOfWork.EmployeeRepository.RemovePermanent(Utils.ID).ConfigureAwait(false);
                _unitOfWork.UserRepository.Remove(result.user_id);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Employee removed successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemoveEmployeePermanent")]
        public async Task<object> RemoveEmployeePermanent([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = await _unitOfWork.EmployeeRepository.Find(Utils.ID).ConfigureAwait(false);
                await _unitOfWork.EmployeeRepository.RemovePermanent(Utils.ID).ConfigureAwait(false);
                _unitOfWork.UserRepository.Remove(result.user_id);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Employee removed successfully." }).ConfigureAwait(false);
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

                var result = await _unitOfWork.EmployeeRepository.All().ConfigureAwait(false);
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

                var result = await _unitOfWork.EmployeeRepository.Find(Utils.ID).ConfigureAwait(false);

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

                var result = await _unitOfWork.EmployeeRepository.FindByEmpName(UtilsName.FullName).ConfigureAwait(false);

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

                var result = await _unitOfWork.EmployeeRepository.FindByOrgIDCode(Utils.ID).ConfigureAwait(false);
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

                var result = await _unitOfWork.EmployeeRepository.FindByEmpCode(_EmpCode.Code).ConfigureAwait(false);
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

                var result = await _unitOfWork.EmployeeRepository.FindByRoleName(UtilsRole.Role).ConfigureAwait(false);
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
                var results = await _unitOfWork.EmployeeRepository.FetchGridDataEmployeeByOrgID(UtilsOrgID.OrgID).ConfigureAwait(false);
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
                var results = await _unitOfWork.EmployeeRepository.FindEmployeeListByDesignationID(utils.ID).ConfigureAwait(false);
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
                var results = await _unitOfWork.EmployeeRepository.FindEmployeeListByDepartmentID(utils.ID).ConfigureAwait(false);
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
                var results = await _unitOfWork.EmployeeRepository.FindEmpDepartDesignByEmpID(Utils.ID).ConfigureAwait(false);
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
                var results = await _unitOfWork.EmployeeRepository.GetAllOutsourcedEmpByOrgID(UtilsOrgID.OrgID).ConfigureAwait(false);
                var xResult = _oDataTable.ToDataTable(results);

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
                var results = await _unitOfWork.EmployeeRepository.GetAllFreelancerEmpByOrgID(UtilsOrgID.OrgID).ConfigureAwait(false);
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
                var results = await _unitOfWork.EmployeeRepository.FindEmpDepartDesignByTeamID(Utils.ID).ConfigureAwait(false);
                var xResult = _oDataTable.ToDataTable(results);

                return await System.Threading.Tasks.Task.FromResult<object>(JsonConvert.SerializeObject(results, Formatting.Indented)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return System.Threading.Tasks.Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("GetOrgScreenshotDetailByUserID")]
        public async Task<object> GetOrgScreenshotDetailByUserID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = await _unitOfWork.EmployeeRepository.GetOrganizationScreenshotDetails(Utils.ID).ConfigureAwait(false);
                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("AddEmployeeAppUsage")]
        public async Task<object> AddEmployeeAppUsage([FromBody] EmployeeAppUsageViewModel employeeViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (employeeViewModel == null)
                    throw new ArgumentNullException(nameof(employeeViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<EmployeeAppUsageViewModel, EmployeeAppUsage>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<EmployeeAppUsage>(employeeViewModel);

                modal.id = Guid.NewGuid().ToString();
                modal.created_date = _dateTime.ToString();
                modal.is_deleted = false;
                modal.ondate = _dateTime.ToString();

                for (int i = 0; i < employeeViewModel.AppUsedViewModel.Count; i++)
                {
                    string Url = string.Empty;
                    if (employeeViewModel.AppUsedViewModel[i].app_name != "unknown"
                        && employeeViewModel.AppUsedViewModel[i].app_name != " "
                        && !String.IsNullOrEmpty(employeeViewModel.AppUsedViewModel[i].app_name.Trim())
                        && !String.IsNullOrWhiteSpace(employeeViewModel.AppUsedViewModel[i].app_name))
                    {
                        Url = VerifyURL(employeeViewModel.AppUsedViewModel[i].app_name);
                        var AppTrackedViewModel = new EmployeeAppTracked
                        {
                            id = Guid.NewGuid().ToString(),
                            emp_app_usage_id = modal.id,
                            emp_id = modal.emp_id,
                            app_name = Url,
                            app_category_name = employeeViewModel.AppUsedViewModel[i].app_category_name,
                            time_spend = employeeViewModel.AppUsedViewModel[i].time_spend,
                            icon = employeeViewModel.AppUsedViewModel[i].icon,
                            is_productive = false,
                            is_unproductive = false,
                            is_neutral = false,
                            created_date = _dateTime.ToString(),
                            is_deleted = false
                        };
                        _unitOfWork.EmployeeAppTrackedRepository.Add(AppTrackedViewModel);
                    }
                }

                _unitOfWork.EmployeeAppUsageRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Record Added Successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel
                {
                    Status = "201",
                    Code = ex.Message,
                    Desc = ex.Message
                });
            }

            static string VerifyURL(string _app_name)
            {
                string Url;
                if (_app_name.Contains(".com/"))
                {
                    Url = _app_name.Split(".com/")[0].ToString() + ".com";
                }
                else if (_app_name.Contains(".io/"))
                {
                    Url = _app_name.Split(".io/")[0].ToString() + ".io";
                }
                else if (_app_name.Contains(".info/"))
                {
                    Url = _app_name.Split(".info/")[0].ToString() + ".info";
                }
                else if (_app_name.Contains(".net/"))
                {
                    Url = _app_name.Split(".net/")[0].ToString() + ".net";
                }
                else if (_app_name.Contains(".gov/"))
                {
                    Url = _app_name.Split(".gov/")[0].ToString() + ".gov";
                }
                else if (_app_name.Contains(".edu/"))
                {
                    Url = _app_name.Split(".edu/")[0].ToString() + ".edu";
                }
                else if (_app_name.Contains(".ai/"))
                {
                    Url = _app_name.Split(".ai/")[0].ToString() + ".ai";
                }
                else if (_app_name.Contains(".ae/"))
                {
                    Url = _app_name.Split(".ae/")[0].ToString() + ".ae";
                }
                else if (_app_name.Contains(".org/"))
                {
                    Url = _app_name.Split(".org/")[0].ToString() + ".org";
                }
                else
                    Url = _app_name;
                return Url;
            }
        }

        #endregion Employee

        #region EmployeeLeave

        [HttpPost]
        [Route("AddEmployeeLeave")]
        public async Task<object> AddEmployeeLeave([FromBody] EmployeeLeaveViewModel employeeViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (employeeViewModel == null)
                    throw new ArgumentNullException(nameof(employeeViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<EmployeeLeaveViewModel, EmployeeLeave>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<EmployeeLeave>(employeeViewModel);
                modal.id = Guid.NewGuid().ToString();
                modal.created_date = _dateTime.ToString();

                var LeaveName = _unitOfWork.LeaveSetupRepository.Find(modal.leave_setup_id).leave_name;
                var FromName = (await _unitOfWork.EmployeeRepository.Find(modal.emp_id).ConfigureAwait(false)).full_name;
                var ToName = (await _unitOfWork.EmployeeRepository.Find(modal.approver_emp_id).ConfigureAwait(false)).full_name;

                var EmployeeLeaveLog = new EmployeeLeaveLog
                {
                    id = Guid.NewGuid().ToString(),
                    leave_type = LeaveName,
                    emp_id = modal.emp_id,
                    emp_leave_id = modal.id,
                    no_of_days = modal.leave_days,
                    start_date = modal.leave_start_date,
                    end_date = modal.leave_end_date,
                    ondate = _dateTime.ToString(),
                    from_user = FromName,
                    to_user = ToName,
                    createdby = modal.createdby,
                    created_date = _dateTime.ToString()
                };

                _unitOfWork.EmployeeLeaveLogRepository.Add(EmployeeLeaveLog);
                _unitOfWork.EmployeeLeaveRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Employee Leave added successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdateEmployeeLeave")]
        public async Task<object> UpdateEmployeeLeave([FromBody] EmployeeLeaveViewModel employeeViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (employeeViewModel == null)
                    throw new ArgumentNullException(nameof(employeeViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<EmployeeLeaveViewModel, EmployeeLeave>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<EmployeeLeave>(employeeViewModel);
                modal.modified_date = _dateTime.ToString();

                _unitOfWork.EmployeeLeaveRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Employee Leave updated successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemoveEmployeeLeave")]
        public async Task<object> RemoveEmployeeLeave([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                _unitOfWork.EmployeeLeaveRepository.Remove(Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "EmployeeLeave removed successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindEmployeeLeaveByID")]
        public async Task<object> FindEmployeeLeaveByID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = await _unitOfWork.EmployeeLeaveRepository.FetchEmployeeLeaveID(Utils.ID).ConfigureAwait(false);
                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FetchEmployeeLeaveOrgID")]
        public async Task<object> FetchEmployeeLeaveOrgID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                oDataTable _oDataTable = new oDataTable();
                var result = await _unitOfWork.EmployeeLeaveRepository.FetchEmployeeLeaveOrgID(Utils.ID).ConfigureAwait(false);
                //var xResult = _oDataTable.ToDataTable(result);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FetchEmployeeLeaveEmpID")]
        public async Task<object> FetchEmployeeLeaveEmpID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = await _unitOfWork.EmployeeLeaveRepository.FetchEmployeeLeaveEmpID(Utils.ID).ConfigureAwait(false);
                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdateApprovedByID")]
        public async Task<object> UpdateApprovedByID([FromBody] EmployeeLeaveViewModel employeeViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (employeeViewModel == null)
                    throw new ArgumentNullException(nameof(employeeViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<EmployeeLeaveViewModel, EmployeeLeave>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<EmployeeLeave>(employeeViewModel);
                modal.modified_date = _dateTime.ToString();

                var LeaveName = _unitOfWork.LeaveSetupRepository.Find(modal.leave_setup_id).leave_name;
                var ToName = (await _unitOfWork.EmployeeRepository.Find(modal.emp_id).ConfigureAwait(false)).full_name;
                var FromName = (await _unitOfWork.EmployeeRepository.Find(modal.approver_emp_id).ConfigureAwait(false)).full_name;

                var EmployeeLeaveLog = new EmployeeLeaveLog
                {
                    id = Guid.NewGuid().ToString(),
                    leave_type = LeaveName,
                    emp_id = modal.emp_id,
                    emp_leave_id = modal.id,
                    no_of_days = modal.approved_days,
                    start_date = modal.approve_start_date,
                    end_date = modal.approve_end_date,
                    ondate = _dateTime.ToString(),
                    from_user = FromName,
                    to_user = ToName,
                    createdby = modal.createdby,
                    created_date = _dateTime.ToString()
                };

                _unitOfWork.EmployeeLeaveLogRepository.Add(EmployeeLeaveLog);
                await _unitOfWork.EmployeeLeaveRepository.UpdateApprovedByID(modal).ConfigureAwait(false);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Employee Leave updated successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FetchEmployeeLeaveHistoryEmpID")]
        public async Task<object> FetchEmployeeLeaveHistoryEmpID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = await _unitOfWork.EmployeeLeaveRepository.FetchEmployeeLeaveHistoryEmpID(Utils.ID).ConfigureAwait(false);
                var json = JsonConvert.SerializeObject(result);
                DataTable dataTable = (DataTable)JsonConvert.DeserializeObject(json, (typeof(DataTable)));

                List<UtilLeaveGridData> LeaveGridData = new List<UtilLeaveGridData>();

                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    var LeaveDays = await _unitOfWork.EmployeeLeaveRepository.GetDaysOfMonth
                                                                        (dataTable.Rows[i]["leave_start_date"].ToString(),
                                                                        dataTable.Rows[i]["leave_end_date"].ToString()).ConfigureAwait(false);

                    var json2 = JsonConvert.SerializeObject(LeaveDays);
                    DataTable dataTable2 = (DataTable)JsonConvert.DeserializeObject(json2, (typeof(DataTable)));

                    foreach (DataRow item in dataTable2.Rows)
                    {
                        LeaveGridData.Add(new UtilLeaveGridData
                        {
                            emp_id = dataTable.Rows[i]["emp_id"].ToString(),
                            leave_status_name = dataTable.Rows[i]["leave_status_name"].ToString(),
                            leave_type_name = dataTable.Rows[i]["leave_type_name"].ToString(),
                            leave_name = dataTable.Rows[i]["leave_name"].ToString(),
                            leave_start_date = dataTable.Rows[i]["leave_start_date"].ToString(),
                            leave_end_date = dataTable.Rows[i]["leave_end_date"].ToString(),
                            month = item["month"].ToString(),
                            days = item["days"].ToString(),
                        });
                    }
                }

                var results = from line in LeaveGridData
                              let k = new
                              {
                                  leave_status_name = line.leave_status_name,
                                  month = line.month
                              }
                              group line by k into t
                              select new UtilLeaveGridData
                              {
                                  emp_id = t.First().emp_id,
                                  leave_status_name = t.First().leave_status_name,
                                  leave_type_name = t.First().leave_type_name,
                                  leave_name = t.First().leave_name,
                                  leave_start_date = t.First().leave_start_date,
                                  leave_end_date = t.First().leave_end_date,
                                  month = t.First().month,
                                  days = t.Sum(pc => Convert.ToInt32(pc.days)).ToString()
                              };

                return await Task.FromResult<object>(results).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel
                {
                    Status = "201",
                    Code = ex.Message,
                    Desc = ex.Message
                });
            }
        }

        [HttpPost]
        [Route("FetchEmployeeLeaveHistoryOrgID")]
        public async Task<object> FetchEmployeeLeaveHistoryOrgID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = await _unitOfWork.EmployeeLeaveRepository.FetchEmployeeLeaveHistoryOrgID(Utils.ID).ConfigureAwait(false);
                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FetchEmployeeLeaveHistoryApproverID")]
        public async Task<object> FetchEmployeeLeaveHistoryApproverID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = await _unitOfWork.EmployeeLeaveRepository.FetchEmployeeLeaveHistoryApproverID(Utils.ID).ConfigureAwait(false);
                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FetchEmployeeLeaveLogEmpID")]
        public async Task<object> FetchEmployeeLeaveLogEmpID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = await _unitOfWork.EmployeeLeaveLogRepository.FetchEmployeeLeaveLogHistoryEmpID(Utils.ID).ConfigureAwait(false);
                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        #endregion EmployeeLeave

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
            modal.created_date = _dateTime.ToString();
            modal.is_deleted = false;
            modal.user_id = user.Id;
            modal.is_admin = false;
            modal.is_superadmin = false;
            modal.is_password_reset = false;
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

        #endregion Helpers
    }
}