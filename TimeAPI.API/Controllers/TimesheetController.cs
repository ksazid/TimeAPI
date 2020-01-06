using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TimeAPI.API.Models;
using TimeAPI.API.Models.TimesheetActivityCommentViewModels;
using TimeAPI.API.Models.TimesheetActivityFileViewModels;
using TimeAPI.API.Models.TimesheetActivityViewModels;
using TimeAPI.API.Models.TimesheetViewModels;
using TimeAPI.API.Services;
using TimeAPI.Domain;
using TimeAPI.Domain.Entities;

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
            IEmailSender emailSender, IOptions<ApplicationSettings> AppSettings)
        {
            _emailSender = emailSender;
            _logger = logger;
            _appSettings = AppSettings.Value;
            _unitOfWork = unitOfWork;
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

                #region TimesheetWithTeamMembers

                if (timesheetViewModel.team_member_empid != null)
                {
                    foreach (var item in timesheetViewModel.team_member_empid.Distinct())
                    {
                        modal.id = Guid.NewGuid().ToString();
                        modal.empid = item;
                        modal.ondate = DateTime.Now.ToString(CultureInfo.CurrentCulture);
                        modal.created_date = DateTime.Now.ToString(CultureInfo.CurrentCulture);
                        modal.is_deleted = false;
                        modal.groupid = _groupid;
                        modal.is_deleted = false;
                        modal.is_checkout = false;
                        modal.check_out = null;
                        modal.total_hrs = null;

                        _unitOfWork.TimesheetRepository.Add(modal);
                    }
                }

                #endregion TimesheetWithTeamMembers

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
                            created_date = DateTime.Now.ToString(CultureInfo.CurrentCulture),
                            createdby = modal.createdby
                        };
                        _unitOfWork.TimesheetTeamRepository.Add(timesheet_team);
                    }
                }

                #endregion Teams

                #region TimesheetProjectCategory

                if (timesheetViewModel.TimesheetCategoryViewModel != null)
                {
                    var project_category_type = new TimesheetProjectCategory
                    {
                        id = Guid.NewGuid().ToString(),
                        //timesheet_id = modal.id,
                        groupid = modal.groupid,
                        project_category_id = timesheetViewModel.TimesheetCategoryViewModel.project_category_id,
                        project_or_comp_id = timesheetViewModel.TimesheetCategoryViewModel.project_or_comp_id,
                        is_deleted = false,
                        created_date = DateTime.Now.ToString(CultureInfo.CurrentCulture),
                        createdby = modal.createdby
                    };
                    _unitOfWork.TimesheetProjectCategoryRepository.Add(project_category_type);
                }

                #endregion TimesheetProjectCategory

                #region TimesheetAdministrative

                if (timesheetViewModel.TimesheetAdministrativeViewModel != null)
                {
                    foreach (var item in timesheetViewModel.TimesheetAdministrativeViewModel.administrative_id.Distinct())
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
                }

                #endregion TimesheetAdministrative

                #region TimesheetProjectCategory

                if (timesheetViewModel.TimesheetCategoryViewModel != null)
                {
                    var project_category_type = new TimesheetProjectCategory
                    {
                        id = Guid.NewGuid().ToString(),
                        //timesheet_id = modal.id,
                        groupid = modal.groupid,
                        project_category_id = timesheetViewModel.TimesheetCategoryViewModel.project_category_id,
                        project_or_comp_id = timesheetViewModel.TimesheetCategoryViewModel.project_or_comp_id,
                        is_deleted = false,
                        created_date = DateTime.Now.ToString(CultureInfo.CurrentCulture),
                        createdby = modal.createdby
                    };
                    _unitOfWork.TimesheetProjectCategoryRepository.Add(project_category_type);
                }

                #endregion TimesheetProjectCategory

                #region TimesheetLocation

                if (timesheetViewModel.TimesheetSearchLocationViewModel != null)
                {
                    var TimesheetLocation = new TimesheetLocation
                    {
                        id = Guid.NewGuid().ToString(),
                        groupid = modal.groupid,
                        manual_address = timesheetViewModel.TimesheetSearchLocationViewModel.manual_address,
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
                        created_date = DateTime.Now.ToString(CultureInfo.CurrentCulture),
                        createdby = modal.createdby
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
                        is_deleted = false,
                        is_checkout = false,
                        created_date = DateTime.Now.ToString(CultureInfo.CurrentCulture),
                        createdby = modal.createdby
                    };
                    _unitOfWork.LocationRepository.Add(Location);
                }

                #endregion CurrentLocation

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

                    modal.modified_date = DateTime.Now.ToString(CultureInfo.CurrentCulture);
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
                        modified_date = DateTime.Now.ToString(CultureInfo.CurrentCulture),
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
                        project_category_id = timesheetViewModel.TimesheetCategoryViewModel.project_category_id,
                        project_or_comp_id = timesheetViewModel.TimesheetCategoryViewModel.project_or_comp_id,
                        is_deleted = false,
                        modified_date = DateTime.Now.ToString(CultureInfo.CurrentCulture),
                        modifiedby = modal.createdby
                    };
                    _unitOfWork.TimesheetProjectCategoryRepository.Add(project_category_type);
                }

                #endregion TimesheetProjectCategory

                #region TimesheetAdministrative

                _unitOfWork.TimesheetAdministrativeRepository.RemoveByGroupID(modal.groupid);
                foreach (var item in timesheetViewModel.TimesheetAdministrativeViewModel.administrative_id.Distinct())
                {
                    var project_administrative = new TimesheetAdministrative
                    {
                        id = Guid.NewGuid().ToString(),
                        administrative_id = item,
                        groupid = modal.groupid,
                        is_deleted = false,
                        modified_date = DateTime.Now.ToString(CultureInfo.CurrentCulture),
                        modifiedby = modal.createdby
                    };
                    _unitOfWork.TimesheetAdministrativeRepository.Add(project_administrative);
                }

                #endregion TimesheetAdministrative

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
                        modified_date = DateTime.Now.ToString(CultureInfo.CurrentCulture),
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
                        modified_date = DateTime.Now.ToString(CultureInfo.CurrentCulture),
                        modifiedby = modal.createdby
                    };
                    _unitOfWork.LocationRepository.Add(Location);
                }

                #endregion CurrentLocation

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

                    var _TotalMinutes = (Convert.ToDateTime(modal.check_out) - Convert.ToDateTime(Timesheet.check_in)).TotalMinutes;
                    TimeSpan spWorkMin = TimeSpan.FromMinutes(_TotalMinutes);

                    modal.total_hrs = spWorkMin.ToString(FormatTime);
                    modal.modified_date = DateTime.Now.ToString(CultureInfo.CurrentCulture);
                    modal.is_checkout = true;
                    modal.is_deleted = false;
                    modal.check_out = timesheetViewModel.check_out;
                    modal.modifiedby = timesheetViewModel.modifiedby;

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
                        created_date = DateTime.Now.ToString(CultureInfo.CurrentCulture),
                        createdby = timesheetViewModel.modifiedby
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

        #endregion Timesheet

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

                if (timesheetActivityViewModel.groupid == "string" || string.IsNullOrWhiteSpace(timesheetActivityViewModel.groupid) || string.IsNullOrEmpty(timesheetActivityViewModel.groupid))
                    return await Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = "Error", Desc = "Invalid GroupID" }).ConfigureAwait(false);

                timesheetActivityViewModel.is_deleted = false;
                timesheetActivityViewModel.ondate = DateTime.Now.ToString(CultureInfo.CurrentCulture);
                timesheetActivityViewModel.created_date = DateTime.Now.ToString(CultureInfo.CurrentCulture);

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<TimesheetActivityViewModel, TimesheetActivity>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<TimesheetActivity>(timesheetActivityViewModel);

                _unitOfWork.TimesheetActivityRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Timesheet Activity added succefully." }).ConfigureAwait(false);
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

                timesheetActivityViewModel.modified_date = DateTime.Now.ToString(CultureInfo.CurrentCulture);
                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<TimesheetActivityViewModel, TimesheetActivity>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<TimesheetActivity>(timesheetActivityViewModel);
                modal.modified_date = DateTime.Now.ToString(CultureInfo.CurrentCulture);

                _unitOfWork.TimesheetActivityRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Timesheet Activity updated succefully." }).ConfigureAwait(false);
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

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Timesheet Activity removed succefully." }).ConfigureAwait(false);
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

        #endregion TimesheetActivity

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
                timesheetActivityCommentViewModel.ondate = DateTime.Now.ToString(CultureInfo.CurrentCulture);
                timesheetActivityCommentViewModel.created_date = DateTime.Now.ToString(CultureInfo.CurrentCulture);

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<TimesheetActivityCommentViewModel, TimesheetActivityComment>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<TimesheetActivityComment>(timesheetActivityCommentViewModel);

                _unitOfWork.TimesheetActivityCommentRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Comment added succefully." }).ConfigureAwait(false);
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

                timesheetActivityCommentViewModel.modified_date = DateTime.Now.ToString(CultureInfo.CurrentCulture);
                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<TimesheetActivityCommentViewModel, TimesheetActivityComment>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<TimesheetActivityComment>(timesheetActivityCommentViewModel);
                modal.modified_date = DateTime.Now.ToString(CultureInfo.CurrentCulture);

                _unitOfWork.TimesheetActivityCommentRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Comment updated succefully." }).ConfigureAwait(false);
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

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Comment removed succefully." }).ConfigureAwait(false);
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
                timesheetActivityFileViewModel.ondate = DateTime.Now.ToString(CultureInfo.CurrentCulture);
                timesheetActivityFileViewModel.created_date = DateTime.Now.ToString(CultureInfo.CurrentCulture);

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<TimesheetActivityFileViewModel, TimesheetActivityFile>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<TimesheetActivityFile>(timesheetActivityFileViewModel);

                _unitOfWork.TimesheetActivityFileRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Comment added succefully." }).ConfigureAwait(false);
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

                timesheetActivityFileViewModel.modified_date = DateTime.Now.ToString(CultureInfo.CurrentCulture);
                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<TimesheetActivityFileViewModel, TimesheetActivityFile>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<TimesheetActivityFile>(timesheetActivityFileViewModel);
                modal.modified_date = DateTime.Now.ToString(CultureInfo.CurrentCulture);

                _unitOfWork.TimesheetActivityFileRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Comment updated succefully." }).ConfigureAwait(false);
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

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Comment removed succefully." }).ConfigureAwait(false);
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
    }
}