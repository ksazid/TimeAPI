using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TimeAPI.API.Models;
using TimeAPI.API.Models.EntityCallViewModels;
using TimeAPI.API.Models.EntityHistoryLogViewModels;
using TimeAPI.API.Models.EntityMeetingViewModels;
using TimeAPI.API.Models.EntityNotesViewModels;
using TimeAPI.API.Services;
using TimeAPI.Domain;
using TimeAPI.Domain.Entities;
using TimeAPI.Domain.Model;

namespace TimeAPI.API.Controllroers
{
    //[ApiKeyAuth]
    //[EnableCors("CorsPolicy")]
    [Route("[controller]")]
    //[Authorize(Roles = "superadmin")]
    public class EntityController : Controller
    {
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly ApplicationSettings _appSettings;
        private readonly IUnitOfWork _unitOfWork;
        private readonly DateTime _dateTime;

        public EntityController(IUnitOfWork unitOfWork, ILogger<EntityController> logger,
            IEmailSender emailSender,
            IOptions<ApplicationSettings> AppSettings)
        {
            _emailSender = emailSender;
            _logger = logger;
            _appSettings = AppSettings.Value;
            _unitOfWork = unitOfWork;
            _dateTime = InternetTime.GetCurrentTimeFromTimeZone().Value.DateTime;
        }

        #region EntityMeeting

