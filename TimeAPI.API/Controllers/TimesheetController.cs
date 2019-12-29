using TimeAPI.API.Models.AccountViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TimeAPI.API.Models;
using TimeAPI.API.Services;
using Microsoft.AspNetCore.Cors;
using TimeAPI.API.Filters;
using TimeAPI.Domain;
using System.Threading;
using TimeAPI.Domain.Entities;
using TimeAPI.API.Models.DesignationViewModels;
using TimeAPI.API.Models.TimesheetViewModels;
using System.Collections.Generic;
using System.Globalization;

namespace TimeAPI.API.Controllers
{
    //[ApiKeyAuth]
    //[EnableCors("CorsPolicy")]
    [Route("[controller]")]
    //[Authorize(Roles = "superadmin")]
    public class TimesheetController : Controller
    {
        private const string FormatTime = @"hh\:mm";
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly ApplicationSettings _appSettings;
        private readonly IUnitOfWork _unitOfWork;
        public TimesheetController(IUnitOfWork unitOfWork, ILogger<TimesheetController> logger,
            IEmailSender emailSender,
            IOptions<ApplicationSettings> AppSettings)
        {
            _emailSender = emailSender;
            _logger = logger;
            _appSettings = AppSettings.Value;
            _unitOfWork = unitOfWork;
        }

