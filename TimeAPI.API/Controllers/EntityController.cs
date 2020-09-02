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

                _unitOfWork.EntityMeetingRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Meeting saved successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
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

                var result = _unitOfWork.EntityMeetingRepository.All();

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

                var result = _unitOfWork.EntityMeetingRepository.Find(Utils.ID);


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

                var result = _unitOfWork.EntityMeetingRepository.EntityMeetingByOrgID(Utils.ID);

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

                var result = _unitOfWork.EntityMeetingRepository.EntityMeetingByEntityID(Utils.ID);

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
                        //created_date = item.created_date,
                        //createdby = item.createdby,
                        //modified_date = item.modified_date,
                        //modifiedby = item.modifiedby,
                        //is_deleted = item.is_deleted,

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

                var result = _unitOfWork.EntityNotesRepository.All();

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

                var result = _unitOfWork.EntityNotesRepository.Find(Utils.ID);

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

                var result = _unitOfWork.EntityNotesRepository.EntityNotesByOrgID(Utils.ID);

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

                var result = _unitOfWork.EntityNotesRepository.EntityNotesByEntityID(Utils.ID);

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

                _unitOfWork.EntityCallRepository.Add(modal);
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

                var result = _unitOfWork.EntityCallRepository.All();

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

                var result = _unitOfWork.EntityCallRepository.Find(Utils.ID);

 
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

                var result = _unitOfWork.EntityCallRepository.EntityCallByEntityID(Utils.ID);

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


                var result = _unitOfWork.EntityMeetingRepository.GetAllOpenActivitiesEntityID(Utils.ID);
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


                var result = _unitOfWork.EntityMeetingRepository.GetAllCloseActivitiesEntityID(Utils.ID);
                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

    }
}