        [HttpPost]
        [Route("AddEntityMeeting")]
        public async Task<object> AddEntityMeeting([FromBody] EntityMeetingViewModel socialViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (socialViewModel == null)
                    throw new ArgumentNullException(nameof(socialViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<EntityMeetingViewModel, EntityMeeting>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<EntityMeeting>(socialViewModel);

                modal.id = Guid.NewGuid().ToString();
                modal.created_date = _dateTime.ToString();
                modal.is_deleted = false;

                foreach (var item in socialViewModel.participant_id)
                {
                    EntityMeetingParticipants entityMeetingParticipants = new EntityMeetingParticipants()
                    {
                        id = Guid.NewGuid().ToString(),
                        meeting_id = modal.id,
                        entity_or_emp_id = item,
                        createdby = modal.createdby,
                        created_date = _dateTime.ToString()
                    };
                    _unitOfWork.EntityMeetingParticipantsRepository.Add(entityMeetingParticipants);
                }

                var _status_id = (await _unitOfWork.StatusRepository.GetStatusByOrgID("default").ConfigureAwait(false))
                                     .Where(s => s.status_name.Equals("Completed"))
                                     .Select(s => s.id)
                                     .FirstOrDefault();

                var _TotalMinutes = (Convert.ToDateTime(modal.end_time) - Convert.ToDateTime(modal.start_time)).Ticks;
                TimeSpan elapsedSpan = new TimeSpan(_TotalMinutes);

                string TotalHours;
                string TotalMinutes;
                ConvertHoursAndMinutes(elapsedSpan, out TotalHours, out TotalMinutes);

                var modalTasks = new Domain.Entities.Tasks()
                {
                    id = Guid.NewGuid().ToString(),
                    empid = socialViewModel.emp_id,
                    project_id = modal.entity_id,
                    task_name = modal.meeting_name,
                    status_id = _status_id,
                    assigned_empid = socialViewModel.emp_id,
                    is_local_activity = false,
                    modifiedby = modal.createdby,
                    created_date = _dateTime.ToString(),
                    is_deleted = false
                };

                foreach (var item in socialViewModel.participant_id.Distinct())
                {
                    var TaskTeamMembers = new TaskTeamMember()
                    {
                        id = Guid.NewGuid().ToString(),
                        task_id = modalTasks.id,
                        empid = item,
                        createdby = modal.createdby,
                        created_date = _dateTime.ToString(),
                        is_deleted = false
                    };
                    _unitOfWork.TaskTeamMembersRepository.Add(TaskTeamMembers);
                }
                string _groupid = string.Empty;
                var RecentTimesheet = await _unitOfWork.EntityMeetingRepository.GetRecentTimesheetByEmpID(socialViewModel.emp_id, _dateTime.ToString())
                                                                               .ConfigureAwait(false);

                if (RecentTimesheet == null)
                {
                    return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Please do checkin first for add logs." }).ConfigureAwait(false);
                }
                _groupid = RecentTimesheet;

                TimesheetActivity timesheetActivity = new TimesheetActivity()
                {
                    id = Guid.NewGuid().ToString(),
                    groupid = _groupid,
                    project_id = modal.entity_id,
                    task_id = modalTasks.id,
                    task_name = modal.meeting_name,
                    remarks = modal.desc,
                    ondate = _dateTime.ToString(),
                    start_time = modal.start_time,
                    end_time = modal.end_time,
                    total_hrs = string.Format(@"{0}:{1}", TotalHours, TotalMinutes),
                    is_billable = false,
                };

                _unitOfWork.EntityMeetingRepository.Add(modal);
                _unitOfWork.TaskRepository.Add(modalTasks);
                _unitOfWork.TimesheetActivityRepository.Add(timesheetActivity);

                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Meeting saved successfully." }).ConfigureAwait(false);
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

        [HttpPatch]
        [Route("UpdateEntityMeeting")]
        public async Task<object> UpdateEntityMeeting([FromBody] EntityMeetingViewModel socialViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (socialViewModel == null)
                    throw new ArgumentNullException(nameof(socialViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<EntityMeetingViewModel, EntityMeeting>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<EntityMeeting>(socialViewModel);
                modal.modified_date = _dateTime.ToString();

                _unitOfWork.EntityMeetingParticipantsRepository.Remove(modal.id);

                foreach (var item in socialViewModel.participant_id)
                {
                    EntityMeetingParticipants entityMeetingParticipants = new EntityMeetingParticipants()
                    {
                        id = Guid.NewGuid().ToString(),
                        meeting_id = modal.id,
                        entity_or_emp_id = item,
                        createdby = modal.createdby,
                        created_date = _dateTime.ToString()
                    };
                    _unitOfWork.EntityMeetingParticipantsRepository.Add(entityMeetingParticipants);
                }

                _unitOfWork.EntityMeetingRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Meeting updated successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemoveEntityMeeting")]
        public async Task<object> RemoveEntityMeeting([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                _unitOfWork.EntityMeetingRepository.Remove(Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Meeting removed successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetAllEntityMeeting")]
        public async Task<object> GetAllEntityMeeting(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = await _unitOfWork.EntityMeetingRepository.All().ConfigureAwait(false);
                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindByEntityMeetingID")]
        public async Task<object> FindByEntityMeetingID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = await _unitOfWork.EntityMeetingRepository.Find(Utils.ID).ConfigureAwait(false);
                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<EntityMeeting, EntityMeetingViewModel>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<EntityMeetingViewModel>(result);

                modal.participant_id = _unitOfWork.EntityMeetingParticipantsRepository.EntityMeetingParticipantsByMeetingID(Utils.ID)
                                                                                      .Select(x => x.entity_or_emp_id)
                                                                                      .ToList();

                return await Task.FromResult<object>(modal).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FetchEntityMeetingOrgID")]
        public async Task<object> FetchEntityMeetingOrgID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = await _unitOfWork.EntityMeetingRepository.EntityMeetingByOrgID(Utils.ID).ConfigureAwait(false);
                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FetchEntityMeetingEntityID")]
        public async Task<object> FetchEntityMeetingEntityID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                List<EntityMeetingViewModel> entityMeetingViewModelList = new List<EntityMeetingViewModel>();

                var result = await _unitOfWork.EntityMeetingRepository.EntityMeetingByEntityID(Utils.ID).ConfigureAwait(false);

                foreach (var item in result)
                {
                    string id = item.id;
                    EntityMeetingViewModel entityMeetingViewModel = new EntityMeetingViewModel()
                    {
                        id = item.id,
                        org_id = item.org_id,
                        entity_id = item.entity_id,
                        meeting_name = item.meeting_name,
                        location = item.location,
                        desc = item.desc,
                        start_time = item.start_time,
                        end_time = item.end_time,
                        host = item.host,
                        host_name = item.host_name,
                        participant_id = _unitOfWork.EntityMeetingParticipantsRepository.EntityMeetingParticipantsByMeetingID(id)
                                                                                       .Select(x => x.entity_or_emp_id)
                                                                                       .ToList()
                    };
                    entityMeetingViewModelList.Add(entityMeetingViewModel);
                }

                return await Task.FromResult<object>(entityMeetingViewModelList).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        #endregion EntityMeeting

        #region EntityNotes

        [HttpPost]
        [Route("AddEntityNotes")]
        public async Task<object> AddEntityNotes([FromBody] EntityNotesViewModel socialViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (socialViewModel == null)
                    throw new ArgumentNullException(nameof(socialViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<EntityNotesViewModel, EntityNotes>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<EntityNotes>(socialViewModel);

                modal.id = Guid.NewGuid().ToString();
                modal.created_date = _dateTime.ToString();
                modal.is_deleted = false;

                _unitOfWork.EntityNotesRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Notes saved successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdateEntityNotes")]
        public async Task<object> UpdateEntityNotes([FromBody] EntityNotesViewModel socialViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (socialViewModel == null)
                    throw new ArgumentNullException(nameof(socialViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<EntityNotesViewModel, EntityNotes>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<EntityNotes>(socialViewModel);
                modal.modified_date = _dateTime.ToString();

                _unitOfWork.EntityNotesRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Notes updated successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemoveEntityNotes")]
        public async Task<object> RemoveEntityNotes([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                _unitOfWork.EntityNotesRepository.Remove(Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Notes removed successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetAllEntityNotes")]
        public async Task<object> GetAllEntityNotes(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = await _unitOfWork.EntityNotesRepository.All().ConfigureAwait(false);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindByEntityNotesID")]
        public async Task<object> FindByEntityNotesID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = await _unitOfWork.EntityNotesRepository.Find(Utils.ID).ConfigureAwait(false);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FetchEntityNotesOrgID")]
        public async Task<object> FetchEntityNotesOrgID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = await _unitOfWork.EntityNotesRepository.EntityNotesByOrgID(Utils.ID).ConfigureAwait(false);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FetchEntityNotesEntityID")]
        public async Task<object> FetchEntityNotesEntityID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = await _unitOfWork.EntityNotesRepository.EntityNotesByEntityID(Utils.ID).ConfigureAwait(false);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        #endregion EntityNotes

        #region EntityCall

        [HttpPost]
        [Route("AddEntityCall")]
        public async Task<object> AddEntityCall([FromBody] EntityCallViewModel socialViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (socialViewModel == null)
                    throw new ArgumentNullException(nameof(socialViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<EntityCallViewModel, EntityCall>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<EntityCall>(socialViewModel);

                modal.id = Guid.NewGuid().ToString();
                modal.created_date = _dateTime.ToString();
                modal.is_deleted = false;

                var _status_id = (await _unitOfWork.StatusRepository.GetStatusByOrgID("default").ConfigureAwait(false))
                     .Where(s => s.status_name.Equals("Completed"))
                     .Select(s => s.id)
                     .FirstOrDefault();

                var _TotalMinutes = (Convert.ToDateTime(modal.end_time) - Convert.ToDateTime(modal.start_time)).Ticks;
                TimeSpan elapsedSpan = new TimeSpan(_TotalMinutes);

                string TotalHours;
                string TotalMinutes;
                ConvertHoursAndMinutes(elapsedSpan, out TotalHours, out TotalMinutes);

                var modalTasks = new Domain.Entities.Tasks()
                {
                    id = Guid.NewGuid().ToString(),
                    empid = socialViewModel.emp_id,
                    project_id = modal.entity_id,
                    task_name = modal.subject,
                    status_id = _status_id,
                    assigned_empid = socialViewModel.emp_id,
                    is_local_activity = false,
                    modifiedby = modal.createdby,
                    created_date = _dateTime.ToString(),
                    is_deleted = false
                };

                string _groupid = string.Empty;
                var RecentTimesheet = await _unitOfWork.EntityMeetingRepository.GetRecentTimesheetByEmpID(socialViewModel.emp_id, _dateTime.ToString())
                                                                               .ConfigureAwait(false);

                if (RecentTimesheet == null)
                {
                    return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Please do checkin first for add logs." }).ConfigureAwait(false);
                }
                _groupid = RecentTimesheet;

                TimesheetActivity timesheetActivity = new TimesheetActivity()
                {
                    id = Guid.NewGuid().ToString(),
                    groupid = _groupid,
                    project_id = modal.entity_id,
                    task_id = modalTasks.id,
                    task_name = modal.call_purpose,
                    remarks = modal.call_desc,
                    ondate = _dateTime.ToString(),
                    start_time = modal.start_time,
                    end_time = modal.end_time,
                    total_hrs = string.Format(@"{0}:{1}", TotalHours, TotalMinutes),
                    is_billable = false,
                };

                _unitOfWork.EntityCallRepository.Add(modal);
                _unitOfWork.TaskRepository.Add(modalTasks);
                _unitOfWork.TimesheetActivityRepository.Add(timesheetActivity);

                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Meeting saved successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdateEntityCall")]
        public async Task<object> UpdateEntityCall([FromBody] EntityCallViewModel socialViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (socialViewModel == null)
                    throw new ArgumentNullException(nameof(socialViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<EntityCallViewModel, EntityCall>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<EntityCall>(socialViewModel);
                modal.modified_date = _dateTime.ToString();

                _unitOfWork.EntityCallRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Meeting updated successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemoveEntityCall")]
        public async Task<object> RemoveEntityCall([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                _unitOfWork.EntityCallRepository.Remove(Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Meeting removed successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetAllEntityCall")]
        public async Task<object> GetAllEntityCall(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = await _unitOfWork.EntityCallRepository.All().ConfigureAwait(false);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindByEntityCallID")]
        public async Task<object> FindByEntityCallID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = await _unitOfWork.EntityCallRepository.Find(Utils.ID).ConfigureAwait(false);
                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FetchEntityCallEntityID")]
        public async Task<object> FetchEntityCallEntityID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = await _unitOfWork.EntityCallRepository.EntityCallByEntityID(Utils.ID).ConfigureAwait(false);
                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        #endregion EntityCall

        #region EntityHistoryLog

        [HttpPost]
        [Route("AddEntityHistoryLog")]
        public async Task<object> AddEntityHistoryLog([FromBody] EntityHistoryLogViewModel socialViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (socialViewModel == null)
                    throw new ArgumentNullException(nameof(socialViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<EntityHistoryLogViewModel, EntityHistoryLog>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<EntityHistoryLog>(socialViewModel);

                modal.id = Guid.NewGuid().ToString();
                modal.created_date = _dateTime.ToString();
                modal.is_deleted = false;

                _unitOfWork.EntityHistoryLogRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Notes saved successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdateEntityHistoryLog")]
        public async Task<object> UpdateEntityHistoryLog([FromBody] EntityHistoryLogViewModel socialViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (socialViewModel == null)
                    throw new ArgumentNullException(nameof(socialViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<EntityHistoryLogViewModel, EntityHistoryLog>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<EntityHistoryLog>(socialViewModel);
                modal.modified_date = _dateTime.ToString();

                _unitOfWork.EntityHistoryLogRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Notes updated successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemoveEntityHistoryLog")]
        public async Task<object> RemoveEntityHistoryLog([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                _unitOfWork.EntityHistoryLogRepository.Remove(Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Notes removed successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetAllEntityHistoryLog")]
        public async Task<object> GetAllEntityHistoryLog(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = await _unitOfWork.EntityHistoryLogRepository.All().ConfigureAwait(false);
                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindByEntityHistoryLogID")]
        public async Task<object> FindByEntityHistoryLogID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = await _unitOfWork.EntityHistoryLogRepository.Find(Utils.ID).ConfigureAwait(false);
                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FetchEntityHistoryLogEntityID")]
        public async Task<object> FetchEntityHistoryLogEntityID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = await _unitOfWork.EntityHistoryLogRepository.EntityHistoryLogByEntityID(Utils.ID).ConfigureAwait(false);
                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        #endregion EntityHistoryLog

        [HttpPost]
        [Route("GetAllOpenActivitiesEntityID")]
        public async Task<object> GetAllOpenActivitiesEntityID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = await _unitOfWork.EntityMeetingRepository.GetAllOpenActivitiesEntityID(Utils.ID).ConfigureAwait(false);
                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("GetAllCloseActivitiesEntityID")]
        public async Task<object> GetAllCloseActivitiesEntityID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = await _unitOfWork.EntityMeetingRepository.GetAllCloseActivitiesEntityID(Utils.ID).ConfigureAwait(false);
                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("GetLocalActivitieEntityID")]
        public async Task<object> GetLocalActivitieEntityID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = await _unitOfWork.EntityMeetingRepository.GetLocalActivitieEntityID(Utils.ID).ConfigureAwait(false);
                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

    }
}