        [HttpPost]
        [Route("AddTimesheet")]
        public async Task<object> AddTimesheet([FromBody] TimesheetViewModel timesheetViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (timesheetViewModel == null)
                    throw new ArgumentNullException(nameof(timesheetViewModel));

                if (timesheetViewModel.groupid == "string" || string.IsNullOrWhiteSpace(timesheetViewModel.groupid)
                    || string.IsNullOrEmpty(timesheetViewModel.groupid))
                    timesheetViewModel.groupid = null;

                timesheetViewModel.is_deleted = false;
                timesheetViewModel.is_checkout = false;
                timesheetViewModel.check_out = null;
                timesheetViewModel.total_hrs = null;
                timesheetViewModel.ondate = DateTime.Now.ToString(CultureInfo.CurrentCulture);
                timesheetViewModel.created_date = DateTime.Now.ToString(CultureInfo.CurrentCulture);

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<TimesheetViewModel, Timesheet>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Timesheet>(timesheetViewModel);

                var _groupid = Guid.NewGuid().ToString();
                if (timesheetViewModel.groupid != null)
                    _groupid = timesheetViewModel.groupid;

                
                #region TimesheetWithTeamMembers

                foreach (var item in timesheetViewModel.team_member_empid.Distinct())
                {
                    modal.id = Guid.NewGuid().ToString();
                    modal.empid = item;
                    modal.created_date = DateTime.Now.ToString(CultureInfo.CurrentCulture);
                    modal.is_deleted = false;
                    modal.groupid = _groupid;

                    _unitOfWork.TimesheetRepository.Add(modal);
                }

                #endregion TimesheetWithTeamMembers

                #region Teams

                foreach (var item in timesheetViewModel.teamid.Distinct())
                {
                    var timesheet_team = new TimesheetTeam
                    {
                        id = Guid.NewGuid().ToString(),
                        teamid = item,
                        groupid = modal.groupid,
                        is_deleted = false,
                        created_date = DateTime.Now.ToString(CultureInfo.CurrentCulture),
                        createdby = modal.createdby
                    };
                    _unitOfWork.TimesheetTeamRepository.Add(timesheet_team);
                }

                #endregion Teams

                #region TimesheetProjectCategory

                if (timesheetViewModel.project_category_type_id != "")
                {
                    var project_category_type = new TimesheetProjectCategory
                    {
                        id = Guid.NewGuid().ToString(),
                        timesheet_id = modal.id,
                        groupid = modal.groupid,
                        project_category_type_id = timesheetViewModel.project_category_type_id,
                        system_id = timesheetViewModel.system_id,
                        is_deleted = false,
                        created_date = DateTime.Now.ToString(CultureInfo.CurrentCulture),
                        createdby = modal.createdby
                    };
                    _unitOfWork.TimesheetProjectCategoryRepository.Add(project_category_type);
                }

                #endregion TimesheetProjectCategory

                #region TimesheetAdministrative

                foreach (var item in timesheetViewModel.timesheet_administrative.Distinct())
                {
                    var project_administrative = new TimesheetAdministrative
                    {
                        id = Guid.NewGuid().ToString(),
                        administrative_id = item,
                        groupid = modal.groupid,
                        is_deleted = false,
                        created_date = DateTime.Now.ToString(CultureInfo.CurrentCulture),
                        createdby = modal.createdby
                    };
                    _unitOfWork.TimesheetAdministrativeRepository.Add(project_administrative);
                }

                #endregion TimesheetAdministrative

                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Timesheet registered succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdateTimesheet")]
        public async Task<object> UpdateAddTimesheet([FromBody] TimesheetViewModel timesheetViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (timesheetViewModel == null)
                    throw new ArgumentNullException(nameof(timesheetViewModel));

                timesheetViewModel.modified_date = DateTime.Now.ToString(CultureInfo.CurrentCulture);
                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<TimesheetViewModel, Timesheet>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Timesheet>(timesheetViewModel);
                modal.modified_date = DateTime.Now.ToString(CultureInfo.CurrentCulture);


                #region TimesheetWithTeamMembers

                //Remove TeamMembers with this CurrentGroupID
                _unitOfWork.TimesheetRepository.RemoveByGroupID(modal.groupid);

                foreach (var item in timesheetViewModel.team_member_empid.Distinct())
                {
                    modal.id = Guid.NewGuid().ToString();
                    modal.empid = item;
                    modal.created_date = DateTime.Now.ToString(CultureInfo.CurrentCulture);
                    modal.is_deleted = false;
                    modal.groupid = timesheetViewModel.groupid;

                    _unitOfWork.TimesheetRepository.Add(modal);
                }

                #endregion TimesheetWithTeamMembers

                #region Teams

                //Remove TeamMembers with this CurrentGroupID
                _unitOfWork.TimesheetTeamRepository.RemoveByGroupID(modal.groupid);

                foreach (var item in timesheetViewModel.teamid.Distinct())
                {
                    var timesheet_team = new TimesheetTeam
                    {
                        id = Guid.NewGuid().ToString(),
                        teamid = item,
                        groupid = modal.groupid,
                        is_deleted = false,
                        created_date = DateTime.Now.ToString(CultureInfo.CurrentCulture),
                        createdby = modal.createdby
                    };
                    _unitOfWork.TimesheetTeamRepository.Add(timesheet_team);
                }

                #endregion Teams

                #region TimesheetActivity

                //Remove ProjectCategory with this CurrentGroupID
                _unitOfWork.TimesheetProjectCategoryRepository.RemoveByGroupID(modal.groupid);
                if (timesheetViewModel.project_category_type_id != "")
                {
                    var timesheet_activity = new TimesheetProjectCategory
                    {
                        id = Guid.NewGuid().ToString(),
                        timesheet_id = modal.id,
                        groupid = modal.groupid,
                        project_category_type_id = timesheetViewModel.project_category_type_id,
                        system_id = timesheetViewModel.system_id,
                        is_deleted = false,
                        created_date = DateTime.Now.ToString(CultureInfo.CurrentCulture),
                        createdby = modal.createdby
                    };

                    _unitOfWork.TimesheetProjectCategoryRepository.Add(timesheet_activity);
                }

                #endregion TimesheetActivity

                #region TimesheetAdministrative

                _unitOfWork.TimesheetAdministrativeRepository.RemoveByGroupID(modal.groupid);
                foreach (var item in timesheetViewModel.timesheet_administrative.Distinct())
                {
                    var project_administrative = new TimesheetAdministrative
                    {
                        id = Guid.NewGuid().ToString(),
                        administrative_id = item,
                        groupid = modal.groupid,
                        is_deleted = false,
                        created_date = DateTime.Now.ToString(CultureInfo.CurrentCulture),
                        createdby = modal.createdby
                    };
                    _unitOfWork.TimesheetAdministrativeRepository.Add(project_administrative);
                }

                #endregion TimesheetAdministrative

                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Timesheet updated succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemoveTimesheet")]
        public async Task<object> RemoveTimesheet([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(paramName: nameof(Utils.ID));

                _unitOfWork.TimesheetRepository.Remove(Utils.ID);
                var Timesheet = _unitOfWork.TimesheetRepository.Find(Utils.ID);

                _unitOfWork.TimesheetRepository.RemoveByGroupID(Timesheet.groupid);
                _unitOfWork.TimesheetTeamRepository.RemoveByGroupID(Timesheet.groupid);
                _unitOfWork.TimesheetProjectCategoryRepository.RemoveByGroupID(Timesheet.groupid);
                _unitOfWork.TimesheetAdministrativeRepository.RemoveByGroupID(Timesheet.groupid);

                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Timesheet removed succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetAllTimesheets")]
        public async Task<object> GetAllTimesheets(CancellationToken cancellationToken)
        {

            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.TimesheetRepository.All();
                _unitOfWork.Commit();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("CheckOutByEmpID")]
        public async Task<object> CheckOutByEmpID([FromBody] TimesheetViewModel timesheetViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (timesheetViewModel == null)
                    throw new ArgumentNullException(nameof(timesheetViewModel));

                timesheetViewModel.modified_date = DateTime.Now.ToString(CultureInfo.CurrentCulture);

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<TimesheetViewModel, Timesheet>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Timesheet>(timesheetViewModel);

                foreach (var item in timesheetViewModel.team_member_empid.Distinct())
                {
                    modal.empid = item;

                    var Timesheet = _unitOfWork.TimesheetRepository.FindTimeSheetByEmpID(modal.empid, modal.groupid);

                    var _TotalMinutes = (Convert.ToDateTime(modal.check_out) - Convert.ToDateTime(Timesheet.check_in)).TotalMinutes;
                    TimeSpan spWorkMin = TimeSpan.FromMinutes(_TotalMinutes);

                    modal.total_hrs = spWorkMin.ToString(FormatTime);
                    modal.modified_date = DateTime.Now.ToString(CultureInfo.CurrentCulture);
                    modal.is_checkout = true;
                    modal.is_deleted = false;
                    modal.groupid = timesheetViewModel.groupid;

                    _unitOfWork.TimesheetRepository.CheckOutByEmpID(modal);
                }

                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Checkout Successfully", Desc = "Checkout Successfully" }).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

    }
}
