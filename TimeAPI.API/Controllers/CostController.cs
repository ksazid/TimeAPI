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
using TimeAPI.API.Services;
using TimeAPI.Domain;
using TimeAPI.Domain.Entities;
using TimeAPI.API.Models.CostProjectViewModels;
using TimeAPI.API.Models.TypeOfDesignViewModels;
using TimeAPI.API.Models.SpecifiationViewModels;
using TimeAPI.API.Models.UnitDescriptionViewModels;
using System.Collections;
using System.Data;

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

                //if (projectViewModel.TypeOfDesign != null)
                //{
                //    foreach (var item in projectViewModel.TypeOfDesign)
                //    {
                //        var typeOfDesign = new TypeOfDesign()
                //        {
                //            id = Guid.NewGuid().ToString(),
                //            org_id = modal.org_id,
                //            project_id = modal.id,
                //            design_name = item.design_name,
                //            createdby = projectViewModel.createdby,
                //            created_date = _dateTime.ToString(),
                //            is_deleted = false
                //        };
                //        _unitOfWork.TypeOfDesignRepository.Add(typeOfDesign);
                //    }
                //}

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

                //get all static milestone
                var StaticCostProjectMilestone = _unitOfWork.CostProjectMilestoneRepository.GetAllStaticMilestoneByOrgID(modal.org_id);
                DataTable tableCostProjectMilestone = new DataTable();
                tableCostProjectMilestone.Columns.Add("id", typeof(string));
                tableCostProjectMilestone.Columns.Add("project_id", typeof(string));
                tableCostProjectMilestone.Columns.Add("milestone_id", typeof(string));
                tableCostProjectMilestone.Columns.Add("milestone_name", typeof(string));

                foreach (var itemStaticCostProjectMilestone in StaticCostProjectMilestone)
                    tableCostProjectMilestone.Rows.Add(Guid.NewGuid().ToString(),
                                    modal.id, itemStaticCostProjectMilestone.id,
                                    itemStaticCostProjectMilestone.milestone_name);


                DataTable tableCostMilestoneTask = new DataTable();
                tableCostMilestoneTask.Columns.Add("milestone_id", typeof(string));
                tableCostMilestoneTask.Columns.Add("project_id", typeof(string));
                tableCostMilestoneTask.Columns.Add("task_name", typeof(string));
                tableCostMilestoneTask.Columns.Add("qty", typeof(int));
                tableCostMilestoneTask.Columns.Add("unit_id", typeof(string));


                if (projectViewModel.ProjectUnit != null)
                    foreach (var item in projectViewModel.ProjectUnit)
                    {
                        var projectUnit = new ProjectUnit()
                        {
                            id = Guid.NewGuid().ToString(),
                            org_id = modal.org_id,
                            project_id = modal.id,
                            unit_id = item.unit_id,
                            unit_name = item.unit_name,
                            no_of_unit = item.no_of_unit,
                            unit_qty = item.unit_qty,
                            note = item.note,
                            createdby = projectViewModel.createdby,
                            created_date = _dateTime.ToString(),
                            is_deleted = false
                        };

                        //ProjectDesignType
                        if (item.ProjectDesignType != null)
                            foreach (var itemProjectDesignType in item.ProjectDesignType)
                            {
                                var projectDesignType = new ProjectDesignType()
                                {
                                    id = Guid.NewGuid().ToString(),
                                    org_id = modal.org_id,
                                    project_id = modal.id,
                                    unit_id = projectUnit.id,
                                    design_type_id = itemProjectDesignType.id,
                                    createdby = projectViewModel.createdby,
                                    created_date = _dateTime.ToString(),
                                    is_deleted = false

                                };
                                _unitOfWork.ProjectDesignTypeRepository.Add(projectDesignType);
                            }

                        //tags
                        if (item.ProjectTags != null)
                            foreach (var itemProjectTags in item.ProjectTags)
                            {
                                var projectTags = new ProjectTags()
                                {
                                    id = Guid.NewGuid().ToString(),
                                    project_id = modal.id,
                                    unit_id = projectUnit.id,
                                    tags = itemProjectTags.tags,
                                    createdby = projectViewModel.createdby,
                                    created_date = _dateTime.ToString(),
                                    is_deleted = false
                                };
                                _unitOfWork.ProjectTagsRepository.Add(projectTags);
                            }


                        //convert the no_of_unit into interger
                        string numberofunit = "";
                        numberofunit = (Convert.ToInt32(item.no_of_unit) * 4).ToString();

                        foreach (DataRow itemStaticCostProjectMilestone in tableCostProjectMilestone.Rows)
                        {
                            //get all static milestone tasks and save it with projectid in cost_milestone_tasks
                            var StaticCostProjectMilestoneTasks = _unitOfWork.CostProjectTaskRepository
                                                                   .GetAllStaticMilestoneTasksByMilestoneID
                                                                   (itemStaticCostProjectMilestone["milestone_id"].ToString());

                            foreach (var itemStaticCostProjectMilestoneTasks in StaticCostProjectMilestoneTasks)
                                if (tableCostMilestoneTask.Rows.Count > 0)
                                {
                                    if (tableCostMilestoneTask.AsEnumerable().Where(c => c.Field<string>("task_name")
                                                            .Equals(itemStaticCostProjectMilestoneTasks.task_name)).Any())
                                    {
                                        var qty = tableCostMilestoneTask.AsEnumerable()
                                                                        .Where(c => c.Field<string>("task_name") == itemStaticCostProjectMilestoneTasks.task_name)
                                                                        .Sum(x => x.Field<int>("qty"));

                                        if (numberofunit.Length > 0)
                                            tableCostMilestoneTask.AsEnumerable()
                                                                            .Where(c => c.Field<string>("task_name") == itemStaticCostProjectMilestoneTasks.task_name)
                                                                            .ToList()
                                                                            .ForEach(D => D.SetField("qty", qty + Convert.ToInt32(numberofunit)));

                                    }
                                    else
                                    {
                                        tableCostMilestoneTask.Rows.Add(itemStaticCostProjectMilestone["id"],
                                           modal.id, itemStaticCostProjectMilestoneTasks.task_name,
                                           numberofunit, item.unit_id);
                                    }
                                }
                                else
                                {
                                    tableCostMilestoneTask.Rows.Add(itemStaticCostProjectMilestone["id"],
                                       modal.id, itemStaticCostProjectMilestoneTasks.task_name,
                                       numberofunit, item.unit_id);
                                }
                        }

                        _unitOfWork.ProjectUnitRepository.Add(projectUnit);
                    }

                foreach (DataRow itemStaticCostProjectMilestone in tableCostProjectMilestone.Rows)
                {
                    var _CostProjectMilestone = new CostProjectMilestone()
                    {
                        id = itemStaticCostProjectMilestone["id"].ToString(),
                        project_id = modal.id,
                        milestone_name = itemStaticCostProjectMilestone["milestone_name"].ToString(),
                        createdby = projectViewModel.createdby,
                        created_date = _dateTime.ToString(),
                        is_deleted = false
                    };

                    _unitOfWork.CostProjectMilestoneRepository.Add(_CostProjectMilestone);
                }

                foreach (DataRow itemtableCostMilestoneTask in tableCostMilestoneTask.Rows)
                {
                    var CostProjectMilestoneTask = new CostProjectTask()
                    {
                        id = Guid.NewGuid().ToString(),
                        project_id = modal.id,
                        is_selected = true,
                        milestone_id = itemtableCostMilestoneTask["milestone_id"].ToString(),
                        task_name = itemtableCostMilestoneTask["task_name"].ToString(),
                        unit = itemtableCostMilestoneTask["unit_id"].ToString(),
                        qty = itemtableCostMilestoneTask["qty"].ToString(),
                        createdby = projectViewModel.createdby,
                        created_date = _dateTime.ToString(),
                        is_deleted = false
                    };
                    _unitOfWork.CostProjectTaskRepository.Add(CostProjectMilestoneTask);
                }

                string _project_id = modal.id;
                _unitOfWork.CostProjectRepository.Add(modal);
                _unitOfWork.Commit();
                return await Task.FromResult<object>(new Utils { ID = _project_id }).ConfigureAwait(false);
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





        [HttpPost]
        [Route("CalculateCostProject")]
        public async Task<object> CalculateCostProject([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils));
                //RETURN ALL DATA FOR PROJECT
                CostProjectViewModel modalCostProject = new CostProjectViewModel();
                List<TypeOfDesign> typeOfDesign1 = new List<TypeOfDesign>();
                EntityContact entityContact1 = new EntityContact();
                List<ProjectUnit> ProjectUnit = new List<ProjectUnit>();
                List<CostProjectMilestone> CostProjectMilestone = new List<CostProjectMilestone>();

                //CustomerProject customer = new CustomerProject();

                string _project_id = Utils.ID;
                var CostProject = _unitOfWork.CostProjectRepository.Find(_project_id);

                var configCostProject = new AutoMapper.MapperConfiguration(m => m.CreateMap<CostProject, CostProjectViewModel>());
                var mapperCostProject = configCostProject.CreateMapper();
                modalCostProject = mapperCostProject.Map<CostProjectViewModel>(CostProject);

                typeOfDesign1 = _unitOfWork.TypeOfDesignRepository.FetchAllTypeOfDesignByProjectID(_project_id).ToList();
                entityContact1 = _unitOfWork.EntityContactRepository.FindByEntityID(_project_id);
                ProjectUnit = _unitOfWork.ProjectUnitRepository.FindByProjectID(_project_id).ToList();

                for (int i = 0; i < ProjectUnit.Count; i++)
                {
                    List<ProjectDesignType> projectDesignType = new List<ProjectDesignType>();
                    List<ProjectTags> projectTags = new List<ProjectTags>();

                    projectDesignType = _unitOfWork.ProjectDesignTypeRepository.GetProjectDesignTypeByUnitID(ProjectUnit[i].id).ToList();
                    projectTags = _unitOfWork.ProjectTagsRepository.GetProjectTagsByUnitID(ProjectUnit[i].id).ToList();

                    ProjectUnit[i].ProjectDesignType = projectDesignType;
                    ProjectUnit[i].ProjectTags = projectTags;
                }

                CostProjectMilestone = _unitOfWork.CostProjectMilestoneRepository.GetCostProjectMilestoneByProjectID(_project_id).ToList();

                for (int i = 0; i < CostProjectMilestone.Count; i++)
                {
                    List<CostProjectTask> costProjectTasks = new List<CostProjectTask>();
                    costProjectTasks = _unitOfWork.CostProjectTaskRepository.GetAllMilestoneTasksByMilestoneID(CostProjectMilestone[i].id).ToList();
                    CostProjectMilestone[i].CostProjectTask = costProjectTasks;

                }

                modalCostProject.TypeOfDesign = (typeOfDesign1);
                modalCostProject.EntityContact = entityContact1;
                modalCostProject.ProjectUnit = (ProjectUnit);
                modalCostProject.CostProjectMilestone = (CostProjectMilestone);

                return await System.Threading.Tasks.Task.FromResult<object>(JsonConvert.SerializeObject(modalCostProject, Formatting.Indented)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        #endregion CostProjects

        #region CostTypeOfDesign

        [HttpPost]
        [Route("AddTypeOfDesign")]
        public async Task<object> AddTypeOfDesign([FromBody] TypeOfDesignViewModel projectViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (projectViewModel == null)
                    throw new ArgumentNullException(nameof(projectViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<TypeOfDesignViewModel, TypeOfDesign>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<TypeOfDesign>(projectViewModel);

                modal.id = Guid.NewGuid().ToString();
                modal.created_date = _dateTime.ToString();
                modal.is_deleted = false;


                _unitOfWork.TypeOfDesignRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "TypeOfDesign saved successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FetchAllTypeOfDesignByOrgID")]
        public async Task<object> FetchAllTypeOfDesignByOrgID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var results = _unitOfWork.TypeOfDesignRepository.FetchAllTypeOfDesignByOrgID(Utils.ID);

                return await System.Threading.Tasks.Task.FromResult<object>(JsonConvert.SerializeObject(results, Formatting.Indented)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindByTypeOfDesignID")]
        public async Task<object> FindByTypeOfDesignID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {


                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var results = _unitOfWork.TypeOfDesignRepository.Find(Utils.ID);
                return await Task.FromResult<object>(results).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetAllTypeOfDesign")]
        public async Task<object> GetAllTypeOfDesign(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.TypeOfDesignRepository.All();
                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemoveTypeOfDesignByID")]
        public async Task<object> RemoveTypeOfDesignByID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                _unitOfWork.TypeOfDesignRepository.Remove(Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "TypeOfDesign removed successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdateTypeOfDesign")]
        public async Task<object> UpdateTypeOfDesign([FromBody] TypeOfDesignViewModel projectViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (projectViewModel == null)
                    throw new ArgumentNullException(nameof(projectViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<TypeOfDesignViewModel, TypeOfDesign>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<TypeOfDesign>(projectViewModel);
                modal.modified_date = _dateTime.ToString();

                _unitOfWork.TypeOfDesignRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "TypeOfDesign updated successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        #endregion CostTypeOfDesign

        #region CostSpecifiation

        [HttpPost]
        [Route("AddSpecifiation")]
        public async Task<object> AddSpecifiation([FromBody] SpecifiationViewModel projectViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (projectViewModel == null)
                    throw new ArgumentNullException(nameof(projectViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<SpecifiationViewModel, Specifiation>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Specifiation>(projectViewModel);

                modal.id = Guid.NewGuid().ToString();
                modal.created_date = _dateTime.ToString();
                modal.is_deleted = false;


                _unitOfWork.SpecifiationRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Specifiation saved successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FetchAllSpecifiationByOrgID")]
        public async Task<object> FetchAllSpecifiationByOrgID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var results = _unitOfWork.SpecifiationRepository.FetchAllSpecifiationByOrgID(Utils.ID);

                return await System.Threading.Tasks.Task.FromResult<object>(JsonConvert.SerializeObject(results, Formatting.Indented)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindBySpecifiationID")]
        public async Task<object> FindBySpecifiationID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {


                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var results = _unitOfWork.SpecifiationRepository.Find(Utils.ID);
                return await Task.FromResult<object>(results).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetAllSpecifiation")]
        public async Task<object> GetAllSpecifiation(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.SpecifiationRepository.All();
                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemoveSpecifiationByID")]
        public async Task<object> RemoveSpecifiationByID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                _unitOfWork.SpecifiationRepository.Remove(Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Specifiation removed successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdateSpecifiation")]
        public async Task<object> UpdateSpecifiation([FromBody] SpecifiationViewModel projectViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (projectViewModel == null)
                    throw new ArgumentNullException(nameof(projectViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<SpecifiationViewModel, Specifiation>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Specifiation>(projectViewModel);
                modal.modified_date = _dateTime.ToString();

                _unitOfWork.SpecifiationRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Specifiation updated successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        #endregion CostSpecifiation

        #region CostUnitDescription

        [HttpPost]
        [Route("AddUnitDescription")]
        public async Task<object> AddUnitDescription([FromBody] UnitDescriptionViewModel projectViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (projectViewModel == null)
                    throw new ArgumentNullException(nameof(projectViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<UnitDescriptionViewModel, UnitDescription>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<UnitDescription>(projectViewModel);

                modal.id = Guid.NewGuid().ToString();
                modal.created_date = _dateTime.ToString();
                modal.is_deleted = false;


                _unitOfWork.UnitDescriptionRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "UnitDescription saved successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FetchAllUnitDescriptionByOrgID")]
        public async Task<object> FetchAllUnitDescriptionByOrgID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var results = _unitOfWork.UnitDescriptionRepository.FetchAllUnitDescriptionByOrgID(Utils.ID);

                return await System.Threading.Tasks.Task.FromResult<object>(JsonConvert.SerializeObject(results, Formatting.Indented)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindByUnitDescriptionID")]
        public async Task<object> FindByUnitDescriptionID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {


                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var results = _unitOfWork.UnitDescriptionRepository.Find(Utils.ID);
                return await Task.FromResult<object>(results).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetAllUnitDescription")]
        public async Task<object> GetAllUnitDescription(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.UnitDescriptionRepository.All();
                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemoveUnitDescriptionByID")]
        public async Task<object> RemoveUnitDescriptionByID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                _unitOfWork.UnitDescriptionRepository.Remove(Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "UnitDescription removed successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdateUnitDescription")]
        public async Task<object> UpdateUnitDescription([FromBody] UnitDescriptionViewModel projectViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (projectViewModel == null)
                    throw new ArgumentNullException(nameof(projectViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<UnitDescriptionViewModel, UnitDescription>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<UnitDescription>(projectViewModel);
                modal.modified_date = _dateTime.ToString();

                _unitOfWork.UnitDescriptionRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "UnitDescription updated successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        #endregion CostUnitDescription




    }
}