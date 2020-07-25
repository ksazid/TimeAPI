using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using TimeAPI.API.Models;
using TimeAPI.API.Models.MilestoneTemplateViewModels;
using TimeAPI.API.Models.TaskTemplateViewModels;
using TimeAPI.API.Services;
using TimeAPI.Domain;
using TimeAPI.Domain.Entities;

namespace TimeAPI.API.Controllroers
{
    //[ApiKeyAuth]
    //[EnableCors("CorsPolicy")]
    [Route("[controller]")]
    //[Authorize(Roles = "superadmin")]
    public class TemplateController : Controller
    {
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly ApplicationSettings _appSettings;
        private readonly IUnitOfWork _unitOfWork;
        private readonly DateTime _dateTime;

        public TemplateController(IUnitOfWork unitOfWork, ILogger<TemplateController> logger,
            IEmailSender emailSender, IOptions<ApplicationSettings> AppSettings)
        {
            _emailSender = emailSender;
            _logger = logger;
            _appSettings = AppSettings.Value;
            _unitOfWork = unitOfWork;
            _dateTime = InternetTime.GetCurrentTimeFromTimeZone().Value.DateTime;
        }

        [HttpPost]
        [Route("AddMilestoneTemplate")]
        public async Task<object> AddMilestoneTemplate([FromBody] MilestoneTemplateViewModel MilestoneTemplateViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (MilestoneTemplateViewModel == null)
                    throw new ArgumentNullException(nameof(MilestoneTemplateViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<MilestoneTemplateViewModel, MilestoneTemplate>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<MilestoneTemplate>(MilestoneTemplateViewModel);

                modal.id = Guid.NewGuid().ToString();
                modal.created_date = _dateTime.ToString();
                modal.is_deleted = false;

                _unitOfWork.MilestoneTemplateRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "MilestoneTemplate saved successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdateMilestoneTemplate")]
        public async Task<object> UpdateMilestoneTemplate([FromBody] MilestoneTemplateViewModel MilestoneTemplateViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (MilestoneTemplateViewModel == null)
                    throw new ArgumentNullException(nameof(MilestoneTemplateViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<MilestoneTemplateViewModel, MilestoneTemplate>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<MilestoneTemplate>(MilestoneTemplateViewModel);
                modal.modified_date = _dateTime.ToString();

                _unitOfWork.MilestoneTemplateRepository.Update(modal);

                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "MilestoneTemplate updated successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemoveMilestoneTemplateByID")]
        public async Task<object> RemoveMilestoneTemplateByID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                _unitOfWork.MilestoneTemplateRepository.Remove(Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "MilestoneTemplate removed successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetAllMilestoneTemplate")]
        public async Task<object> GetAllMilestoneTemplate(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.MilestoneTemplateRepository.All();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindMilestoneTemplatesByTemplateID")]
        public async Task<object> FindMilestoneTemplatesByTemplateID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = _unitOfWork.MilestoneTemplateRepository.Find(Utils.ID);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindMilestoneTemplatesByOrgID")]
        public async Task<object> FindMilestoneTemplatesByOrgID([FromBody] UtilsOrgID Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.OrgID));

                var result = _unitOfWork.MilestoneTemplateRepository.FindByOrgID(Utils.OrgID);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("AddTaskTemplate")]
        public async Task<object> AddTaskTemplate([FromBody] TaskTemplateViewModel TaskTemplateViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (TaskTemplateViewModel == null)
                    throw new ArgumentNullException(nameof(TaskTemplateViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<TaskTemplateViewModel, TaskTemplate>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<TaskTemplate>(TaskTemplateViewModel);

                modal.id = Guid.NewGuid().ToString();
                modal.created_date = _dateTime.ToString();
                modal.is_deleted = false;

                _unitOfWork.TaskTemplateRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "TaskTemplate saved successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdateTaskTemplate")]
        public async Task<object> UpdateTaskTemplate([FromBody] TaskTemplateViewModel TaskTemplateViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (TaskTemplateViewModel == null)
                    throw new ArgumentNullException(nameof(TaskTemplateViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<TaskTemplateViewModel, TaskTemplate>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<TaskTemplate>(TaskTemplateViewModel);
                modal.modified_date = _dateTime.ToString();

                _unitOfWork.TaskTemplateRepository.Update(modal);

                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "TaskTemplate updated successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemoveTaskTemplateByID")]
        public async Task<object> RemoveTaskTemplateByID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                _unitOfWork.TaskTemplateRepository.Remove(Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "TaskTemplate removed successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetAllTaskTemplate")]
        public async Task<object> GetAllTaskTemplate(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.TaskTemplateRepository.All();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindTaskTemplatesByTemplateID")]
        public async Task<object> FindTaskTemplatesByTemplateID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = _unitOfWork.TaskTemplateRepository.Find(Utils.ID);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindTaskTemplatesByOrgID")]
        public async Task<object> FindTaskTemplatesByOrgID([FromBody] UtilsOrgID Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.OrgID));

                var result = _unitOfWork.TaskTemplateRepository.FindByOrgID(Utils.OrgID);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }
    }
}