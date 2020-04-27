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
using TimeAPI.API.Models.ProjectTypeViewModels;
using TimeAPI.API.Models.ProjectViewModels;
using TimeAPI.API.Models.StatusViewModels;
using TimeAPI.API.Models.TaskViewModels;
using TimeAPI.API.Services;
using TimeAPI.Domain;
using TimeAPI.Domain.Entities;
using TimeAPI.API.Models.CostProjectViewModels;

namespace TimeAPI.API.Controllroers
{
    //[ApiKeyAuth]
    //[EnableCors("CorsPolicy")]
    [Route("[controller]")]
    //[Authorize(Roles = "superadmin")]
    public class CostController : Controller
    {
        private readonly ApplicationSettings _appSettings;
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly DateTime _dateTime;

        public CostController(IUnitOfWork unitOfWork, ILogger<CostController> logger,
            IEmailSender emailSender, IOptions<ApplicationSettings> AppSettings)
        {
            _emailSender = emailSender;
            _logger = logger;
            _appSettings = AppSettings.Value;
            _unitOfWork = unitOfWork;
            _dateTime = InternetTime.GetCurrentTimeFromTimeZone().Value.DateTime;
        }

        #region CostProjects

        [HttpPost]
        [Route("AddCostProject")]
        public async Task<object> AddCostProject([FromBody] CostProjectViewModel projectViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (projectViewModel == null)
                    throw new ArgumentNullException(nameof(projectViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<CostProjectViewModel, CostProject>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<CostProject>(projectViewModel);


                modal.id = Guid.NewGuid().ToString();
                modal.project_status_id = _unitOfWork.ProjectStatusRepository.GetProjectStatusByOrgID("default")
                                            .Where(s => s.project_status_name.Equals("Open"))
                                            .Select(s => s.id)
                                            .FirstOrDefault();

                modal.created_date = _dateTime.ToString();
                modal.is_deleted = false;

                if (projectViewModel.EntityContact != null)
                {
                    var entityContact = new EntityContact()
                    {
                        id = Guid.NewGuid().ToString(),
                        entity_id = modal.id,
                        name = projectViewModel.EntityContact.name,
                        position = projectViewModel.EntityContact.position,
                        phone = projectViewModel.EntityContact.phone,
                        mobile = projectViewModel.EntityContact.mobile,
                        email = projectViewModel.EntityContact.email,
                        createdby = projectViewModel.createdby,
                        created_date = _dateTime.ToString(),
                        is_deleted = false
                    };
                    _unitOfWork.EntityContactRepository.Add(entityContact);
                }


                if (projectViewModel.cst_id != null)
                {
                    var customerCostProject = new CustomerProject()
                    {
                        id = Guid.NewGuid().ToString(),
                        cst_id = projectViewModel.cst_id,
                        project_id = modal.id,
                        createdby = projectViewModel.createdby,
                        created_date = _dateTime.ToString(),
                        is_deleted = false
                    };
                    _unitOfWork.CustomerProjectRepository.Add(customerCostProject);
                }


                for (int i = 0; i < projectViewModel.CostProjectMilestone.Count; i++)
                {
                    //Add Milestone
                    var configMileStone = new AutoMapper.MapperConfiguration(m => m.CreateMap<CostProjectMilestone, CostProjectMilestone>());
                    var mapperMileStone = config.CreateMapper();
                    var modalMileStone = mapper.Map<CostProjectMilestone>(projectViewModel.CostProjectMilestone[i]);

                    modalMileStone.id = Guid.NewGuid().ToString();
                    modalMileStone.project_id = modal.id;
                    modalMileStone.createdby = projectViewModel.createdby;
                    modalMileStone.created_date = _dateTime.ToString();
                    modalMileStone.is_deleted = false;


                    for (int x = 0; x < projectViewModel.CostProjectMilestone[i].CostProjectTask.Count; x++)
                    {
                        //Add Task
                        var configTask = new AutoMapper.MapperConfiguration(m => m.CreateMap<CostProjectTask, CostProjectTask>());
                        var mapperTask = config.CreateMapper();
                        var modalTask = mapper.Map<CostProjectTask>(projectViewModel.CostProjectMilestone[i].CostProjectTask[x]);

                        modalTask.id = Guid.NewGuid().ToString();
                        modalTask.project_id = modal.id;
                        modalTask.activtity_id = modalMileStone.id;
                        modalTask.createdby = projectViewModel.createdby;
                        modalTask.created_date = _dateTime.ToString();
                        modalTask.is_deleted = false;
                        _unitOfWork.CostProjectTaskRepository.Add(modalTask);
                    }
                    _unitOfWork.CostProjectMilestoneRepository.Add(modalMileStone);
                }

                _unitOfWork.CostProjectRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "CostProject saved successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FetchAllCostProjectByOrgID")]
        public async Task<object> FetchAllCostProjectByOrgID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                oDataTable _oDataTable = new oDataTable();
                var results = _unitOfWork.CostProjectRepository.FetchAllCostProjectByOrgID(Utils.ID);
                var xResult = _oDataTable.ToDataTable(results);

                return await System.Threading.Tasks.Task.FromResult<object>(JsonConvert.SerializeObject(xResult, Formatting.Indented)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindByCostProjectID")]
        public async Task<object> FindByCostProjectID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                CostProjectDetailViewModel projectViewModel = new CostProjectDetailViewModel();

                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var results = _unitOfWork.CostProjectRepository.Find(Utils.ID);
                //var resultLocation = _unitOfWork.EntityLocationRepository.FindByEnitiyID(results.id);
                var resultContact = _unitOfWork.EntityContactRepository.FindByEntityID(results.id);
                var resultCustomer = _unitOfWork.CustomerRepository.FindCustomerByProjectID(results.id);

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<CostProjectDetailViewModel, CostProject>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<CostProject>(results);

                //modal.EntityLocation = resultLocation;
                modal.EntityContact = resultContact;
                modal.EntityCustomer = resultCustomer;

                return await Task.FromResult<object>(modal).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetAllCostProject")]
        public async Task<object> GetAllCostProject(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.CostProjectRepository.All();
                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemoveCostProjectByID")]
        public async Task<object> RemoveCostProjectByID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                _unitOfWork.CostProjectRepository.Remove(Utils.ID);
                _unitOfWork.EntityContactRepository.RemoveByEntityID(Utils.ID);
                //_unitOfWork.EntityLocationRepository.RemoveByEntityID(Utils.ID);
                //_unitOfWork.CostProjectActivityTaskRepository.RemoveByCostProjectID(Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "CostProject removed successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdateCostProject")]
        public async Task<object> UpdateCostProject([FromBody] CostProjectViewModel projectViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (projectViewModel == null)
                    throw new ArgumentNullException(nameof(projectViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<CostProjectViewModel, CostProject>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<CostProject>(projectViewModel);
                modal.modified_date = _dateTime.ToString();

                if (projectViewModel.EntityContact != null)
                {
                    var entityContact = new EntityContact()
                    {
                        entity_id = modal.id,
                        name = projectViewModel.EntityContact.name,
                        position = projectViewModel.EntityContact.position,
                        phone = projectViewModel.EntityContact.phone,
                        mobile = projectViewModel.EntityContact.mobile,
                        email = projectViewModel.EntityContact.email,
                        modifiedby = projectViewModel.createdby,
                        modified_date = _dateTime.ToString()
                    };
                    _unitOfWork.EntityContactRepository.Update(entityContact);
                }



                if (projectViewModel.cst_id != null)
                {
                    var customerCostProject = new CustomerProject()
                    {
                        cst_id = projectViewModel.cst_id,
                        project_id = modal.id,
                        createdby = projectViewModel.createdby,
                        created_date = _dateTime.ToString(),
                        is_deleted = false
                    };
                    _unitOfWork.CustomerProjectRepository.Update(customerCostProject);
                }

                _unitOfWork.CostProjectRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "CostProject updated successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("UpdateCostProjectStatusByID")]
        public async Task<object> UpdateCostProjectStatusByID([FromBody] CostProjectStatusModel projectViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (projectViewModel == null)
                    throw new ArgumentNullException(nameof(projectViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<CostProjectStatusModel, CostProject>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<CostProject>(projectViewModel);

                modal.id = Guid.NewGuid().ToString();
                modal.modified_date = _dateTime.ToString();
                modal.is_deleted = false;

                _unitOfWork.CostProjectRepository.UpdateCostProjectStatusByID(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "CostProject Status Updated Successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("AddCostProjectCustomer")]
        public async Task<object> AddCostProjectCustomer([FromBody] CostProjectCustomerViewModel statusingViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (statusingViewModel == null)
                    throw new ArgumentNullException(nameof(statusingViewModel));

                var CustomerCostProject = _unitOfWork.CustomerProjectRepository.Find(statusingViewModel.project_id);

                if (CustomerCostProject == null)
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
                else if (CustomerCostProject != null)
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

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "CostProject registered successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        //[HttpPost]
        //[Route("CostProjectTaskCountByCostProjectID")]
        //public async Task<object> CostProjectTaskCountByCostProjectID([FromBody] Utils _Utils, CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        if (cancellationToken != null)
        //            cancellationToken.ThrowIfCancellationRequested();

        //        if (_Utils == null)
        //            throw new ArgumentNullException(nameof(_Utils));

        //        var results = _unitOfWork.CostProjectRepository.CostProjectTaskCount(_Utils.ID);

        //        return await Task.FromResult<object>(results).ConfigureAwait(false);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
        //    }
        //}

        //[HttpPost]
        //[Route("FindAllCostProjectActivityByCostProjectID")]
        //public async Task<object> FindAllCostProjectActivityByCostProjectID([FromBody] Utils Utils, CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        if (cancellationToken != null)
        //            cancellationToken.ThrowIfCancellationRequested();

        //        if (Utils == null)
        //            throw new ArgumentNullException(nameof(Utils.ID));

        //        oDataTable _oDataTable = new oDataTable();
        //        var results = _unitOfWork.CostProjectRepository.FindAllCostProjectActivityByCostProjectID(Utils.ID);
        //        var xResult = _oDataTable.ToDataTable(results);

        //        return await System.Threading.Tasks.Task.FromResult<object>(JsonConvert.SerializeObject(xResult, Formatting.Indented)).ConfigureAwait(false);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
        //    }
        //}

        #endregion CostProjects







        //#region ProjectActivity

        //[HttpPost]
        //[Route("AddProjectActivity")]
        //public async Task<object> AddProjectActivity([FromBody] ProjectActivityViewModel statusingViewModel, CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        if (cancellationToken != null)
        //            cancellationToken.ThrowIfCancellationRequested();

        //        if (statusingViewModel == null)
        //            throw new ArgumentNullException(nameof(statusingViewModel));

        //        var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<ProjectActivityViewModel, ProjectActivity>());
        //        var mapper = config.CreateMapper();
        //        var modal = mapper.Map<ProjectActivity>(statusingViewModel);

        //        modal.id = Guid.NewGuid().ToString();
        //        modal.created_date = _dateTime.ToString();
        //        modal.is_deleted = false;

        //        modal.status_id = _unitOfWork.ProjectStatusRepository.GetProjectStatusByOrgID("default")
        //                              .Where(s => s.project_status_name.Equals("Open"))
        //                              .Select(s => s.id)
        //                              .FirstOrDefault();

        //        _unitOfWork.ProjectActivityRepository.Add(modal);
        //        _unitOfWork.Commit();

        //        return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Project Activity registered successfully." }).ConfigureAwait(false);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
        //    }
        //}

        //[HttpPost]
        //[Route("FindByProjectActivityID")]
        //public async Task<object> FindByProjectActivityID([FromBody] Utils Utils, CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        if (cancellationToken != null)
        //            cancellationToken.ThrowIfCancellationRequested();

        //        if (Utils == null)
        //            throw new ArgumentNullException(nameof(Utils.ID));

        //        var result = _unitOfWork.ProjectActivityRepository.FindByProjectActivityID(Utils.ID);

        //        return await Task.FromResult<object>(result).ConfigureAwait(false);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
        //    }
        //}

        //[HttpGet]
        //[Route("GetAllProjectActivity")]
        //public async Task<object> GetAllProjectActivity(CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        if (cancellationToken != null)
        //            cancellationToken.ThrowIfCancellationRequested();

        //        var result = _unitOfWork.ProjectActivityRepository.All();

        //        return await Task.FromResult<object>(result).ConfigureAwait(false);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
        //    }
        //}

        //[HttpPost]
        //[Route("GetProjectActivityByProjectID")]
        //public async Task<object> GetProjectActivityByProjectID([FromBody] Utils Utils, CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        if (cancellationToken != null)
        //            cancellationToken.ThrowIfCancellationRequested();

        //        if (Utils == null)
        //            throw new ArgumentNullException(nameof(Utils.ID));

        //        var result = _unitOfWork.ProjectActivityRepository.GetProjectActivityByProjectID(Utils.ID);

        //        return await Task.FromResult<object>(result).ConfigureAwait(false);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
        //    }
        //}

        //[HttpPost]
        //[Route("RemoveProjectActivity")]
        //public async Task<object> RemoveProjectActivity([FromBody] Utils Utils, CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        if (cancellationToken != null)
        //            cancellationToken.ThrowIfCancellationRequested();

        //        if (Utils == null)
        //            throw new ArgumentNullException(nameof(Utils.ID));

        //        _unitOfWork.ProjectActivityRepository.Remove(Utils.ID);
        //        _unitOfWork.ProjectActivityTaskRepository.RemoveByProjectActivityID(Utils.ID);
        //        _unitOfWork.Commit();

        //        return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Project Activity Removed successfully." }).ConfigureAwait(false);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
        //    }
        //}

        //[HttpPatch]
        //[Route("UpdateProjectActivity")]
        //public async Task<object> UpdateProjectActivity([FromBody] ProjectActivityViewModel statusingViewModel, CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        if (cancellationToken != null)
        //            cancellationToken.ThrowIfCancellationRequested();

        //        if (statusingViewModel == null)
        //            throw new ArgumentNullException(nameof(statusingViewModel));

        //        statusingViewModel.modified_date = _dateTime.ToString();
        //        var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<ProjectActivityViewModel, ProjectActivity>());
        //        var mapper = config.CreateMapper();
        //        var modal = mapper.Map<ProjectActivity>(statusingViewModel);

        //        modal.modified_date = _dateTime.ToString();

        //        var status = _unitOfWork.StatusRepository.Find(statusingViewModel.status_id);
        //        if (status != null)
        //            if (status.status_name == "Completed")
        //            {
        //                var Result = _unitOfWork.ProjectActivityTaskRepository.GetAllTaskByActivityID(modal.id);
        //                if (Result != null)
        //                {
        //                    return await Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = "Error", Desc = "There are still task for this milestone is pending." }).ConfigureAwait(false);
        //                }
        //            }

        //        _unitOfWork.ProjectActivityRepository.Update(modal);
        //        _unitOfWork.Commit();

        //        return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Project Activity updated successfully." }).ConfigureAwait(false);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
        //    }
        //}

        //[HttpPatch]
        //[Route("UpdateProjectActivityStatusByActivityID")]
        //public async Task<object> UpdateProjectActivityStatusByActivityID([FromBody] ProjectActivityStatusUpdateViewModel statusingViewModel, CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        if (cancellationToken != null)
        //            cancellationToken.ThrowIfCancellationRequested();

        //        if (statusingViewModel == null)
        //            throw new ArgumentNullException(nameof(statusingViewModel));

        //        statusingViewModel.modified_date = _dateTime.ToString();
        //        var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<ProjectActivityViewModel, ProjectActivity>());
        //        var mapper = config.CreateMapper();
        //        var modal = mapper.Map<ProjectActivity>(statusingViewModel);

        //        modal.modified_date = _dateTime.ToString();

        //        _unitOfWork.ProjectActivityRepository.UpdateProjectActivityStatusByActivityID(modal);
        //        _unitOfWork.Commit();

        //        return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Project Activity updated successfully." }).ConfigureAwait(false);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
        //    }
        //}

        //[HttpPost]
        //[Route("GetProjectActivityRatioByProjectID")]
        //public async Task<object> GetProjectActivityRatioByProjectID([FromBody] Utils Utils, CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        if (cancellationToken != null)
        //            cancellationToken.ThrowIfCancellationRequested();

        //        if (Utils == null)
        //            throw new ArgumentNullException(nameof(Utils.ID));

        //        var result = _unitOfWork.ProjectActivityRepository.GetProjectActivityRatioByProjectID(Utils.ID);

        //        return await Task.FromResult<object>(result).ConfigureAwait(false);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
        //    }
        //}

        //#endregion ProjectActivity

        //#region ProjectActivityTask

        //[HttpPost]
        //[Route("AddTask")]
        //public async Task<object> AddTask([FromBody]  ProjectActivityTaskViewModel TaskViewModel, CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        if (cancellationToken != null)
        //            cancellationToken.ThrowIfCancellationRequested();

        //        if (TaskViewModel == null)
        //            throw new ArgumentNullException(nameof(TaskViewModel));

        //        var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<ProjectActivityTaskViewModel, Domain.Entities.Tasks>());
        //        var mapper = config.CreateMapper();
        //        var modal = mapper.Map<Domain.Entities.Tasks>(TaskViewModel);

        //        modal.id = Guid.NewGuid().ToString();
        //        modal.status_id = _unitOfWork.StatusRepository.GetStatusByOrgID("default")
        //                            .Where(s => s.status_name.Equals("Open"))
        //                            .Select(s => s.id)
        //                            .FirstOrDefault();

        //        modal.created_date = _dateTime.ToString();
        //        modal.is_deleted = false;

        //        if (TaskViewModel.employees != null)
        //        {
        //            foreach (var item in TaskViewModel.employees.Distinct())
        //            {

        //                var TaskTeamMembers = new TaskTeamMember()
        //                {
        //                    id = Guid.NewGuid().ToString(),
        //                    task_id = modal.id,
        //                    empid = item.empid,
        //                    createdby = modal.createdby,
        //                    created_date = _dateTime.ToString(),
        //                    is_deleted = false
        //                };
        //                _unitOfWork.TaskTeamMembersRepository.Add(TaskTeamMembers);
        //            }
        //        }

        //        var ProjectTask = new ProjectActivityTask()
        //        {
        //            id = Guid.NewGuid().ToString(),
        //            project_id = TaskViewModel.project_id,
        //            activity_id = TaskViewModel.activtity_id,
        //            task_id = modal.id,
        //            created_date = _dateTime.ToString(),
        //            createdby = TaskViewModel.createdby,
        //            is_deleted = false
        //        };

        //        _unitOfWork.TaskRepository.Add(modal);
        //        _unitOfWork.ProjectActivityTaskRepository.Add(ProjectTask);
        //        _unitOfWork.Commit();

        //        return await System.Threading.Tasks.Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Task registered successfully." }).ConfigureAwait(false);
        //    }
        //    catch (Exception ex)
        //    {
        //        return System.Threading.Tasks.Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
        //    }
        //}

        //[HttpPost]
        //[Route("AddMultipleTask")]
        //public async Task<object> AddMultipleTask([FromBody] List<ProjectActivityTaskViewModel> TaskViewModel, CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        if (cancellationToken != null)
        //            cancellationToken.ThrowIfCancellationRequested();

        //        if (TaskViewModel == null)
        //            throw new ArgumentNullException(nameof(TaskViewModel));

        //        for (int i = 0; i < TaskViewModel.Count; i++)
        //        {
        //            var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<ProjectActivityTaskViewModel, Domain.Entities.Tasks>());
        //            var mapper = config.CreateMapper();
        //            var modal = mapper.Map<Domain.Entities.Tasks>(TaskViewModel[i]);

        //            modal.id = Guid.NewGuid().ToString();
        //            modal.status_id = _unitOfWork.StatusRepository.GetStatusByOrgID("default")
        //                                .Where(s => s.status_name.Equals("Open"))
        //                                .Select(s => s.id)
        //                                .FirstOrDefault();

        //            modal.created_date = _dateTime.ToString();
        //            modal.is_deleted = false;

        //            if (TaskViewModel[i].employees != null)
        //            {
        //                foreach (var item in TaskViewModel[i].employees.Distinct())
        //                {

        //                    var TaskTeamMembers = new TaskTeamMember()
        //                    {
        //                        id = Guid.NewGuid().ToString(),
        //                        task_id = modal.id,
        //                        empid = item.empid,
        //                        createdby = modal.createdby,
        //                        created_date = _dateTime.ToString(),
        //                        is_deleted = false
        //                    };
        //                    _unitOfWork.TaskTeamMembersRepository.Add(TaskTeamMembers);
        //                }
        //            }

        //            var ProjectTask = new ProjectActivityTask()
        //            {
        //                id = Guid.NewGuid().ToString(),
        //                project_id = TaskViewModel[i].project_id,
        //                activity_id = TaskViewModel[i].activtity_id,
        //                task_id = modal.id,
        //                created_date = _dateTime.ToString(),
        //                createdby = TaskViewModel[i].createdby,
        //                is_deleted = false
        //            };

        //            _unitOfWork.TaskRepository.Add(modal);
        //            _unitOfWork.ProjectActivityTaskRepository.Add(ProjectTask);
        //        }


        //        _unitOfWork.Commit();

        //        return await System.Threading.Tasks.Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Task registered successfully." }).ConfigureAwait(false);
        //    }
        //    catch (Exception ex)
        //    {
        //        return System.Threading.Tasks.Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
        //    }
        //}



        //[HttpPost]
        //[Route("FindByTasksId")]
        //public async Task<object> FindByTasksID([FromBody] Utils Utils, CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        if (cancellationToken != null)
        //            cancellationToken.ThrowIfCancellationRequested();

        //        if (Utils == null)
        //            throw new ArgumentNullException(nameof(Utils.ID));

        //        var results = _unitOfWork.TaskRepository.Find(Utils.ID);
        //        results.employees = _unitOfWork.TaskTeamMembersRepository.FindByTaskID(Utils.ID).ToList();

        //        return await System.Threading.Tasks.Task.FromResult<object>(results).ConfigureAwait(false);
        //    }
        //    catch (Exception ex)
        //    {
        //        return System.Threading.Tasks.Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
        //    }
        //}

        //[HttpGet]
        //[Route("GetAllTasks")]
        //public async Task<object> GetAllTasks(CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        if (cancellationToken != null)
        //            cancellationToken.ThrowIfCancellationRequested();

        //        var result = _unitOfWork.TaskRepository.All();

        //        return await System.Threading.Tasks.Task.FromResult<object>(result).ConfigureAwait(false);
        //    }
        //    catch (Exception ex)
        //    {
        //        return System.Threading.Tasks.Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
        //    }
        //}

        //[HttpPost]
        //[Route("RemoveTask")]
        //public async Task<object> RemoveTask([FromBody] Utils Utils, CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        if (cancellationToken != null)
        //            cancellationToken.ThrowIfCancellationRequested();

        //        if (Utils == null)
        //            throw new ArgumentNullException(nameof(Utils.ID));

        //        _unitOfWork.TaskRepository.Remove(Utils.ID);
        //        _unitOfWork.ProjectActivityTaskRepository.Remove(Utils.ID);
        //        _unitOfWork.Commit();

        //        return await System.Threading.Tasks.Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Task removed successfully." }).ConfigureAwait(false);
        //    }
        //    catch (Exception ex)
        //    {
        //        return System.Threading.Tasks.Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
        //    }
        //}

        //[HttpPatch]
        //[Route("UpdateTask")]
        //public async Task<object> UpdateAddTask([FromBody] ProjectActivityTaskViewModel TaskViewModel, CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        if (cancellationToken != null)
        //            cancellationToken.ThrowIfCancellationRequested();

        //        if (TaskViewModel == null)
        //            throw new ArgumentNullException(nameof(TaskViewModel));

        //        var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<ProjectActivityTaskViewModel, Domain.Entities.Tasks>());
        //        var mapper = config.CreateMapper();
        //        var modal = mapper.Map<Domain.Entities.Tasks>(TaskViewModel);
        //        modal.modified_date = _dateTime.ToString();

        //        _unitOfWork.TaskTeamMembersRepository.RemoveByTaskID(modal.id);

        //        if (TaskViewModel.employees != null)
        //        {
        //            foreach (var item in TaskViewModel.employees.Distinct())
        //            {


        //                var TaskTeamMembers = new TaskTeamMember()
        //                {
        //                    id = Guid.NewGuid().ToString(),
        //                    task_id = modal.id,
        //                    empid = item.empid,
        //                    createdby = modal.createdby,
        //                    created_date = _dateTime.ToString(),
        //                    is_deleted = false
        //                };
        //                _unitOfWork.TaskTeamMembersRepository.Add(TaskTeamMembers);
        //            }
        //        }

        //        _unitOfWork.TaskRepository.Update(modal);
        //        _unitOfWork.Commit();

        //        return await System.Threading.Tasks.Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Task updated successfully." }).ConfigureAwait(false);
        //    }
        //    catch (Exception ex)
        //    {
        //        return System.Threading.Tasks.Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
        //    }
        //}

        //[HttpPost]
        //[Route("GetAllTaskByActivityID")]
        //public async Task<object> GetAllTaskByActivityID([FromBody] Utils Utils, CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        if (cancellationToken != null)
        //            cancellationToken.ThrowIfCancellationRequested();

        //        if (Utils == null)
        //            throw new ArgumentNullException(nameof(Utils.ID));

        //        oDataTable _oDataTable = new oDataTable();
        //        var results = _unitOfWork.ProjectActivityTaskRepository.GetAllTaskByActivityID(Utils.ID);
        //        var xResult = _oDataTable.ToDataTable(results);

        //        return await System.Threading.Tasks.Task.FromResult<object>(JsonConvert.SerializeObject(xResult, Formatting.Indented)).ConfigureAwait(false);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
        //    }
        //}

        //[HttpPost]
        //[Route("GetAllTaskByProjectID")]
        //public async Task<object> GetAllTaskByProjectID([FromBody] Utils Utils, CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        if (cancellationToken != null)
        //            cancellationToken.ThrowIfCancellationRequested();

        //        if (Utils == null)
        //            throw new ArgumentNullException(nameof(Utils.ID));

        //        oDataTable _oDataTable = new oDataTable();
        //        var results = _unitOfWork.ProjectActivityTaskRepository.GetAllTaskByProjectID(Utils.ID);
        //        var xResult = _oDataTable.ToDataTable(results);

        //        return await System.Threading.Tasks.Task.FromResult<object>(JsonConvert.SerializeObject(xResult, Formatting.Indented)).ConfigureAwait(false);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
        //    }
        //}

        //[HttpPost]
        //[Route("GetProjectActivityTaskRatioByProjectID")]
        //public async Task<object> GetProjectActivityTaskRatioByProjectID([FromBody] Utils Utils, CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        if (cancellationToken != null)
        //            cancellationToken.ThrowIfCancellationRequested();

        //        if (Utils == null)
        //            throw new ArgumentNullException(nameof(Utils.ID));

        //        var results = _unitOfWork.ProjectActivityTaskRepository.GetProjectActivityTaskRatioByProjectID(Utils.ID);
        //        return await Task.FromResult<object>(results).ConfigureAwait(false);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
        //    }
        //}

        //#endregion ProjectActivityTask


    }
}