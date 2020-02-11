using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TimeAPI.API.Models;
using TimeAPI.API.Models.EntityLocationViewModels;
using TimeAPI.API.Models.ProjectActivityViewModels;
using TimeAPI.API.Models.ProjectViewModels;
using TimeAPI.API.Models.StatusViewModels;
using TimeAPI.API.Models.TaskViewModels;
using TimeAPI.API.Services;
using TimeAPI.Domain;
using TimeAPI.Domain.Entities;

namespace TimeAPI.API.Controllroers
{
    //[ApiKeyAuth]
    //[EnableCors("CorsPolicy")]
    [Route("[controller]")]
    //[Authorize(Roles = "superadmin")]
    public class ProjectController : Controller
    {
        private readonly ApplicationSettings _appSettings;
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly DateTime _dateTime;

        public ProjectController(IUnitOfWork unitOfWork, ILogger<ProjectController> logger,
            IEmailSender emailSender, IOptions<ApplicationSettings> AppSettings)
        {
            _emailSender = emailSender;
            _logger = logger;
            _appSettings = AppSettings.Value;
            _unitOfWork = unitOfWork;
            _dateTime = InternetTime.GetCurrentTimeFromTimeZone().Value.DateTime;
        }

        #region Projects

        [HttpPost]
        [Route("AddProject")]
        public async Task<object> AddProject([FromBody] ProjectViewModel projectViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (projectViewModel == null)
                    throw new ArgumentNullException(nameof(projectViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<ProjectViewModel, Project>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Project>(projectViewModel);

                modal.id = Guid.NewGuid().ToString();

                modal.project_status_id = _unitOfWork.ProjectStatusRepository.GetProjectStatusByOrgID("")
                                            .Where(s => s.project_status_name.Equals("Active"))
                                            .Select(s => s.id)
                                            .FirstOrDefault();

                modal.created_date = _dateTime.ToString();
                modal.is_deleted = false;

                _unitOfWork.ProjectRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Project saved successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FetchAllProjectByOrgID")]
        public async Task<object> FetchAllProjectByOrgID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                oDataTable _oDataTable = new oDataTable();
                var results = _unitOfWork.ProjectRepository.FetchAllProjectByOrgID(Utils.ID);
                var xResult = _oDataTable.ToDataTable(results);

                return await System.Threading.Tasks.Task.FromResult<object>(JsonConvert.SerializeObject(xResult, Formatting.Indented)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindByProjectID")]
        public async Task<object> FindByProjectID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var results = _unitOfWork.ProjectRepository.Find(Utils.ID);

                return await System.Threading.Tasks.Task.FromResult<object>(JsonConvert.SerializeObject(results, Formatting.Indented)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetAllProject")]
        public async Task<object> GetAllProject(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.ProjectRepository.All();
                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemoveProjectByID")]
        public async Task<object> RemoveProjectByID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                _unitOfWork.ProjectRepository.Remove(Utils.ID);
                _unitOfWork.EntityContactRepository.RemoveByEntityID(Utils.ID);
                _unitOfWork.EntityLocationRepository.RemoveByEntityID(Utils.ID);
                _unitOfWork.ProjectActivityTaskRepository.RemoveByProjectID(Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Project removed succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdateProject")]
        public async Task<object> UpdateProject([FromBody] ProjectViewModel projectViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (projectViewModel == null)
                    throw new ArgumentNullException(nameof(projectViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<ProjectViewModel, Project>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Project>(projectViewModel);
                modal.modified_date = _dateTime.ToString();

                _unitOfWork.ProjectRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Project updated succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("UpdateProjectStatusByID")]
        public async Task<object> UpdateProjectStatusByID([FromBody] ProjectStatusModel projectViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (projectViewModel == null)
                    throw new ArgumentNullException(nameof(projectViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<ProjectStatusModel, Project>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Project>(projectViewModel);

                modal.id = Guid.NewGuid().ToString();
                modal.modified_date = _dateTime.ToString();
                modal.is_deleted = false;

                _unitOfWork.ProjectRepository.UpdateProjectStatusByID(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Project Status Updated Successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        #endregion Projects

        #region EntityLocation

        [HttpPost]
        [Route("AddEntityLocation")]
        public async Task<object> AddEntityLocation([FromBody] EntityLocationViewModel projectViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (projectViewModel == null)
                    throw new ArgumentNullException(nameof(projectViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<EntityLocationViewModel, EntityLocation>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<EntityLocation>(projectViewModel);

                modal.id = Guid.NewGuid().ToString();
                modal.created_date = _dateTime.ToString();
                modal.is_deleted = false;

                _unitOfWork.EntityLocationRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Entity Location saved successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindByEntityLocationID")]
        public async Task<object> FindByEntityLocationID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var results = _unitOfWork.EntityLocationRepository.Find(Utils.ID);

                return await System.Threading.Tasks.Task.FromResult<object>(JsonConvert.SerializeObject(results, Formatting.Indented)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetAllEntityLocation")]
        public async Task<object> GetAllEntityLocation(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.EntityLocationRepository.All();
                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemoveEntityLocationByID")]
        public async Task<object> RemoveEntityLocationByID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                _unitOfWork.EntityLocationRepository.Remove(Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Entity Location removed succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdateEntityLocation")]
        public async Task<object> UpdateEntityLocation([FromBody] EntityLocationViewModel projectViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (projectViewModel == null)
                    throw new ArgumentNullException(nameof(projectViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<EntityLocationViewModel, EntityLocation>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<EntityLocation>(projectViewModel);
                modal.modified_date = _dateTime.ToString();

                _unitOfWork.EntityLocationRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Entity Location updated succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        #endregion EntityLocation

        #region EntityContact

        [HttpPost]
        [Route("AddEntityContact")]
        public async Task<object> AddEntityContact([FromBody] EntityContactViewModel projectViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (projectViewModel == null)
                    throw new ArgumentNullException(nameof(projectViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<EntityContactViewModel, EntityContact>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<EntityContact>(projectViewModel);

                modal.id = Guid.NewGuid().ToString();
                modal.created_date = _dateTime.ToString();
                modal.is_deleted = false;

                _unitOfWork.EntityContactRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Entity Location saved successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindByEntityContactID")]
        public async Task<object> FindByEntityContactID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var results = _unitOfWork.EntityContactRepository.Find(Utils.ID);

                return await System.Threading.Tasks.Task.FromResult<object>(JsonConvert.SerializeObject(results, Formatting.Indented)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetAllEntityContact")]
        public async Task<object> GetAllEntityContact(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.EntityContactRepository.All();
                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemoveEntityContactByID")]
        public async Task<object> RemoveEntityContactByID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                _unitOfWork.EntityContactRepository.Remove(Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Entity Location removed succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdateEntityContact")]
        public async Task<object> UpdateEntityContact([FromBody] EntityContactViewModel projectViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (projectViewModel == null)
                    throw new ArgumentNullException(nameof(projectViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<EntityContactViewModel, EntityContact>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<EntityContact>(projectViewModel);
                modal.modified_date = _dateTime.ToString();

                _unitOfWork.EntityContactRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Entity Location updated succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        #endregion EntityContact

        #region ProjectStatus

        [HttpPost]
        [Route("AddProjectStatus")]
        public async Task<object> AddProjectStatus([FromBody] ProjectStatusViewModel statusingViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (statusingViewModel == null)
                    throw new ArgumentNullException(nameof(statusingViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<ProjectStatusViewModel, ProjectStatus>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<ProjectStatus>(statusingViewModel);

                modal.id = Guid.NewGuid().ToString();
                modal.created_date = _dateTime.ToString();
                modal.is_deleted = false;

                _unitOfWork.ProjectStatusRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "ProjectStatus registered succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindByProjectStatusID")]
        public async Task<object> FindByProjectStatusID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = _unitOfWork.ProjectStatusRepository.Find(Utils.ID);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetAllProjectStatus")]
        public async Task<object> GetAllProjectStatus(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.ProjectStatusRepository.All();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("GetProjectStatusByOrgID")]
        public async Task<object> GetProjectStatusByOrgID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = _unitOfWork.ProjectStatusRepository.GetProjectStatusByOrgID(Utils.ID);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemoveProjectStatus")]
        public async Task<object> RemoveProjectStatus([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                _unitOfWork.ProjectStatusRepository.Remove(Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "ProjectStatus removed succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdateProjectStatus")]
        public async Task<object> UpdateProjectStatus([FromBody] ProjectStatusViewModel statusingViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (statusingViewModel == null)
                    throw new ArgumentNullException(nameof(statusingViewModel));

                statusingViewModel.modified_date = _dateTime.ToString();
                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<ProjectStatusViewModel, ProjectStatus>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<ProjectStatus>(statusingViewModel);

                modal.modified_date = _dateTime.ToString();

                _unitOfWork.ProjectStatusRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "ProjectStatus updated succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        #endregion ProjectStatus

        #region ProjectActivity

        [HttpPost]
        [Route("AddProjectActivity")]
        public async Task<object> AddProjectActivity([FromBody] ProjectActivityViewModel statusingViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (statusingViewModel == null)
                    throw new ArgumentNullException(nameof(statusingViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<ProjectActivityViewModel, ProjectActivity>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<ProjectActivity>(statusingViewModel);

                modal.id = Guid.NewGuid().ToString();
                modal.created_date = _dateTime.ToString();
                modal.is_deleted = false;

                _unitOfWork.ProjectActivityRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Project Activity registered succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindByProjectActivityID")]
        public async Task<object> FindByProjectActivityID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = _unitOfWork.ProjectActivityRepository.Find(Utils.ID);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetAllProjectActivity")]
        public async Task<object> GetAllProjectActivity(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.ProjectActivityRepository.All();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("GetProjectActivityByProjectID")]
        public async Task<object> GetProjectActivityByProjectID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = _unitOfWork.ProjectActivityRepository.GetProjectActivityByProjectID(Utils.ID);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemoveProjectActivity")]
        public async Task<object> RemoveProjectActivity([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                _unitOfWork.ProjectActivityRepository.Remove(Utils.ID);
                _unitOfWork.ProjectActivityTaskRepository.RemoveByProjectActivityID(Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Project Activity Removed succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdateProjectActivity")]
        public async Task<object> UpdateProjectActivity([FromBody] ProjectActivityViewModel statusingViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (statusingViewModel == null)
                    throw new ArgumentNullException(nameof(statusingViewModel));

                statusingViewModel.modified_date = _dateTime.ToString();
                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<ProjectActivityViewModel, ProjectActivity>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<ProjectActivity>(statusingViewModel);

                modal.modified_date = _dateTime.ToString();

                _unitOfWork.ProjectActivityRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Project Activity updated succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdateProjectActivityStatusByActivityID")]
        public async Task<object> UpdateProjectActivityStatusByActivityID([FromBody] ProjectActivityStatusUpdateViewModel statusingViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (statusingViewModel == null)
                    throw new ArgumentNullException(nameof(statusingViewModel));

                statusingViewModel.modified_date = _dateTime.ToString();
                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<ProjectActivityViewModel, ProjectActivity>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<ProjectActivity>(statusingViewModel);

                modal.modified_date = _dateTime.ToString();

                _unitOfWork.ProjectActivityRepository.UpdateProjectActivityStatusByActivityID(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Project Activity updated succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        #endregion ProjectActivity

        #region ProjectActivityTask

        [HttpPost]
        [Route("AddTask")]
        public async Task<object> AddTask([FromBody]  ProjectActivityTaskViewModel TaskViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (TaskViewModel == null)
                    throw new ArgumentNullException(nameof(TaskViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<ProjectActivityTaskViewModel, Domain.Entities.Tasks>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Domain.Entities.Tasks>(TaskViewModel);

                modal.id = Guid.NewGuid().ToString();
                modal.status_id = _unitOfWork.StatusRepository.GetStatusByOrgID("")
                            .Where(s => s.status_name.Equals("Open"))
                            .Select(s => s.id)
                            .FirstOrDefault();
                modal.created_date = _dateTime.ToString();
                modal.is_deleted = false;

                if (TaskViewModel.employees != null)
                {
                    foreach (var item in TaskViewModel.employees.empid.Distinct())
                    {
                        modal.id = Guid.NewGuid().ToString();
                        modal.created_date = _dateTime.ToString();
                        modal.is_deleted = false;

                        var TaskTeamMembers = new TaskTeamMember()
                        {
                            id = Guid.NewGuid().ToString(),
                            task_id = modal.id,
                            empid = item,
                            createdby = modal.createdby,
                            created_date = _dateTime.ToString(),
                            is_deleted = false
                        };
                        _unitOfWork.TaskTeamMembersRepository.Add(TaskTeamMembers);
                    }
                }

                var ProjectTask = new ProjectActivityTask()
                {
                    id = Guid.NewGuid().ToString(),
                    project_id = TaskViewModel.project_id,
                    activity_id = TaskViewModel.activtity_id,
                    task_id = modal.id,
                    created_date = _dateTime.ToString(),
                    createdby = TaskViewModel.createdby,
                    is_deleted = false
                };

                _unitOfWork.TaskRepository.Add(modal);
                _unitOfWork.ProjectActivityTaskRepository.Add(ProjectTask);
                _unitOfWork.Commit();

                return await System.Threading.Tasks.Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Task registered succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return System.Threading.Tasks.Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindByTasksId")]
        public async Task<object> FindByTasksID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var results = _unitOfWork.TaskRepository.Find(Utils.ID);

                return await System.Threading.Tasks.Task.FromResult<object>(results).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return System.Threading.Tasks.Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetAllTasks")]
        public async Task<object> GetAllTasks(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.TaskRepository.All();

                return await System.Threading.Tasks.Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return System.Threading.Tasks.Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemoveTask")]
        public async Task<object> RemoveTask([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                _unitOfWork.TaskRepository.Remove(Utils.ID);
                _unitOfWork.ProjectActivityTaskRepository.Remove(Utils.ID);
                _unitOfWork.Commit();

                return await System.Threading.Tasks.Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Task removed succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return System.Threading.Tasks.Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdateTask")]
        public async Task<object> UpdateAddTask([FromBody] ProjectActivityTaskViewModel TaskViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (TaskViewModel == null)
                    throw new ArgumentNullException(nameof(TaskViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<ProjectActivityTaskViewModel, Domain.Entities.Tasks>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Domain.Entities.Tasks>(TaskViewModel);
                modal.modified_date = _dateTime.ToString();

                _unitOfWork.TaskTeamMembersRepository.RemoveByTaskID(modal.id);

                if (TaskViewModel.employees != null)
                {
                    foreach (var item in TaskViewModel.employees.empid.Distinct())
                    {
                        modal.id = Guid.NewGuid().ToString();
                        modal.created_date = _dateTime.ToString();
                        modal.is_deleted = false;

                        var TaskTeamMembers = new TaskTeamMember()
                        {
                            id = Guid.NewGuid().ToString(),
                            task_id = modal.id,
                            empid = item,
                            createdby = modal.createdby,
                            created_date = _dateTime.ToString(),
                            is_deleted = false
                        };
                        _unitOfWork.TaskTeamMembersRepository.Add(TaskTeamMembers);
                    }
                }

                _unitOfWork.TaskRepository.Update(modal);
                _unitOfWork.Commit();

                return await System.Threading.Tasks.Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Task updated succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return System.Threading.Tasks.Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("GetAllTaskByActivityID")]
        public async Task<object> GetAllTaskByActivityID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                oDataTable _oDataTable = new oDataTable();
                var results = _unitOfWork.ProjectActivityTaskRepository.GetAllTaskByActivityID(Utils.ID);
                var xResult = _oDataTable.ToDataTable(results);

                return await System.Threading.Tasks.Task.FromResult<object>(JsonConvert.SerializeObject(xResult, Formatting.Indented)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        #endregion ProjectActivityTask
    }
}