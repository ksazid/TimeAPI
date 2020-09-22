using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nancy.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TimeAPI.API.Models;
using TimeAPI.API.Models.EntityLocationViewModels;
using TimeAPI.API.Models.ProjectActivityViewModels;
using TimeAPI.API.Models.ProjectTagsViewModels;
using TimeAPI.API.Models.ProjectTypeViewModels;
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
                string project_id = modal.id = Guid.NewGuid().ToString();

                modal.project_status_id = _unitOfWork.ProjectStatusRepository.GetProjectStatusByOrgID("default")
                                            .Where(s => s.project_status_name.Equals("Open"))
                                            .Select(s => s.id)
                                            .FirstOrDefault();

                modal.created_date = _dateTime.ToString();

                modal.is_deleted = false;

                if (projectViewModel.EntityContact != null)
                {
                    for (int i = 0; i < projectViewModel.EntityContact.Count; i++)
                    {
                        var entityContact = new EntityContact()
                        {
                            id = Guid.NewGuid().ToString(),
                            entity_id = modal.id,
                            name = projectViewModel.EntityContact[i].name,
                            position = projectViewModel.EntityContact[i].position,
                            phone = projectViewModel.EntityContact[i].phone,
                            mobile = projectViewModel.EntityContact[i].mobile,
                            email = projectViewModel.EntityContact[i].email,
                            createdby = projectViewModel.createdby,
                            created_date = _dateTime.ToString(),
                            is_primary = projectViewModel.EntityContact[i].is_primary,
                            is_deleted = false
                        };
                        _unitOfWork.EntityContactRepository.Add(entityContact);
                    }
                }

                if (projectViewModel.EntityLocation != null)
                {
                    var entityLocation = new EntityLocation()
                    {
                        id = Guid.NewGuid().ToString(),
                        entity_id = modal.id,
                        geo_address = projectViewModel.EntityLocation.geo_address,
                        formatted_address = projectViewModel.EntityLocation.formatted_address,
                        lat = projectViewModel.EntityLocation.lat,
                        lang = projectViewModel.EntityLocation.lang,
                        street_number = projectViewModel.EntityLocation.street_number,
                        route = projectViewModel.EntityLocation.route,
                        locality = projectViewModel.EntityLocation.locality,
                        administrative_area_level_2 = projectViewModel.EntityLocation.administrative_area_level_2,
                        administrative_area_level_1 = projectViewModel.EntityLocation.administrative_area_level_1,
                        postal_code = projectViewModel.EntityLocation.postal_code,
                        country = projectViewModel.EntityLocation.country,
                        createdby = projectViewModel.createdby,
                        created_date = _dateTime.ToString(),
                        is_deleted = false
                    };
                    _unitOfWork.EntityLocationRepository.Add(entityLocation);
                }

                if (projectViewModel.cst_id != null)
                {
                    var customerProject = new CustomerProject()
                    {
                        id = Guid.NewGuid().ToString(),
                        cst_id = projectViewModel.cst_id,
                        project_id = modal.id,
                        createdby = projectViewModel.createdby,
                        created_date = _dateTime.ToString(),
                        is_deleted = false
                    };
                    _unitOfWork.CustomerProjectRepository.Add(customerProject);
                }

                _unitOfWork.ProjectRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = project_id, Desc = "Project saved successfully." }).ConfigureAwait(false);
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
                var results = await _unitOfWork.ProjectRepository.FetchAllProjectByOrgID(Utils.ID).ConfigureAwait(false);
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
                ProjectDetailViewModel projectViewModel = new ProjectDetailViewModel();

                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var results = await _unitOfWork.ProjectRepository.Find(Utils.ID).ConfigureAwait(false);
                var resultLocation = _unitOfWork.EntityLocationRepository.FindByEnitiyID(results.id);
                var resultContact = await _unitOfWork.EntityContactRepository.FindByEntityListID(results.id).ConfigureAwait(false);
                var resultCustomer = await _unitOfWork.CustomerRepository.FindCustomerByProjectID(results.id).ConfigureAwait(false);

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<ProjectDetailViewModel, Project>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Project>(results);

                modal.EntityLocation = resultLocation;
                modal.EntityContact = resultContact.ToList();
                modal.EntityCustomer = resultCustomer;

                return await Task.FromResult<object>(modal).ConfigureAwait(false);
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

                var result = await _unitOfWork.ProjectRepository.All().ConfigureAwait(false);
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
                await _unitOfWork.EntityContactRepository.RemoveByEntityID(Utils.ID).ConfigureAwait(false);
                _unitOfWork.EntityLocationRepository.RemoveByEntityID(Utils.ID);
                await _unitOfWork.ProjectActivityTaskRepository.RemoveByProjectID(Utils.ID).ConfigureAwait(false);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Project removed successfully." }).ConfigureAwait(false);
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

                //if (projectViewModel.EntityContact != null)
                //{
                //    var entityContact = new EntityContact()
                //    {
                //        entity_id = modal.id,
                //        name = projectViewModel.EntityContact.name,
                //        position = projectViewModel.EntityContact.position,
                //        phone = projectViewModel.EntityContact.phone,
                //        mobile = projectViewModel.EntityContact.mobile,
                //        email = projectViewModel.EntityContact.email,
                //        modifiedby = projectViewModel.createdby,
                //        modified_date = _dateTime.ToString(),
                //        is_primary = projectViewModel.EntityContact.is_primary
                //    };
                //    _unitOfWork.EntityContactRepository.Update(entityContact);
                //}
                await _unitOfWork.EntityContactRepository.RemoveByEntityID(modal.id).ConfigureAwait(false);
                if (projectViewModel.EntityContact != null)
                {
                    for (int i = 0; i < projectViewModel.EntityContact.Count; i++)
                    {
                        var entityContact = new EntityContact()
                        {
                            id = Guid.NewGuid().ToString(),
                            entity_id = modal.id,
                            name = projectViewModel.EntityContact[i].name,
                            position = projectViewModel.EntityContact[i].position,
                            phone = projectViewModel.EntityContact[i].phone,
                            mobile = projectViewModel.EntityContact[i].mobile,
                            email = projectViewModel.EntityContact[i].email,
                            createdby = projectViewModel.createdby,
                            created_date = _dateTime.ToString(),
                            is_primary = projectViewModel.EntityContact[i].is_primary,
                            is_deleted = false
                        };
                        _unitOfWork.EntityContactRepository.Add(entityContact);
                    }
                }

                if (projectViewModel.EntityLocation != null)
                {
                    var entityLocation = new EntityLocation()
                    {
                        entity_id = modal.id,
                        geo_address = projectViewModel.EntityLocation.id,
                        formatted_address = projectViewModel.EntityLocation.formatted_address,
                        lat = projectViewModel.EntityLocation.lat,
                        lang = projectViewModel.EntityLocation.lang,
                        street_number = projectViewModel.EntityLocation.street_number,
                        route = projectViewModel.EntityLocation.route,
                        locality = projectViewModel.EntityLocation.locality,
                        administrative_area_level_2 = projectViewModel.EntityLocation.administrative_area_level_2,
                        administrative_area_level_1 = projectViewModel.EntityLocation.administrative_area_level_1,
                        postal_code = projectViewModel.EntityLocation.postal_code,
                        country = projectViewModel.EntityLocation.country,
                        modifiedby = projectViewModel.createdby,
                        modified_date = _dateTime.ToString()
                    };
                    _unitOfWork.EntityLocationRepository.Update(entityLocation);
                }

                if (projectViewModel.cst_id != null)
                {
                    var customerProject = new CustomerProject()
                    {
                        cst_id = projectViewModel.cst_id,
                        project_id = modal.id,
                        createdby = projectViewModel.createdby,
                        created_date = _dateTime.ToString(),
                        is_deleted = false
                    };
                    _unitOfWork.CustomerProjectRepository.Update(customerProject);
                }

                _unitOfWork.ProjectRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Project updated successfully." }).ConfigureAwait(false);
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

                await _unitOfWork.ProjectRepository.UpdateProjectStatusByID(modal).ConfigureAwait(false);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Project Status Updated Successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindAutoProjectPrefixByOrgID")]
        public async Task<object> FindAutoProjectPrefixByOrgID([FromBody] UtilsOrgID Utils, CancellationToken cancellationToken)
        {
            try
            {
                ProjectViewModel projectViewModel = new ProjectViewModel();

                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.OrgID));

                var results = await _unitOfWork.ProjectRepository.FindAutoProjectPrefixByOrgID(Utils.OrgID, _dateTime.ToString()).ConfigureAwait(false);

                return await Task.FromResult<object>(results).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindAutoCostProjectPrefixByOrgID")]
        public async Task<object> FindAutoCostProjectPrefixByOrgID([FromBody] UtilsOrgID Utils, CancellationToken cancellationToken)
        {
            try
            {
                ProjectViewModel projectViewModel = new ProjectViewModel();

                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.OrgID));

                var results = await _unitOfWork.ProjectRepository.FindAutoCostProjectPrefixByOrgID(Utils.OrgID, _dateTime.ToString()).ConfigureAwait(false);

                return await Task.FromResult<object>(results).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindCustomProjectPrefixByOrgIDAndPrefix")]
        public async Task<object> FindCustomProjectPrefixByOrgIDAndPrefix([FromBody] UtilsOrgIDAndPrefix Utils, CancellationToken cancellationToken)
        {
            try
            {
                ProjectViewModel projectViewModel = new ProjectViewModel();

                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.OrgID));

                var results = _unitOfWork.ProjectRepository.FindCustomProjectPrefixByOrgIDAndPrefix(Utils.OrgID, Utils.Prefix);

                return await Task.FromResult<object>(results).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("AddProjectCustomer")]
        public async Task<object> AddProjectCustomer([FromBody] ProjectCustomerViewModel statusingViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (statusingViewModel == null)
                    throw new ArgumentNullException(nameof(statusingViewModel));

                var CustomerProject = await _unitOfWork.CustomerProjectRepository.Find(statusingViewModel.project_id).ConfigureAwait(false);

                if (CustomerProject == null)
                {
                    var modal = new CustomerProject()
                    {
                        id = Guid.NewGuid().ToString(),
                        project_id = statusingViewModel.project_id,
                        cst_id = statusingViewModel.cst_id,
                        createdby = statusingViewModel.createdby,
                        created_date = _dateTime.ToString(),
                        is_deleted = false,
                    };

                    _unitOfWork.CustomerProjectRepository.Add(modal);
                }
                else if (CustomerProject != null)
                {
                    var modal = new CustomerProject()
                    {
                        project_id = statusingViewModel.project_id,
                        cst_id = statusingViewModel.cst_id,
                        modifiedby = statusingViewModel.createdby,
                        created_date = _dateTime.ToString(),
                        is_deleted = false,
                    };

                    _unitOfWork.CustomerProjectRepository.Update(modal);
                }
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Project registered successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("ProjectTaskCountByProjectID")]
        public async Task<object> ProjectTaskCountByProjectID([FromBody] Utils _Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (_Utils == null)
                    throw new ArgumentNullException(nameof(_Utils));

                var results = await _unitOfWork.ProjectRepository.ProjectTaskCount(_Utils.ID).ConfigureAwait(false);
                return await Task.FromResult<object>(results).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindAllProjectActivityByProjectID")]
        public async Task<object> FindAllProjectActivityByProjectID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                oDataTable _oDataTable = new oDataTable();
                var results = await _unitOfWork.ProjectRepository.FindAllProjectActivityByProjectID(Utils.ID).ConfigureAwait(false);
                var xResult = _oDataTable.ToDataTable(results);

                return await System.Threading.Tasks.Task.FromResult<object>(JsonConvert.SerializeObject(xResult, Formatting.Indented)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("GetLastAddedProjectPrefixByOrgID")]
        public async Task<object> GetLastAddedProjectPrefixByOrgID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = await _unitOfWork.ProjectRepository.GetLastAddedProjectPrefixByOrgID(Utils.ID).ConfigureAwait(false);
                return await Task.FromResult<object>(new SuccessViewModel
                {
                    Status = "200",
                    Code = (result ?? string.Empty).ToString(),
                    Desc = ""
                }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("GetAllSubTaskByTaskID")]
        public async Task<object> GetAllSubTaskByTaskID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                List<ProjectSubTaskEntityViewModel> projectActivityTasks = new List<ProjectSubTaskEntityViewModel>();

                projectActivityTasks = (await _unitOfWork.ProjectActivityTaskRepository.GetAllSubTaskByTaskID(Utils.ID).ConfigureAwait(false)).ToList();

                foreach (var item in projectActivityTasks.Select((value, index) => new { value, index }))
                    projectActivityTasks[item.index].TaskTeamMember = (await _unitOfWork.TaskTeamMembersRepository.FindByTaskID(item.value.id).ConfigureAwait(false)).ToList();

                return await System.Threading.Tasks.Task.FromResult<object>(JsonConvert.SerializeObject(projectActivityTasks, Formatting.Indented)).ConfigureAwait(false);
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

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Entity Location removed successfully." }).ConfigureAwait(false);
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

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Entity Location updated successfully." }).ConfigureAwait(false);
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

                string entity_cont_id = modal.id = Guid.NewGuid().ToString();
                modal.created_date = _dateTime.ToString();
                modal.is_deleted = false;

                _unitOfWork.EntityContactRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = entity_cont_id, Desc = "Entity Location saved successfully." }).ConfigureAwait(false);
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


        [HttpPost]
        [Route("FindByEntityContactOrgID")]
        public async Task<object> FindByEntityContactOrgID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var results = await _unitOfWork.EntityContactRepository.FindByEntityContactOrgID(Utils.ID).ConfigureAwait(false);

                return await System.Threading.Tasks.Task.FromResult<object>(JsonConvert.SerializeObject(results, Formatting.Indented)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindEntityContactByEntityID")]
        public async Task<object> FindEntityContactByEntityID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var results = await _unitOfWork.EntityContactRepository.FindByEntityListID(Utils.ID).ConfigureAwait(false);

                return await System.Threading.Tasks.Task.FromResult<object>(JsonConvert.SerializeObject(results, Formatting.Indented)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }


        [HttpPost]
        [Route("GetAllEntityContactByEntityIDAndCstID")]
        public async Task<object> GetAllEntityContactByEntityIDAndCstID([FromBody] UtilsEntityAndCstID Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.EntityID));

                var results = (await _unitOfWork.EntityContactRepository.GetAllEntityContactByEntityIDAndCstID(Utils.EntityID, Utils.CstID)
                                                                        .ConfigureAwait(false)).DistinctBy(x => new { x.name, x.email });

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

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Entity Location removed successfully." }).ConfigureAwait(false);
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

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Entity Location updated successfully." }).ConfigureAwait(false);
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

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "ProjectStatus registered successfully." }).ConfigureAwait(false);
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

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "ProjectStatus removed successfully." }).ConfigureAwait(false);
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

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "ProjectStatus updated successfully." }).ConfigureAwait(false);
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

                modal.status_id = _unitOfWork.ProjectStatusRepository.GetProjectStatusByOrgID("default")
                                      .Where(s => s.project_status_name.Equals("Open"))
                                      .Select(s => s.id)
                                      .FirstOrDefault();

                _unitOfWork.ProjectActivityRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Project Activity registered successfully." }).ConfigureAwait(false);
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

                var result = await _unitOfWork.ProjectActivityRepository.FindByProjectActivityID(Utils.ID).ConfigureAwait(false);

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

                var result = await _unitOfWork.ProjectActivityRepository.All().ConfigureAwait(false);

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

                var result = await _unitOfWork.ProjectActivityRepository.GetProjectActivityByProjectID(Utils.ID).ConfigureAwait(false);

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
                await _unitOfWork.ProjectActivityTaskRepository.RemoveByProjectActivityID(Utils.ID).ConfigureAwait(false);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Project Activity Removed successfully." }).ConfigureAwait(false);
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

                var status = await _unitOfWork.StatusRepository.Find(statusingViewModel.status_id).ConfigureAwait(false);
                if (status != null)
                    if (status.status_name == "Completed")
                    {
                        var Result = await _unitOfWork.ProjectActivityTaskRepository.GetAllTaskByActivityID(modal.id).ConfigureAwait(false);
                        if (Result != null)
                        {
                            return await Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = "Error", Desc = "There are still task for this milestone is pending." }).ConfigureAwait(false);
                        }
                    }

                _unitOfWork.ProjectActivityRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Project Activity updated successfully." }).ConfigureAwait(false);
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

                await _unitOfWork.ProjectActivityRepository.UpdateProjectActivityStatusByActivityID(modal).ConfigureAwait(false);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Project Activity updated successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("GetProjectActivityRatioByProjectID")]
        public async Task<object> GetProjectActivityRatioByProjectID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = await _unitOfWork.ProjectActivityRepository.GetProjectActivityRatioByProjectID(Utils.ID).ConfigureAwait(false);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
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
                modal.status_id = (await _unitOfWork.StatusRepository.GetStatusByOrgID("default").ConfigureAwait(false))
                                    .Where(s => s.status_name.Equals("Open"))
                                    .Select(s => s.id)
                                    .FirstOrDefault();

                modal.created_date = _dateTime.ToString();
                modal.is_deleted = false;

                if (TaskViewModel.employees != null)
                {
                    foreach (var item in TaskViewModel.employees.Distinct())
                    {
                        var TaskTeamMembers = new TaskTeamMember()
                        {
                            id = Guid.NewGuid().ToString(),
                            task_id = modal.id,
                            empid = item.empid,
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

                return await System.Threading.Tasks.Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Task registered successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return System.Threading.Tasks.Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("AddMultipleTask")]
        public async Task<object> AddMultipleTask([FromBody] List<ProjectActivityTaskViewModel> TaskViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (TaskViewModel == null)
                    throw new ArgumentNullException(nameof(TaskViewModel));

                for (int i = 0; i < TaskViewModel.Count; i++)
                {
                    var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<ProjectActivityTaskViewModel, Domain.Entities.Tasks>());
                    var mapper = config.CreateMapper();
                    var modal = mapper.Map<Domain.Entities.Tasks>(TaskViewModel[i]);

                    modal.id = Guid.NewGuid().ToString();
                    modal.status_id = (await _unitOfWork.StatusRepository.GetStatusByOrgID("default").ConfigureAwait(false))
                                        .Where(s => s.status_name.Equals("Open"))
                                        .Select(s => s.id)
                                        .FirstOrDefault();

                    modal.created_date = _dateTime.ToString();
                    modal.is_deleted = false;

                    if (TaskViewModel[i].employees != null)
                    {
                        foreach (var item in TaskViewModel[i].employees.Distinct())
                        {
                            var TaskTeamMembers = new TaskTeamMember()
                            {
                                id = Guid.NewGuid().ToString(),
                                task_id = modal.id,
                                empid = item.empid,
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
                        project_id = TaskViewModel[i].project_id,
                        activity_id = TaskViewModel[i].activtity_id,
                        task_id = modal.id,
                        created_date = _dateTime.ToString(),
                        createdby = TaskViewModel[i].createdby,
                        is_deleted = false
                    };

                    _unitOfWork.TaskRepository.Add(modal);
                    _unitOfWork.ProjectActivityTaskRepository.Add(ProjectTask);
                }

                _unitOfWork.Commit();

                return await System.Threading.Tasks.Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Task registered successfully." }).ConfigureAwait(false);
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

                var results = await _unitOfWork.TaskRepository.Find(Utils.ID).ConfigureAwait(false);
                results.employees = (await _unitOfWork.TaskTeamMembersRepository.FindByTaskID(Utils.ID).ConfigureAwait(false)).ToList();

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

                return await System.Threading.Tasks.Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Task removed successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return System.Threading.Tasks.Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdateTask")]
        public async Task<object> UpdateTask([FromBody] ProjectActivityTaskViewModel TaskViewModel, CancellationToken cancellationToken)
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

                await _unitOfWork.TaskTeamMembersRepository.RemoveByTaskID(modal.id).ConfigureAwait(false);

                if (TaskViewModel.employees != null)
                {
                    foreach (var item in TaskViewModel.employees.Distinct())
                    {
                        var TaskTeamMembers = new TaskTeamMember()
                        {
                            id = Guid.NewGuid().ToString(),
                            task_id = modal.id,
                            empid = item.empid,
                            createdby = modal.createdby,
                            created_date = _dateTime.ToString(),
                            is_deleted = false
                        };
                        _unitOfWork.TaskTeamMembersRepository.Add(TaskTeamMembers);
                    }
                }

                _unitOfWork.TaskRepository.Update(modal);
                _unitOfWork.Commit();

                return await System.Threading.Tasks.Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Task updated successfully." }).ConfigureAwait(false);
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
                var results = await _unitOfWork.ProjectActivityTaskRepository.GetAllTaskByActivityID(Utils.ID).ConfigureAwait(false);
                var xResult = _oDataTable.ToDataTable(results);

                return await System.Threading.Tasks.Task.FromResult<object>(JsonConvert.SerializeObject(xResult, Formatting.Indented)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("GetAllTaskByProjectID")]
        public async Task<object> GetAllTaskByProjectID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                oDataTable _oDataTable = new oDataTable();
                var results = await _unitOfWork.ProjectActivityTaskRepository.GetAllTaskByProjectID(Utils.ID).ConfigureAwait(false);
                var xResult = _oDataTable.ToDataTable(results);

                return await System.Threading.Tasks.Task.FromResult<object>(JsonConvert.SerializeObject(xResult, Formatting.Indented)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("GetProjectActivityTaskRatioByProjectID")]
        public async Task<object> GetProjectActivityTaskRatioByProjectID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var results = await _unitOfWork.ProjectActivityTaskRepository.GetProjectActivityTaskRatioByProjectID(Utils.ID).ConfigureAwait(false);
                return await Task.FromResult<object>(results).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("AssignEmpployeeToTask")]
        public async Task<object> AssignEmpployeeToTask([FromBody] AssignEmployeeTaskViewModel TaskViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (TaskViewModel == null)
                    throw new ArgumentNullException(nameof(TaskViewModel));

                bool? checkIfAssigned = null;

                var task = await _unitOfWork.TaskRepository.Find(TaskViewModel.id).ConfigureAwait(false);
                if (task != null)
                {
                    int count = 0;
                    var sub_task = (await _unitOfWork.SubTaskRepository.FindSubTaskByTaskID(task.id).ConfigureAwait(false)).ToList();
                    for (int i = 0; i < sub_task.Count; i++)
                    {
                        //await _unitOfWork.TaskTeamMembersRepository.RemoveByTaskID(sub_task[i].id).ConfigureAwait(false);
                        var value = await _unitOfWork.TaskTeamMembersRepository.FindByTaskID(sub_task[i].id).ConfigureAwait(false);
                        if (!value.Any())
                        {
                            if (TaskViewModel.empid != null)
                            {
                                var TaskTeamMembers = new TaskTeamMember()
                                {
                                    id = Guid.NewGuid().ToString(),
                                    task_id = sub_task[i].id,
                                    empid = TaskViewModel.empid,
                                    createdby = TaskViewModel.createdby,
                                    created_date = _dateTime.ToString(),
                                    is_deleted = false
                                };

                                var Subtask = new SubTasks()
                                {
                                    id = sub_task[i].id,
                                    lead_id = TaskViewModel.empid,
                                    modifiedby = TaskViewModel.createdby,
                                    modified_date = _dateTime.ToString()
                                };
                                await _unitOfWork.SubTaskRepository.UpdateSubTaskLeadBySubTaskID(Subtask).ConfigureAwait(false);
                                _unitOfWork.TaskTeamMembersRepository.Add(TaskTeamMembers);
                                checkIfAssigned = true;
                            }
                        }
                        else
                        {
                            count++;
                            if (count == sub_task.Count)
                            {
                                checkIfAssigned = false;
                            }

                        }
                    }
                }
                else
                {
                    await _unitOfWork.TaskTeamMembersRepository.RemoveByTaskID(TaskViewModel.id).ConfigureAwait(false);
                    if (TaskViewModel.empid != null)
                    {
                        var TaskTeamMembers = new TaskTeamMember()
                        {
                            id = Guid.NewGuid().ToString(),
                            task_id = TaskViewModel.id,
                            empid = TaskViewModel.empid,
                            createdby = TaskViewModel.createdby,
                            created_date = _dateTime.ToString(),
                            is_deleted = false
                        };
                        var Subtask = new SubTasks()
                        {
                            id = TaskViewModel.id,
                            lead_id = TaskViewModel.empid,
                            modifiedby = TaskViewModel.createdby,
                            modified_date = _dateTime.ToString()
                        };
                        await _unitOfWork.SubTaskRepository.UpdateSubTaskLeadBySubTaskID(Subtask).ConfigureAwait(false);
                        _unitOfWork.TaskTeamMembersRepository.Add(TaskTeamMembers);
                    }
                    checkIfAssigned = true;
                }
                _unitOfWork.Commit();

                if (Convert.ToBoolean(checkIfAssigned) == false)
                {
                    return await System.Threading.Tasks.Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Alert", Desc = "All subtask are already assigned. Please assign manually." }).ConfigureAwait(false);
                }

                return await System.Threading.Tasks.Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Task updated successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return System.Threading.Tasks.Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("GetAllTaskForAssignByProjectID")]
        public async Task<object> GetAllTaskForAssignByProjectID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                List<ProjectActivityTaskEntityViewModel> projectActivityTasks = new List<ProjectActivityTaskEntityViewModel>();

                projectActivityTasks = (await _unitOfWork.ProjectActivityTaskRepository.GetAllTaskForAssignByProjectID(Utils.ID).ConfigureAwait(false)).ToList();

                foreach (var item in projectActivityTasks.Select((value, index) => new { value, index }))
                    projectActivityTasks[item.index].TaskTeamMember = (await _unitOfWork.TaskTeamMembersRepository.FindByTaskID(item.value.task_id).ConfigureAwait(false)).ToList();

                return await System.Threading.Tasks.Task.FromResult<object>(JsonConvert.SerializeObject(projectActivityTasks, Formatting.Indented)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        #endregion ProjectActivityTask

        #region ProjectType

        [HttpPost]
        [Route("AddProjectType")]
        public async Task<object> AddProjectType([FromBody] ProjectTypeViewModel statusingViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (statusingViewModel == null)
                    throw new ArgumentNullException(nameof(statusingViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<ProjectTypeViewModel, ProjectType>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<ProjectType>(statusingViewModel);

                modal.id = Guid.NewGuid().ToString();
                modal.created_date = _dateTime.ToString();
                modal.is_deleted = false;

                _unitOfWork.ProjectTypeRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "ProjectType registered successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindByProjectTypeID")]
        public async Task<object> FindByProjectTypeID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = _unitOfWork.ProjectTypeRepository.Find(Utils.ID);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetAllProjectType")]
        public async Task<object> GetAllProjectType(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.ProjectTypeRepository.All();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("GetProjectTypeByOrgID")]
        public async Task<object> GetProjectTypeByOrgID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = _unitOfWork.ProjectTypeRepository.FindByOrgID(Utils.ID);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemoveProjectType")]
        public async Task<object> RemoveProjectType([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                _unitOfWork.ProjectTypeRepository.Remove(Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "ProjectType removed successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdateProjectType")]
        public async Task<object> UpdateProjectType([FromBody] ProjectTypeViewModel statusingViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (statusingViewModel == null)
                    throw new ArgumentNullException(nameof(statusingViewModel));

                statusingViewModel.modified_date = _dateTime.ToString();
                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<ProjectTypeViewModel, ProjectType>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<ProjectType>(statusingViewModel);

                modal.modified_date = _dateTime.ToString();

                _unitOfWork.ProjectTypeRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "ProjectType updated successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        #endregion ProjectType

        #region ProjectTags

        [HttpPost]
        [Route("AddProjectTags")]
        public async Task<object> AddProjectTags([FromBody] ProjectTagsViewModel statusingViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (statusingViewModel == null)
                    throw new ArgumentNullException(nameof(statusingViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<ProjectTagsViewModel, ProjectTags>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<ProjectTags>(statusingViewModel);

                modal.id = Guid.NewGuid().ToString();
                modal.created_date = _dateTime.ToString();
                modal.is_deleted = false;

                _unitOfWork.ProjectTagsRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "ProjectTags registered successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindByProjectTagsID")]
        public async Task<object> FindByProjectTagsID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = await _unitOfWork.ProjectTagsRepository.Find(Utils.ID).ConfigureAwait(false);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetAllProjectTags")]
        public async Task<object> GetAllProjectTags(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = await _unitOfWork.ProjectTagsRepository.All().ConfigureAwait(false);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("GetProjectTagsByUnitID")]
        public async Task<object> GetProjectTagsByUnitID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = await _unitOfWork.ProjectTagsRepository.GetProjectTagsByUnitID(Utils.ID).ConfigureAwait(false);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemoveProjectTags")]
        public async Task<object> RemoveProjectTags([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                _unitOfWork.ProjectTagsRepository.Remove(Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "ProjectTags removed successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdateProjectTags")]
        public async Task<object> UpdateProjectTags([FromBody] ProjectTagsViewModel statusingViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (statusingViewModel == null)
                    throw new ArgumentNullException(nameof(statusingViewModel));

                statusingViewModel.modified_date = _dateTime.ToString();
                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<ProjectTagsViewModel, ProjectTags>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<ProjectTags>(statusingViewModel);

                modal.modified_date = _dateTime.ToString();

                _unitOfWork.ProjectTagsRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "ProjectTags updated successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        #endregion ProjectTags
    }
}