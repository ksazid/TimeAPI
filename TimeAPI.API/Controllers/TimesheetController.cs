using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TimeAPI.API.Models;
using TimeAPI.API.Models.TimesheetActivityCommentViewModels;
using TimeAPI.API.Models.TimesheetActivityFileViewModels;
using TimeAPI.API.Models.TimesheetActivityViewModels;
using TimeAPI.API.Models.TimesheetViewModels;
using TimeAPI.API.Models.TimesheetBreakPostViewModels;
using TimeAPI.API.Services;
using TimeAPI.Domain;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Repositories;

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
        private readonly DateTime _dateTime;
        //private IHubContext<NotifyHub, ITypedHubClient> _hubContext;

        public TimesheetController(IUnitOfWork unitOfWork, ILogger<TimesheetController> logger,
            IEmailSender emailSender, IOptions<ApplicationSettings> AppSettings
            //,IHubContext<NotifyHub, ITypedHubClient> hubContext
            )
        {
            _emailSender = emailSender;
            _logger = logger;
            _appSettings = AppSettings.Value;
            _unitOfWork = unitOfWork;
            _dateTime = InternetTime.GetCurrentTimeFromTimeZone().Value.DateTime;
            //_hubContext = hubContext;
        }

        #region Timesheet

        [HttpPost]
        [Route("AddTimesheet")]
        public async Task<object> AddTimesheet([FromBody] TimesheetPostViewModel timesheetViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (timesheetViewModel == null)
                    throw new ArgumentNullException(nameof(timesheetViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<TimesheetPostViewModel, Timesheet>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Timesheet>(timesheetViewModel);

                var _groupid = Guid.NewGuid().ToString();

                modal.groupid = _groupid;

                //Adding Multiple Timesheet
                AddTimesheetWithTeamMembers(timesheetViewModel, modal);

                //Adding Timesheet Teams
                AddTeamsTimesheet(timesheetViewModel, modal);

                //Adding Timehseet Job/Case/Place Checkin
                AddProjectCategoryTimesheet(timesheetViewModel, modal);

                //Adding Timesheet Location Checkin
                AddTimesheetLocation(timesheetViewModel, modal);

                //Default User Location At the time of checkin
                AddTimesheetCurrentLocation(timesheetViewModel, modal);

                //Exeception Checkin Not In Range
                AddTimesheetLocationException(timesheetViewModel, modal);

                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Timesheet registered successfully." }).ConfigureAwait(false);
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

                timesheetViewModel.modified_date = _dateTime.ToString();
                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<TimesheetViewModel, Timesheet>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Timesheet>(timesheetViewModel);
                modal.modified_date = _dateTime.ToString();

                #region TimesheetWithTeamMembers

                //Remove TeamMembers with this CurrentGroupID
                _unitOfWork.TimesheetRepository.RemoveByGroupID(modal.groupid);

                foreach (var item in timesheetViewModel.team_member_empid.Distinct())
                {
                    modal.id = Guid.NewGuid().ToString();
                    modal.empid = item;
                    modal.created_date = _dateTime.ToString();
                    modal.is_deleted = false;
                    modal.groupid = timesheetViewModel.groupid;

                    modal.modified_date = _dateTime.ToString();
                    modal.modifiedby = modal.createdby;

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
                        modified_date = _dateTime.ToString(),
                        modifiedby = modal.createdby
                    };
                    _unitOfWork.TimesheetTeamRepository.Add(timesheet_team);
                }

                #endregion Teams

                #region TimesheetProjectCategory

                //Remove ProjectCategory with this CurrentGroupID
                _unitOfWork.TimesheetProjectCategoryRepository.RemoveByGroupID(modal.groupid);
                if (timesheetViewModel.TimesheetCategoryViewModel != null)
                {
                    var project_category_type = new TimesheetProjectCategory
                    {
                        id = Guid.NewGuid().ToString(),
                        //timesheet_id = modal.id,
                        groupid = modal.groupid,
                        project_category_type = timesheetViewModel.TimesheetCategoryViewModel.project_category_type,
                        project_or_comp_id = timesheetViewModel.TimesheetCategoryViewModel.project_or_comp_id,
                        project_or_comp_name = timesheetViewModel.TimesheetCategoryViewModel.project_or_comp_name,
                        project_or_comp_type = timesheetViewModel.TimesheetCategoryViewModel.project_or_comp_type,
                        is_deleted = false,
                        modified_date = _dateTime.ToString(),
                        modifiedby = modal.createdby
                    };
                    _unitOfWork.TimesheetProjectCategoryRepository.Add(project_category_type);
                }

                #endregion TimesheetProjectCategory

                #region TimesheetLocation

                _unitOfWork.TimesheetLocationRepository.RemoveByGroupID(modal.groupid);

                if (timesheetViewModel.TimesheetSearchLocationViewModel != null)
                {
                    var TimesheetLocation = new TimesheetLocation
                    {
                        id = Guid.NewGuid().ToString(),
                        groupid = modal.groupid,
                        manual_address = timesheetViewModel.TimesheetSearchLocationViewModel.formatted_address,
                        formatted_address = timesheetViewModel.TimesheetSearchLocationViewModel.formatted_address,
                        lat = timesheetViewModel.TimesheetSearchLocationViewModel.lat,
                        lang = timesheetViewModel.TimesheetSearchLocationViewModel.lang,
                        street_number = timesheetViewModel.TimesheetSearchLocationViewModel.street_number,
                        route = timesheetViewModel.TimesheetSearchLocationViewModel.route,
                        locality = timesheetViewModel.TimesheetSearchLocationViewModel.locality,
                        administrative_area_level_2 = timesheetViewModel.TimesheetSearchLocationViewModel.administrative_area_level_2,
                        administrative_area_level_1 = timesheetViewModel.TimesheetSearchLocationViewModel.administrative_area_level_1,
                        postal_code = timesheetViewModel.TimesheetSearchLocationViewModel.postal_code,
                        country = timesheetViewModel.TimesheetSearchLocationViewModel.country,
                        is_office = timesheetViewModel.TimesheetSearchLocationViewModel.is_office,
                        is_manual = timesheetViewModel.TimesheetSearchLocationViewModel.is_manual,
                        modified_date = _dateTime.ToString(),
                        modifiedby = modal.createdby
                    };
                    _unitOfWork.TimesheetLocationRepository.Add(TimesheetLocation);
                }

                #endregion TimesheetLocation

                #region CurrentLocation

                if (timesheetViewModel.TimesheetCurrentLocationViewModel != null)
                {
                    var Location = new Location
                    {
                        id = Guid.NewGuid().ToString(),
                        groupid = modal.groupid,
                        formatted_address = timesheetViewModel.TimesheetCurrentLocationViewModel.formatted_address,
                        lat = timesheetViewModel.TimesheetCurrentLocationViewModel.lat,
                        lang = timesheetViewModel.TimesheetCurrentLocationViewModel.lang,
                        street_number = timesheetViewModel.TimesheetCurrentLocationViewModel.street_number,
                        route = timesheetViewModel.TimesheetCurrentLocationViewModel.route,
                        locality = timesheetViewModel.TimesheetCurrentLocationViewModel.locality,
                        administrative_area_level_2 = timesheetViewModel.TimesheetCurrentLocationViewModel.administrative_area_level_2,
                        administrative_area_level_1 = timesheetViewModel.TimesheetCurrentLocationViewModel.administrative_area_level_1,
                        postal_code = timesheetViewModel.TimesheetCurrentLocationViewModel.postal_code,
                        country = timesheetViewModel.TimesheetCurrentLocationViewModel.country,
                        modified_date = _dateTime.ToString(),
                        modifiedby = modal.createdby
                    };
                    _unitOfWork.LocationRepository.Add(Location);
                }

                #endregion CurrentLocation

                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Timesheet updated successfully." }).ConfigureAwait(false);
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

                var Timesheet = _unitOfWork.TimesheetRepository.Find(Utils.ID);
                _unitOfWork.TimesheetRepository.Remove(Utils.ID);
                _unitOfWork.TimesheetRepository.RemoveByGroupID(Timesheet.groupid);
                _unitOfWork.TimesheetTeamRepository.RemoveByGroupID(Timesheet.groupid);
                _unitOfWork.TimesheetProjectCategoryRepository.RemoveByGroupID(Timesheet.groupid);
                _unitOfWork.TimesheetActivityRepository.RemoveByGroupID(Timesheet.groupid);
                _unitOfWork.TimesheetAdministrativeRepository.RemoveByGroupID(Timesheet.groupid);
                _unitOfWork.TimesheetBreakRepository.RemoveByGroupID(Timesheet.groupid);

                if (_unitOfWork.LocationExceptionRepository.FindByGroupID(Timesheet.groupid) != null)
                    _unitOfWork.LocationExceptionRepository.RemoveByGroupID(Timesheet.groupid);

                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Timesheet removed successfully." }).ConfigureAwait(false);
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

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("CheckOutByEmpID")]
        public async Task<object> CheckOutByEmpID([FromBody] TimesheetCheckoutViewModel timesheetViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (timesheetViewModel == null)
                    throw new ArgumentNullException(nameof(timesheetViewModel));

                foreach (var item in timesheetViewModel.team_member_empid.Distinct())
                {
                    Timesheet modal = new Timesheet();

                    modal.empid = item;
                    modal.groupid = timesheetViewModel.groupid;

                    var Timesheet = _unitOfWork.TimesheetRepository.FindTimeSheetByEmpID(modal.empid, modal.groupid);
                    if (Timesheet == null)
                    {
                        return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = "Not a valid employee", Desc = modal.empid });
                    }

                    modal.check_out = _dateTime.ToString();
                    modal.is_deleted = false;

                    var _TotalMinutes = (Convert.ToDateTime(modal.check_out) - Convert.ToDateTime(Timesheet.check_in)).Ticks;
                    TimeSpan elapsedSpan = new TimeSpan(_TotalMinutes);

                    string TotalHours;
                    string TotalMinutes;
                    ConvertHoursAndMinutes(elapsedSpan, out TotalHours, out TotalMinutes);

                    modal.total_hrs = string.Format(@"{0}:{1}", TotalHours, TotalMinutes);
                    modal.modified_date = _dateTime.ToString();
                    modal.is_checkout = true;
                    modal.check_out = _dateTime.ToString();
                    modal.modifiedby = _dateTime.ToString();

                    _unitOfWork.TimesheetRepository.CheckOutByEmpID(modal);
                }

                #region Location

                if (timesheetViewModel.TimesheetCurrentLocationViewModel != null)
                {
                    var Location = new Location
                    {
                        id = Guid.NewGuid().ToString(),
                        groupid = timesheetViewModel.groupid,
                        formatted_address = timesheetViewModel.TimesheetCurrentLocationViewModel.formatted_address,
                        lat = timesheetViewModel.TimesheetCurrentLocationViewModel.lat,
                        lang = timesheetViewModel.TimesheetCurrentLocationViewModel.lang,
                        street_number = timesheetViewModel.TimesheetCurrentLocationViewModel.street_number,
                        route = timesheetViewModel.TimesheetCurrentLocationViewModel.route,
                        locality = timesheetViewModel.TimesheetCurrentLocationViewModel.locality,
                        administrative_area_level_2 = timesheetViewModel.TimesheetCurrentLocationViewModel.administrative_area_level_2,
                        administrative_area_level_1 = timesheetViewModel.TimesheetCurrentLocationViewModel.administrative_area_level_1,
                        postal_code = timesheetViewModel.TimesheetCurrentLocationViewModel.postal_code,
                        country = timesheetViewModel.TimesheetCurrentLocationViewModel.country,
                        is_checkout = true,
                        created_date = _dateTime.ToString(),
                        createdby = timesheetViewModel.modifiedby
                    };

                    _unitOfWork.LocationRepository.Add(Location);
                }

                #endregion Location

                #region CurrentLocationException

                if (timesheetViewModel.TimesheetCurrentLocationViewModel != null)
                {
                    var Location = new LocationException
                    {
                        group_id = timesheetViewModel.groupid,
                        checkout_lat = timesheetViewModel.TimesheetCurrentLocationViewModel.lat,
                        checkout_lang = timesheetViewModel.TimesheetCurrentLocationViewModel.lang,
                        is_chkout_inrange = timesheetViewModel.is_inrange,
                        is_checkout = true,
                        modified_date = _dateTime.ToString(),
                        modifiedby = timesheetViewModel.modifiedby
                    };
                    _unitOfWork.LocationExceptionRepository.Update(Location);
                }

                #endregion CurrentLocationException

                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Checkout Successfully", Desc = "Checkout Successfully" }).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        #endregion Timesheet

        #region TimesheetBreak

        [HttpPost]
        [Route("AddTimesheetBreak")]
        public async Task<object> AddTimesheetBreak([FromBody] TimesheetBreakPostViewModel TimesheetBreakViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (TimesheetBreakViewModel == null)
                    throw new ArgumentNullException(nameof(TimesheetBreakViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<TimesheetBreakPostViewModel, TimesheetBreak>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<TimesheetBreak>(TimesheetBreakViewModel);

                if (TimesheetBreakViewModel.team_member_empid != null)
                {
                    foreach (var item in TimesheetBreakViewModel.team_member_empid.Distinct())
                    {
                        modal.id = Guid.NewGuid().ToString();
                        modal.empid = item;
                        modal.ondate = _dateTime.ToString();
                        modal.created_date = _dateTime.ToString();
                        modal.break_in = _dateTime.ToString();
                        modal.is_deleted = false;
                        modal.is_breakout = false;
                        modal.break_out = null;
                        modal.total_hrs = null;

                        _unitOfWork.TimesheetBreakRepository.Add(modal);
                    }
                }

                //Adding TimesheetBreak Location BreakIN
                AddTimesheetBreakCurrentLocation(TimesheetBreakViewModel, modal);



                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "TimesheetBreak registered successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdateTimesheetBreak")]
        public async Task<object> UpdateAddTimesheetBreak([FromBody] TimesheetBreakPostViewModel TimesheetBreakViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (TimesheetBreakViewModel == null)
                    throw new ArgumentNullException(nameof(TimesheetBreakViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<TimesheetBreakPostViewModel, TimesheetBreak>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<TimesheetBreak>(TimesheetBreakViewModel);
                modal.modified_date = _dateTime.ToString();

                #region TimesheetBreakWithTeamMembers

                //Remove TeamMembers with this CurrentGroupID
                _unitOfWork.TimesheetBreakRepository.RemoveByGroupID(modal.groupid);

                foreach (var item in TimesheetBreakViewModel.team_member_empid.Distinct())
                {
                    modal.id = Guid.NewGuid().ToString();
                    modal.empid = item;
                    modal.created_date = _dateTime.ToString();
                    modal.is_deleted = false;
                    modal.groupid = TimesheetBreakViewModel.groupid;

                    modal.modified_date = _dateTime.ToString();
                    modal.modifiedby = modal.createdby;

                    _unitOfWork.TimesheetBreakRepository.Add(modal);
                }

                #endregion TimesheetBreakWithTeamMembers

                #region CurrentLocation

                if (TimesheetBreakViewModel.TimesheetCurrentLocationViewModel != null)
                {
                    var Location = new Location
                    {
                        id = Guid.NewGuid().ToString(),
                        groupid = modal.groupid,
                        formatted_address = TimesheetBreakViewModel.TimesheetCurrentLocationViewModel.formatted_address,
                        lat = TimesheetBreakViewModel.TimesheetCurrentLocationViewModel.lat,
                        lang = TimesheetBreakViewModel.TimesheetCurrentLocationViewModel.lang,
                        street_number = TimesheetBreakViewModel.TimesheetCurrentLocationViewModel.street_number,
                        route = TimesheetBreakViewModel.TimesheetCurrentLocationViewModel.route,
                        locality = TimesheetBreakViewModel.TimesheetCurrentLocationViewModel.locality,
                        administrative_area_level_2 = TimesheetBreakViewModel.TimesheetCurrentLocationViewModel.administrative_area_level_2,
                        administrative_area_level_1 = TimesheetBreakViewModel.TimesheetCurrentLocationViewModel.administrative_area_level_1,
                        postal_code = TimesheetBreakViewModel.TimesheetCurrentLocationViewModel.postal_code,
                        country = TimesheetBreakViewModel.TimesheetCurrentLocationViewModel.country,
                        modified_date = _dateTime.ToString(),
                        modifiedby = modal.createdby
                    };
                    _unitOfWork.LocationRepository.Add(Location);
                }

                #endregion CurrentLocation

                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "TimesheetBreak updated successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemoveTimesheetBreak")]
        public async Task<object> RemoveTimesheetBreak([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(paramName: nameof(Utils.ID));

                var TimesheetBreak = _unitOfWork.TimesheetBreakRepository.Find(Utils.ID);
                _unitOfWork.TimesheetBreakRepository.Remove(Utils.ID);
                _unitOfWork.TimesheetBreakRepository.RemoveByGroupID(TimesheetBreak.groupid);

                if (_unitOfWork.LocationExceptionRepository.FindByGroupID(TimesheetBreak.groupid) != null)
                    _unitOfWork.LocationExceptionRepository.RemoveByGroupID(TimesheetBreak.groupid);

                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "TimesheetBreak removed successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetAllTimesheetBreaks")]
        public async Task<object> GetAllTimesheetBreaks(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.TimesheetBreakRepository.All();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("BreakOutByEmpIDAndGrpID")]
        public async Task<object> BreakOutByEmpIDAndGrpID([FromBody] TimesheetBreakOutViewModel TimesheetBreakOutViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (TimesheetBreakOutViewModel == null)
                    throw new ArgumentNullException(nameof(TimesheetBreakOutViewModel));

                foreach (var item in TimesheetBreakOutViewModel.team_member_empid.Distinct())
                {
                    TimesheetBreak modal = new TimesheetBreak();

                    modal.empid = item;
                    modal.groupid = TimesheetBreakOutViewModel.groupid;

                    var TimesheetBreak = _unitOfWork.TimesheetBreakRepository.FindTimeSheetBreakByEmpID(modal.empid, modal.groupid);
                    if (TimesheetBreak == null)
                    {
                        return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = "Not a valid employee", Desc = modal.empid });
                    }

                    modal.break_out = _dateTime.ToString();
                    modal.is_deleted = false;

                    var _TotalMinutes = (Convert.ToDateTime(modal.break_out) - Convert.ToDateTime(TimesheetBreak.break_in)).Ticks;
                    TimeSpan elapsedSpan = new TimeSpan(_TotalMinutes);

                    string TotalHours;
                    string TotalMinutes;
                    ConvertHoursAndMinutes(elapsedSpan, out TotalHours, out TotalMinutes);

                    modal.total_hrs = string.Format(@"{0}:{1}", TotalHours, TotalMinutes);
                    modal.modified_date = _dateTime.ToString();
                    modal.is_breakout = true;
                    modal.break_out = _dateTime.ToString();
                    modal.modifiedby = _dateTime.ToString();

                    _unitOfWork.TimesheetBreakRepository.BreakOutByEmpIDAndGrpID(modal);
                }

                #region Location

                if (TimesheetBreakOutViewModel.TimesheetCurrentLocationViewModel != null)
                {
                    var Location = new Location
                    {
                        id = Guid.NewGuid().ToString(),
                        groupid = TimesheetBreakOutViewModel.groupid,
                        formatted_address = TimesheetBreakOutViewModel.TimesheetCurrentLocationViewModel.formatted_address,
                        lat = TimesheetBreakOutViewModel.TimesheetCurrentLocationViewModel.lat,
                        lang = TimesheetBreakOutViewModel.TimesheetCurrentLocationViewModel.lang,
                        street_number = TimesheetBreakOutViewModel.TimesheetCurrentLocationViewModel.street_number,
                        route = TimesheetBreakOutViewModel.TimesheetCurrentLocationViewModel.route,
                        locality = TimesheetBreakOutViewModel.TimesheetCurrentLocationViewModel.locality,
                        administrative_area_level_2 = TimesheetBreakOutViewModel.TimesheetCurrentLocationViewModel.administrative_area_level_2,
                        administrative_area_level_1 = TimesheetBreakOutViewModel.TimesheetCurrentLocationViewModel.administrative_area_level_1,
                        postal_code = TimesheetBreakOutViewModel.TimesheetCurrentLocationViewModel.postal_code,
                        country = TimesheetBreakOutViewModel.TimesheetCurrentLocationViewModel.country,
                        is_checkout = true,
                        created_date = _dateTime.ToString(),
                        createdby = TimesheetBreakOutViewModel.modifiedby
                    };

                    _unitOfWork.LocationRepository.Add(Location);
                }



                #endregion Location

                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Checkout Successfully", Desc = "Checkout Successfully" }).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindLastTimeSheetBreakByEmpIDAndGrpID")]
        public async Task<object> FindLastTimeSheetBreakByEmpIDAndGrpID([FromBody] UtilsEmpIDAndGrpID utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();


                if (utils == null)
                    throw new ArgumentNullException(nameof(utils));

                var result = _unitOfWork.TimesheetBreakRepository.FindLastTimeSheetBreakByEmpIDAndGrpID(utils.EmpID, utils.GrpID);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        #endregion TimesheetBreak

        #region TimesheetActivity

        [HttpPost]
        [Route("AddTimesheetActivity")]
        public async Task<object> AddTimesheetActivity([FromBody] TimesheetActivityViewModel timesheetActivityViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (timesheetActivityViewModel == null)
                    throw new ArgumentNullException(nameof(timesheetActivityViewModel));

                if ((timesheetActivityViewModel.groupid != null) && (timesheetActivityViewModel.groupid == ""
                                    || string.IsNullOrWhiteSpace(timesheetActivityViewModel.groupid)
                                    || string.IsNullOrEmpty(timesheetActivityViewModel.groupid)))
                    return await Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = "Error", Desc = "Invalid GroupID" }).ConfigureAwait(false);

                timesheetActivityViewModel.id = Guid.NewGuid().ToString();
                timesheetActivityViewModel.is_deleted = false;
                timesheetActivityViewModel.ondate = _dateTime.ToString();
                timesheetActivityViewModel.created_date = _dateTime.ToString();

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<TimesheetActivityViewModel, TimesheetActivity>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<TimesheetActivity>(timesheetActivityViewModel);

                var _TotalMinutes = (Convert.ToDateTime(modal.end_time) - Convert.ToDateTime(modal.start_time)).Ticks;
                TimeSpan elapsedSpan = new TimeSpan(_TotalMinutes);

                string TotalHours;
                string TotalMinutes;
                ConvertHoursAndMinutes(elapsedSpan, out TotalHours, out TotalMinutes);

                modal.total_hrs = string.Format(@"{0}:{1}", TotalHours, TotalMinutes);

                //update status
                if (timesheetActivityViewModel.status_id != null)
                {
                    var modalTasks = new Domain.Entities.Tasks()
                    {
                        id = timesheetActivityViewModel.task_id,
                        status_id = timesheetActivityViewModel.status_id,
                        modifiedby = timesheetActivityViewModel.modifiedby,
                        modified_date = _dateTime.ToString()
                    };
                    _unitOfWork.TaskRepository.UpdateTaskStatus(modalTasks);
                }

                _unitOfWork.TimesheetActivityRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Timesheet Activity added successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdateTimesheetActivity")]
        public async Task<object> UpdateAddTimesheetActivity([FromBody] TimesheetActivityViewModel timesheetActivityViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (timesheetActivityViewModel == null)
                    throw new ArgumentNullException(nameof(timesheetActivityViewModel));

                timesheetActivityViewModel.modified_date = _dateTime.ToString();
                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<TimesheetActivityViewModel, TimesheetActivity>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<TimesheetActivity>(timesheetActivityViewModel);
                modal.modified_date = _dateTime.ToString();

                _unitOfWork.TimesheetActivityRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Timesheet Activity updated successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemoveTimesheetActivity")]
        public async Task<object> RemoveTimesheetActivity([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(paramName: nameof(Utils.ID));

                _unitOfWork.TimesheetActivityRepository.Remove(Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Timesheet Activity removed successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetAllTimesheetActivitys")]
        public async Task<object> GetAllTimesheetActivitys(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.TimesheetActivityRepository.All();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("GetTop10TimesheetActivityOnTaskID")]
        public async Task<object> GetTop10TimesheetActivityOnTaskID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                oDataTable _oDataTable = new oDataTable();
                var result = _unitOfWork.TimesheetActivityRepository.GetTop10TimesheetActivityOnTaskID(Utils.ID);
                var xResult = _oDataTable.ToDataTable(result);

                return await System.Threading.Tasks.Task.FromResult<object>(JsonConvert.SerializeObject(xResult, Formatting.Indented)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("GetTimesheetActivityByGroupAndProjectID")]
        public async Task<object> GetTimesheetActivityByGroupAndProjectID([FromBody] UtilsGroupIDAndProjectID Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.TimesheetActivityRepository.GetTimesheetActivityByGroupAndProjectID(Utils.GroupID, Utils.ProjectID, Utils.Date.ToString());
                var GroupID = result.Select(x => x.groupid).ToList();

                for (int i = 0; i < GroupID.Count(); i++)
                {
                    var Members = String.Join(", ", _unitOfWork.TimesheetRepository.GetAllEmpByGroupID(GroupID[i]));

                    result.Where(usr => usr.groupid == GroupID[i])
                   .Select(usr => { usr.members = Members; return usr; })
                   .ToList();
                }

                return await System.Threading.Tasks.Task.FromResult<object>(JsonConvert.SerializeObject(result, Formatting.Indented)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("GetTimesheetActivityByEmpIDAndDate")]
        public async Task<object> GetTimesheetActivityByEmpIDAndDate([FromBody] UtilsEmpIDAndDate Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                oDataTable _oDataTable = new oDataTable();
                var result = _unitOfWork.TimesheetActivityRepository.GetTimesheetActivityByEmpID(Utils.EmpID, Utils.StartDate, Utils.EndDate);

                var GroupID = result.Select(x => x.groupid).ToList();
                for (int i = 0; i < GroupID.Count; i++)
                {
                    var Members = String.Join(", ", _unitOfWork.TimesheetRepository.GetAllEmpByGroupID(GroupID[i]));

                    result.Where(usr => usr.groupid == GroupID[i])
                   .Select(usr => { usr.members = Members; return usr; })
                   .ToList();
                }

                return await System.Threading.Tasks.Task.FromResult<object>(JsonConvert.SerializeObject(result, Formatting.Indented)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        #endregion TimesheetActivity

        #region TimesheetAdministraitiveActivity

        [HttpPost]
        [Route("AddTimesheetAdministrativeActivity")]
        public async Task<object> AddTimesheetAdministrativeActivity([FromBody] TimesheetAdministrativeActivityViewModel timesheetAdministrativeActivityViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (timesheetAdministrativeActivityViewModel == null)
                    throw new ArgumentNullException(nameof(timesheetAdministrativeActivityViewModel));

                if ((timesheetAdministrativeActivityViewModel.groupid != null) && (timesheetAdministrativeActivityViewModel.groupid == ""
                                    || string.IsNullOrWhiteSpace(timesheetAdministrativeActivityViewModel.groupid)
                                    || string.IsNullOrEmpty(timesheetAdministrativeActivityViewModel.groupid)))
                    return await Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = "Error", Desc = "Invalid GroupID" }).ConfigureAwait(false);

                timesheetAdministrativeActivityViewModel.id = Guid.NewGuid().ToString();
                timesheetAdministrativeActivityViewModel.is_deleted = false;
                timesheetAdministrativeActivityViewModel.ondate = _dateTime.ToString();
                timesheetAdministrativeActivityViewModel.created_date = _dateTime.ToString();

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<TimesheetAdministrativeActivityViewModel, TimesheetAdministrative>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<TimesheetAdministrative>(timesheetAdministrativeActivityViewModel);

                //var _TotalMinutes = (Convert.ToDateTime(modal.start_time) - Convert.ToDateTime(modal.end_time)).TotalMinutes;
                //TimeSpan spWorkMin = TimeSpan.FromMinutes(_TotalMinutes);
                //modal.total_hrs = spWorkMin.ToString(FormatTime);

                var _TotalMinutes = (Convert.ToDateTime(modal.end_time) - Convert.ToDateTime(modal.start_time)).Ticks;
                TimeSpan elapsedSpan = new TimeSpan(_TotalMinutes);

                string TotalHours;
                string TotalMinutes;
                ConvertHoursAndMinutes(elapsedSpan, out TotalHours, out TotalMinutes);

                modal.total_hrs = string.Format(@"{0}:{1}", TotalHours, TotalMinutes);

                _unitOfWork.TimesheetAdministrativeRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Timesheet Activity added successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdateTimesheetAdministrativeActivity")]
        public async Task<object> UpdateAddTimesheetAdministrativeActivity([FromBody] TimesheetAdministrativeActivityViewModel timesheetAdministrativeActivityViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (timesheetAdministrativeActivityViewModel == null)
                    throw new ArgumentNullException(nameof(timesheetAdministrativeActivityViewModel));

                timesheetAdministrativeActivityViewModel.modified_date = _dateTime.ToString();
                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<TimesheetAdministrativeActivityViewModel, TimesheetAdministrative>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<TimesheetAdministrative>(timesheetAdministrativeActivityViewModel);
                modal.modified_date = _dateTime.ToString();

                _unitOfWork.TimesheetAdministrativeRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Timesheet Activity updated successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemoveTimesheetAdministrativeActivity")]
        public async Task<object> RemoveTimesheetAdministrativeActivity([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(paramName: nameof(Utils.ID));

                _unitOfWork.TimesheetAdministrativeRepository.Remove(Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Timesheet Activity removed successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetAllTimesheetAdministrativeActivity")]
        public async Task<object> GetAllTimesheetAdministrativeActivity(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.TimesheetAdministrativeRepository.All();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("GetTop10TimesheetAdminActivityOnGroupIDAndAdminID")]
        public async Task<object> GetTop10TimesheetAdminActivityOnGroupIDAndAdminID([FromBody] UtilsGroupID Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                oDataTable _oDataTable = new oDataTable();
                var result = _unitOfWork.TimesheetAdministrativeRepository.GetTop10TimesheetAdminActivityOnGroupIDAndAdminID(Utils.GroupID, Utils.AdministrativeID);
                var xResult = _oDataTable.ToDataTable(result);

                return await System.Threading.Tasks.Task.FromResult<object>(JsonConvert.SerializeObject(xResult, Formatting.Indented)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        #endregion TimesheetAdministraitiveActivity

        #region TimesheetActivityComment

        [HttpPost]
        [Route("AddTimesheetActivityComment")]
        public async Task<object> AddTimesheetActivityComment([FromBody] TimesheetActivityCommentViewModel timesheetActivityCommentViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (timesheetActivityCommentViewModel == null)
                    throw new ArgumentNullException(nameof(timesheetActivityCommentViewModel));

                timesheetActivityCommentViewModel.is_deleted = false;
                timesheetActivityCommentViewModel.ondate = _dateTime.ToString();
                timesheetActivityCommentViewModel.created_date = _dateTime.ToString();

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<TimesheetActivityCommentViewModel, TimesheetActivityComment>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<TimesheetActivityComment>(timesheetActivityCommentViewModel);

                _unitOfWork.TimesheetActivityCommentRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Comment added successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdateTimesheetActivityComment")]
        public async Task<object> UpdateAddTimesheetActivityComment([FromBody] TimesheetActivityCommentViewModel timesheetActivityCommentViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (timesheetActivityCommentViewModel == null)
                    throw new ArgumentNullException(nameof(timesheetActivityCommentViewModel));

                timesheetActivityCommentViewModel.modified_date = _dateTime.ToString();
                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<TimesheetActivityCommentViewModel, TimesheetActivityComment>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<TimesheetActivityComment>(timesheetActivityCommentViewModel);
                modal.modified_date = _dateTime.ToString();

                _unitOfWork.TimesheetActivityCommentRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Comment updated successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemoveTimesheetActivityComment")]
        public async Task<object> RemoveTimesheetActivityComment([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(paramName: nameof(Utils.ID));

                _unitOfWork.TimesheetActivityCommentRepository.Remove(Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Comment removed successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetAllTimesheetActivityComments")]
        public async Task<object> GetAllTimesheetActivityComments(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.TimesheetActivityCommentRepository.All();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        #endregion TimesheetActivityComment

        #region TimesheetActivityFile

        [HttpPost]
        [Route("AddTimesheetActivityFile")]
        public async Task<object> AddTimesheetActivityFile([FromBody] TimesheetActivityFileViewModel timesheetActivityFileViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (timesheetActivityFileViewModel == null)
                    throw new ArgumentNullException(nameof(timesheetActivityFileViewModel));

                timesheetActivityFileViewModel.is_deleted = false;
                timesheetActivityFileViewModel.ondate = _dateTime.ToString();
                timesheetActivityFileViewModel.created_date = _dateTime.ToString();

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<TimesheetActivityFileViewModel, TimesheetActivityFile>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<TimesheetActivityFile>(timesheetActivityFileViewModel);

                _unitOfWork.TimesheetActivityFileRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Comment added successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdateTimesheetActivityFile")]
        public async Task<object> UpdateAddTimesheetActivityFile([FromBody] TimesheetActivityFileViewModel timesheetActivityFileViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (timesheetActivityFileViewModel == null)
                    throw new ArgumentNullException(nameof(timesheetActivityFileViewModel));

                timesheetActivityFileViewModel.modified_date = _dateTime.ToString();
                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<TimesheetActivityFileViewModel, TimesheetActivityFile>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<TimesheetActivityFile>(timesheetActivityFileViewModel);
                modal.modified_date = _dateTime.ToString();

                _unitOfWork.TimesheetActivityFileRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Comment updated successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemoveTimesheetActivityFile")]
        public async Task<object> RemoveTimesheetActivityFile([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(paramName: nameof(Utils.ID));

                _unitOfWork.TimesheetActivityFileRepository.Remove(Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Comment removed successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetAllTimesheetActivityFiles")]
        public async Task<object> GetAllTimesheetActivityFiles(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.TimesheetActivityFileRepository.All();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        #endregion TimesheetActivityFile

        #region Private

        private void AddTimesheetWithTeamMembers(TimesheetPostViewModel timesheetViewModel, Timesheet modal)
        {
            #region TimesheetWithTeamMembers

            if (timesheetViewModel.team_member_empid != null)
            {
                foreach (var item in timesheetViewModel.team_member_empid.Distinct())
                {
                    modal.id = Guid.NewGuid().ToString();
                    modal.empid = item;
                    modal.ondate = _dateTime.ToString();
                    modal.created_date = _dateTime.ToString();
                    modal.check_in = _dateTime.ToString();
                    modal.is_deleted = false;
                    modal.is_checkout = false;
                    modal.check_out = null;
                    modal.total_hrs = null;

                    _unitOfWork.TimesheetRepository.Add(modal);
                }
            }

            #endregion TimesheetWithTeamMembers
        }

        private void AddTeamsTimesheet(TimesheetPostViewModel timesheetViewModel, Timesheet modal)
        {
            #region Teams

            if (timesheetViewModel.teamid != null)
            {
                foreach (var item in timesheetViewModel.teamid.Distinct())
                {
                    var timesheet_team = new TimesheetTeam
                    {
                        id = Guid.NewGuid().ToString(),
                        teamid = item,
                        groupid = modal.groupid,
                        is_deleted = false,
                        created_date = _dateTime.ToString(),
                        createdby = modal.createdby
                    };
                    _unitOfWork.TimesheetTeamRepository.Add(timesheet_team);
                }
            }

            #endregion Teams
        }

        private void AddProjectCategoryTimesheet(TimesheetPostViewModel timesheetViewModel, Timesheet modal)
        {
            #region TimesheetProjectCategory

            if (timesheetViewModel.TimesheetCategoryViewModel != null)
            {
                var project_category_type = new TimesheetProjectCategory
                {
                    id = Guid.NewGuid().ToString(),
                    //timesheet_id = modal.id,
                    groupid = modal.groupid,
                    project_category_type = timesheetViewModel.TimesheetCategoryViewModel.project_category_type,
                    project_or_comp_id = timesheetViewModel.TimesheetCategoryViewModel.project_or_comp_id,
                    project_or_comp_name = timesheetViewModel.TimesheetCategoryViewModel.project_or_comp_name,
                    //project_or_comp_type = timesheetViewModel.TimesheetCategoryViewModel.project_or_comp_type,
                    is_deleted = false,
                    created_date = _dateTime.ToString(),
                    createdby = modal.createdby
                };
                _unitOfWork.TimesheetProjectCategoryRepository.Add(project_category_type);
            }

            #endregion TimesheetProjectCategory
        }

        private void AddTimesheetLocation(TimesheetPostViewModel timesheetViewModel, Timesheet modal)
        {
            #region TimesheetLocation

            if (timesheetViewModel.TimesheetSearchLocationViewModel != null)
            {
                var TimesheetLocation = new TimesheetLocation
                {
                    id = Guid.NewGuid().ToString(),
                    groupid = modal.groupid,
                    manual_address = timesheetViewModel.TimesheetSearchLocationViewModel.manual_address,
                    geo_address = timesheetViewModel.TimesheetSearchLocationViewModel.geo_address,
                    formatted_address = timesheetViewModel.TimesheetSearchLocationViewModel.formatted_address,
                    lat = timesheetViewModel.TimesheetSearchLocationViewModel.lat,
                    lang = timesheetViewModel.TimesheetSearchLocationViewModel.lang,
                    street_number = timesheetViewModel.TimesheetSearchLocationViewModel.street_number,
                    route = timesheetViewModel.TimesheetSearchLocationViewModel.route,
                    locality = timesheetViewModel.TimesheetSearchLocationViewModel.locality,
                    administrative_area_level_2 = timesheetViewModel.TimesheetSearchLocationViewModel.administrative_area_level_2,
                    administrative_area_level_1 = timesheetViewModel.TimesheetSearchLocationViewModel.administrative_area_level_1,
                    postal_code = timesheetViewModel.TimesheetSearchLocationViewModel.postal_code,
                    country = timesheetViewModel.TimesheetSearchLocationViewModel.country,
                    is_deleted = false,
                    is_office = timesheetViewModel.TimesheetSearchLocationViewModel.is_office,
                    is_manual = timesheetViewModel.TimesheetSearchLocationViewModel.is_manual,
                    created_date = _dateTime.ToString(),
                    createdby = modal.createdby
                };
                _unitOfWork.TimesheetLocationRepository.Add(TimesheetLocation);
            }

            #endregion TimesheetLocation
        }

        private void AddTimesheetCurrentLocation(TimesheetPostViewModel timesheetViewModel, Timesheet modal)
        {
            #region CurrentLocation

            if (timesheetViewModel.TimesheetCurrentLocationViewModel != null)
            {
                var Location = new Location
                {
                    id = Guid.NewGuid().ToString(),
                    groupid = modal.groupid,
                    geo_address = timesheetViewModel.TimesheetCurrentLocationViewModel.geo_address,
                    formatted_address = timesheetViewModel.TimesheetCurrentLocationViewModel.formatted_address,
                    lat = timesheetViewModel.TimesheetCurrentLocationViewModel.lat,
                    lang = timesheetViewModel.TimesheetCurrentLocationViewModel.lang,
                    street_number = timesheetViewModel.TimesheetCurrentLocationViewModel.street_number,
                    route = timesheetViewModel.TimesheetCurrentLocationViewModel.route,
                    locality = timesheetViewModel.TimesheetCurrentLocationViewModel.locality,
                    administrative_area_level_2 = timesheetViewModel.TimesheetCurrentLocationViewModel.administrative_area_level_2,
                    administrative_area_level_1 = timesheetViewModel.TimesheetCurrentLocationViewModel.administrative_area_level_1,
                    postal_code = timesheetViewModel.TimesheetCurrentLocationViewModel.postal_code,
                    country = timesheetViewModel.TimesheetCurrentLocationViewModel.country,
                    is_deleted = false,
                    is_checkout = false,
                    created_date = _dateTime.ToString(),
                    createdby = modal.createdby
                };
                _unitOfWork.LocationRepository.Add(Location);
            }

            #endregion CurrentLocation
        }

        private void AddTimesheetBreakCurrentLocation(TimesheetBreakPostViewModel timesheetViewModel, TimesheetBreak modal)
        {
            #region CurrentLocation

            if (timesheetViewModel.TimesheetCurrentLocationViewModel != null)
            {
                var Location = new Location
                {
                    id = Guid.NewGuid().ToString(),
                    groupid = modal.groupid,
                    geo_address = timesheetViewModel.TimesheetCurrentLocationViewModel.geo_address,
                    formatted_address = timesheetViewModel.TimesheetCurrentLocationViewModel.formatted_address,
                    lat = timesheetViewModel.TimesheetCurrentLocationViewModel.lat,
                    lang = timesheetViewModel.TimesheetCurrentLocationViewModel.lang,
                    street_number = timesheetViewModel.TimesheetCurrentLocationViewModel.street_number,
                    route = timesheetViewModel.TimesheetCurrentLocationViewModel.route,
                    locality = timesheetViewModel.TimesheetCurrentLocationViewModel.locality,
                    administrative_area_level_2 = timesheetViewModel.TimesheetCurrentLocationViewModel.administrative_area_level_2,
                    administrative_area_level_1 = timesheetViewModel.TimesheetCurrentLocationViewModel.administrative_area_level_1,
                    postal_code = timesheetViewModel.TimesheetCurrentLocationViewModel.postal_code,
                    country = timesheetViewModel.TimesheetCurrentLocationViewModel.country,
                    is_deleted = false,
                    is_checkout = false,
                    created_date = _dateTime.ToString(),
                    createdby = modal.createdby
                };
                _unitOfWork.LocationRepository.Add(Location);
            }

            #endregion CurrentLocation
        }

        private void AddTimesheetLocationException(TimesheetPostViewModel timesheetViewModel, Timesheet modal)
        {
            #region CurrentLocationException

            if (timesheetViewModel.TimesheetCurrentLocationViewModel != null)
            {
                var Location = new LocationException
                {
                    id = Guid.NewGuid().ToString(),
                    group_id = modal.groupid,
                    checkin_lat = timesheetViewModel.TimesheetCurrentLocationViewModel.lat,
                    checkin_lang = timesheetViewModel.TimesheetCurrentLocationViewModel.lang,
                    is_chkin_inrange = timesheetViewModel.is_inrange,
                    is_deleted = false,
                    is_checkout = false,
                    created_date = _dateTime.ToString(),
                    createdby = modal.createdby
                };
                _unitOfWork.LocationExceptionRepository.Add(Location);
            }

            #endregion CurrentLocationException
        }

        private static void ConvertHoursAndMinutes(TimeSpan elapsedSpan, out string TotalHours, out string TotalMinutes)
        {
            if (elapsedSpan.TotalHours.ToString().Split('.')[0].Length == 1)
                TotalHours = "0" + elapsedSpan.TotalHours.ToString().Split('.')[0].ToString();
            else
                TotalHours = elapsedSpan.TotalHours.ToString().Split('.')[0].ToString();

            if (elapsedSpan.Minutes.ToString().Split('.')[0].Length == 1)
                TotalMinutes = "0" + elapsedSpan.Minutes.ToString().Split('.')[0].ToString();
            else
                TotalMinutes = elapsedSpan.Minutes.ToString().Split('.')[0].ToString();
        }

        #endregion Private

        //[HttpPost]
        //public string Post([FromBody]Message msg)
        //{
        //    string retMessage;

        //    try
        //    {
        //        _hubContext.Clients.All.BroadcastMessage(msg.Type, msg.Payload);
        //        retMessage = "Success";
        //    }
        //    catch (Exception e)
        //    {
        //        retMessage = e.ToString();
        //    }

        //    return retMessage;
        //}

        [HttpPost]
        [Route("FindLocationByEntityID")]
        public async Task<object> FindLocationByEntityID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));
                var resultLocation = _unitOfWork.EntityLocationRepository.FindByEnitiyID(Utils.ID);

                return await Task.FromResult<object>(resultLocation).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }


        #region TimesheetDesk

        [HttpPost]
        [Route("AddTimesheetDesk")]
        public async Task<object> AddTimesheetDesk([FromBody] TimesheetDeskPostViewModel timesheetViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (timesheetViewModel == null)
                    throw new ArgumentNullException(nameof(timesheetViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<TimesheetDeskPostViewModel, TimesheetDesk>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<TimesheetDesk>(timesheetViewModel);

                var _groupid = modal.groupid = Guid.NewGuid().ToString();
                modal.id = Guid.NewGuid().ToString();
                modal.created_date = _dateTime.ToString();
                modal.check_in = _dateTime.ToString();
                modal.ondate = _dateTime.ToString();
                modal.is_checkout = false;
                modal.is_deleted = false;

                _unitOfWork.TimesheetDeskRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(_groupid).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdateTimesheetDesk")]
        public async Task<object> UpdateTimesheetDesk([FromBody] TimesheetDeskViewModel timesheetViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (timesheetViewModel == null)
                    throw new ArgumentNullException(nameof(timesheetViewModel));

                timesheetViewModel.modified_date = _dateTime.ToString();
                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<TimesheetDeskViewModel, TimesheetDesk>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<TimesheetDesk>(timesheetViewModel);
                modal.modified_date = _dateTime.ToString();

                _unitOfWork.TimesheetDeskRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "TimesheetDesk updated successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemoveTimesheetDesk")]
        public async Task<object> RemoveTimesheetDesk([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(paramName: nameof(Utils.ID));

                var TimesheetDesk = _unitOfWork.TimesheetDeskRepository.Find(Utils.ID);
                _unitOfWork.TimesheetDeskRepository.Remove(Utils.ID);
                _unitOfWork.TimesheetDeskRepository.RemoveByGroupID(TimesheetDesk.groupid);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "TimesheetDesk removed successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetAllTimesheetDesksDesk")]
        public async Task<object> GetAllTimesheetDesksDesk(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.TimesheetDeskRepository.All();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("CheckOutByEmpIDAndGroupID")]
        public async Task<object> CheckOutByEmpIDAndGroupID([FromBody] TimesheetDeskCheckoutViewModel timesheetViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (timesheetViewModel == null)
                    throw new ArgumentNullException(nameof(timesheetViewModel));

                TimesheetDesk modal = new TimesheetDesk();

                modal.empid = timesheetViewModel.empid;
                modal.groupid = timesheetViewModel.groupid;

                var TimesheetDesk = _unitOfWork.TimesheetDeskRepository.FindTimeSheetDeskByEmpID(modal.empid, modal.groupid);
                if (TimesheetDesk == null)
                {
                    return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = "Not a valid employee", Desc = modal.empid });
                }

                modal.check_out = _dateTime.ToString();
                modal.is_deleted = false;

                var _TotalMinutes = (Convert.ToDateTime(modal.check_out) - Convert.ToDateTime(TimesheetDesk.check_in)).Ticks;
                TimeSpan elapsedSpan = new TimeSpan(_TotalMinutes);

                string TotalHours;
                string TotalMinutes;
                ConvertHoursAndMinutes(elapsedSpan, out TotalHours, out TotalMinutes);

                modal.total_hrs = string.Format(@"{0}:{1}", TotalHours, TotalMinutes);
                modal.modified_date = _dateTime.ToString();
                modal.is_checkout = true;
                modal.check_out = _dateTime.ToString();
                modal.modifiedby = _dateTime.ToString();

                _unitOfWork.TimesheetDeskRepository.CheckOutByEmpID(modal);

                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Checkout Successfully", Desc = "Checkout Successfully" }).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("AddOfflineTimesheetDesk")]
        public async Task<object> AddOfflineTimesheetDesk([FromBody] IEnumerable<TimesheetDesk> timesheetViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (timesheetViewModel == null)
                    throw new ArgumentNullException(nameof(timesheetViewModel));

                foreach (var item in timesheetViewModel)
                {
                    var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<TimesheetDesk, TimesheetDesk>());
                    var mapper = config.CreateMapper();
                    var modal = mapper.Map<TimesheetDesk>(item);
                    modal.id = Guid.NewGuid().ToString();
                    //modal.created_date = _dateTime.ToString();
                    //modal.check_in = _dateTime.ToString();
                    //modal.ondate = _dateTime.ToString();
                    //modal.is_checkout = false;
                    modal.is_deleted = false;

                    _unitOfWork.TimesheetDeskRepository.Add(modal);
                    _unitOfWork.Commit();
                }

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Added Successfully", Desc = "Added Successfully" }).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        #endregion TimesheetDesk
    }
}