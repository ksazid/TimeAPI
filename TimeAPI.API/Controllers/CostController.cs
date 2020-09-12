using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nancy.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TimeAPI.API.Models;
using TimeAPI.API.Models.CostPerHourViewModels;
using TimeAPI.API.Models.CostProjectViewModels;
using TimeAPI.API.Models.PackagesViewModels;
using TimeAPI.API.Models.ProfitMarginViewModels;
using TimeAPI.API.Models.SpecifiationViewModels;
using TimeAPI.API.Models.TypeOfDesignViewModels;
using TimeAPI.API.Models.UnitDescriptionViewModels;
using TimeAPI.API.Services;
using TimeAPI.Domain;
using TimeAPI.Domain.Entities;

namespace TimeAPI.API.Controllroers
{
    //[ApiKeyAuth]
    //[EnableCors("CorsPolicy")]
    [Route("[controller]")]
    //[Authorize(Roles = "superadmin")]
    public class CostController : Controller
    {
        private readonly ApplicationSettings _appSettings;
        private readonly DateTime _dateTime;
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly IUnitOfWork _unitOfWork;
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
                int cost_up = 0;
                int cost_up_study = 0;

                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (projectViewModel == null)
                    throw new ArgumentNullException(nameof(projectViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<CostProjectViewModel, CostProject>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<CostProject>(projectViewModel);

                modal.id = Guid.NewGuid().ToString();
                modal.project_status_id = _unitOfWork.ProjectStatusRepository.GetProjectStatusByOrgID("default")
                                            .Where(s => s.project_status_name.Equals("In Progress"))
                                            .Select(s => s.id)
                                            .FirstOrDefault();

                modal.created_date = _dateTime.ToString();
                modal.is_deleted = false;

                //ProjectDesignType
                if (projectViewModel.TypeOfDesign != null)
                    foreach (var ProjectDesignTypeID in projectViewModel.TypeOfDesign)
                    {
                        var projectDesignType = new ProjectDesignType()
                        {
                            id = Guid.NewGuid().ToString(),
                            org_id = modal.org_id,
                            project_id = modal.id,
                            unit_id = null,
                            design_type_id = ProjectDesignTypeID,
                            createdby = projectViewModel.createdby,
                            created_date = _dateTime.ToString(),
                            is_deleted = false
                        };
                        _unitOfWork.ProjectDesignTypeRepository.Add(projectDesignType);
                    }

                //contact
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
                            city = projectViewModel.EntityContact[i].city,
                            country = projectViewModel.EntityContact[i].country,
                            createdby = projectViewModel.createdby,
                            created_date = _dateTime.ToString(),
                            is_deleted = false,
                            is_primary = projectViewModel.EntityContact[i].is_primary
                        };
                        _unitOfWork.EntityContactRepository.Add(entityContact);
                    }
                }

                //customer
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

                #region RegionStatic

                //get all static milestone
                var StaticCostProjectMilestone = await _unitOfWork.CostProjectMilestoneRepository.GetAllStaticMilestoneByOrgID(modal.org_id).ConfigureAwait(false);
                DataTable tableCostProjectMilestone = new DataTable();
                DataTable tableCostMilestoneTask = StaticTasks(modal, StaticCostProjectMilestone, tableCostProjectMilestone);

                #endregion RegionStatic

                #region ProjectUnit

                if (projectViewModel.ProjectUnit != null)
                    if (cost_up == 0 && projectViewModel.ProjectUnit[0].unit_qty_all != null)
                    {
                        cost_up = Convert.ToInt32(projectViewModel.ProjectUnit[0].unit_qty_all);
                        cost_up_study = Convert.ToInt32(projectViewModel.ProjectUnit[0].unit_qty_all);
                    }

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
                            is_extra = item.is_extra,
                            createdby = projectViewModel.createdby,
                            created_date = _dateTime.ToString(),
                            is_deleted = false
                        };

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


                        //DEFAULT MILESTONE FOR ALL (Study)
                        if (projectUnit.is_extra == false)
                        {
                            #region Study

                            foreach (DataRow itemStaticCostProjectMilestone in tableCostProjectMilestone.Select("milestone_name = 'Study'"))
                            {
                                var StaticCostProjectMilestoneTasks = await _unitOfWork.CostProjectTaskRepository
                                                                       .GetAllStaticMilestoneTasksByMilestoneID
                                                                       (itemStaticCostProjectMilestone["milestone_id"].ToString(), modal.org_id).ConfigureAwait(false);

                                foreach (var itemStaticCostProjectMilestoneTasks in StaticCostProjectMilestoneTasks)
                                {

                                    double numberofunit = 0, floorOrUnit = 0;
                                    if (itemStaticCostProjectMilestoneTasks.unit == "Floors")
                                    {
                                        floorOrUnit = (Convert.ToDouble(projectViewModel.no_of_floors));
                                        numberofunit = (Convert.ToDouble(projectViewModel.no_of_floors) * Convert.ToDouble(itemStaticCostProjectMilestoneTasks.qty));
                                        if (Convert.ToDouble(cost_up) > 0)
                                        {
                                            var PercentValue = Math.Round(Convert.ToDouble(itemStaticCostProjectMilestoneTasks.qty) / 100 * Convert.ToDouble(cost_up), 2);
                                            numberofunit = ((Convert.ToDouble(PercentValue) + Convert.ToDouble(numberofunit)));
                                        }
                                    }
                                    else if (itemStaticCostProjectMilestoneTasks.unit == "Project")
                                    {
                                        floorOrUnit = (Convert.ToDouble(1));
                                        numberofunit = (Convert.ToDouble(1) * Convert.ToDouble(itemStaticCostProjectMilestoneTasks.qty));
                                        if (Convert.ToDouble(cost_up) > 0)
                                        {
                                            var PercentValue = Math.Round(Convert.ToDouble(itemStaticCostProjectMilestoneTasks.qty) / 100 * Convert.ToDouble(cost_up), 2);
                                            numberofunit = ((Convert.ToDouble(PercentValue) + Convert.ToDouble(numberofunit)));
                                        }
                                    }
                                    else
                                    {
                                        numberofunit = (Convert.ToDouble(1) * Convert.ToDouble(itemStaticCostProjectMilestoneTasks.qty));
                                        if (Convert.ToDouble(cost_up_study) > 0)
                                        {
                                            var PercentValue = Math.Round(Convert.ToDouble(numberofunit) / 100 * Convert.ToDouble(cost_up), 2);
                                            numberofunit = ((Convert.ToDouble(PercentValue) + Convert.ToDouble(numberofunit)));
                                        }
                                    }

                                    if (tableCostMilestoneTask.Rows.Count > 0)
                                    {
                                        if (tableCostMilestoneTask.AsEnumerable().Where(c => c.Field<string>("task_name")
                                                                .Equals(itemStaticCostProjectMilestoneTasks.task_name)).Any())
                                        {
                                            var qty = tableCostMilestoneTask.AsEnumerable()
                                                                            .Where(c => c.Field<string>("task_name") == itemStaticCostProjectMilestoneTasks.task_name)
                                                                            .Sum(x => x.Field<double>("qty"));

                                            if (numberofunit > 0)
                                                tableCostMilestoneTask.AsEnumerable()
                                                                                .Where(c => c.Field<string>("task_name") == itemStaticCostProjectMilestoneTasks.task_name)
                                                                                .ToList()
                                                                                .ForEach(D => D.SetField("qty", qty + Convert.ToDouble(numberofunit)));
                                        }
                                        else
                                        {
                                            tableCostMilestoneTask.Rows.Add(itemStaticCostProjectMilestone["id"],
                                               modal.id, itemStaticCostProjectMilestoneTasks.task_name,
                                               numberofunit, item.unit_id, itemStaticCostProjectMilestoneTasks.unit,
                                               projectViewModel.total_unit, itemStaticCostProjectMilestoneTasks.qty);
                                        }
                                    }
                                    else
                                    {
                                        tableCostMilestoneTask.Rows.Add(itemStaticCostProjectMilestone["id"],
                                           modal.id, itemStaticCostProjectMilestoneTasks.task_name,
                                           numberofunit, item.unit_id, itemStaticCostProjectMilestoneTasks.unit,
                                           projectViewModel.total_unit, itemStaticCostProjectMilestoneTasks.qty);
                                    }
                                }

                            }
                            cost_up_study = 0;

                            #endregion Study
                        }
                        if (projectUnit.is_extra == true)
                        {
                            #region Design

                            foreach (DataRow itemStaticCostProjectMilestone in tableCostProjectMilestone.Select("milestone_name = 'Design'"))
                            {
                                //get all static milestone tasks and save it with projectid in cost_milestone_tasks
                                var StaticCostProjectMilestoneTasks = await _unitOfWork.CostProjectTaskRepository
                                                                       .GetAllStaticMilestoneTasksByMilestoneID
                                                                       (itemStaticCostProjectMilestone["milestone_id"].ToString(), modal.org_id).ConfigureAwait(false);

                                foreach (var itemStaticCostProjectMilestoneTasks in StaticCostProjectMilestoneTasks)
                                {
                                    //convert the no_of_unit into interger.
                                    if (itemStaticCostProjectMilestoneTasks.task_name == item.unit_name)
                                    {
                                        double numberofunit = 0, floorOrUnit = 0;
                                        if (itemStaticCostProjectMilestoneTasks.unit == "Floors")
                                        {
                                            floorOrUnit = (Convert.ToDouble(projectViewModel.no_of_floors));
                                            numberofunit = (Convert.ToDouble(projectViewModel.no_of_floors) * Convert.ToDouble(itemStaticCostProjectMilestoneTasks.qty));
                                            if (Convert.ToDouble(cost_up) > 0)
                                            {
                                                var PercentValue = Math.Round(Convert.ToDouble(itemStaticCostProjectMilestoneTasks.qty) / 100 * Convert.ToDouble(cost_up), 2);
                                                numberofunit = ((Convert.ToDouble(PercentValue) + Convert.ToDouble(numberofunit)));
                                            }
                                        }
                                        else if (itemStaticCostProjectMilestoneTasks.unit == "Project")
                                        {
                                            floorOrUnit = (Convert.ToDouble(1));
                                            numberofunit = (Convert.ToDouble(1) * Convert.ToDouble(itemStaticCostProjectMilestoneTasks.qty));
                                            if (Convert.ToDouble(cost_up) > 0)
                                            {
                                                var PercentValue = Math.Round(Convert.ToDouble(itemStaticCostProjectMilestoneTasks.qty) / 100 * Convert.ToDouble(cost_up), 2);
                                                numberofunit = ((Convert.ToDouble(PercentValue) + Convert.ToDouble(numberofunit)));
                                            }
                                        }
                                        else
                                        {
                                            numberofunit = (Convert.ToDouble(item.total_unit) * Convert.ToDouble(itemStaticCostProjectMilestoneTasks.qty));
                                            if (Convert.ToDouble(cost_up) > 0)
                                            {
                                                var PercentValue = Math.Round(Convert.ToDouble(itemStaticCostProjectMilestoneTasks.qty) / 100 * Convert.ToDouble(cost_up), 2);
                                                numberofunit = ((Convert.ToDouble(PercentValue) + Convert.ToDouble(numberofunit)));
                                            }
                                        }
                                        if (tableCostMilestoneTask.Rows.Count > 0)
                                        {
                                            if (tableCostMilestoneTask.AsEnumerable().Where(c => c.Field<string>("task_name")
                                                                    .Equals(itemStaticCostProjectMilestoneTasks.task_name)).Any())
                                            {
                                                var qty = tableCostMilestoneTask.AsEnumerable()
                                                                                .Where(c => c.Field<string>("task_name") == itemStaticCostProjectMilestoneTasks.task_name)
                                                                                .Sum(x => x.Field<double>("qty"));

                                                if (numberofunit > 0)
                                                    tableCostMilestoneTask.AsEnumerable()
                                                                                    .Where(c => c.Field<string>("task_name") == itemStaticCostProjectMilestoneTasks.task_name)
                                                                                    .ToList()
                                                                                    .ForEach(D => D.SetField("qty", qty + Convert.ToDouble(numberofunit)));
                                            }
                                            else
                                            {
                                                tableCostMilestoneTask.Rows.Add(itemStaticCostProjectMilestone["id"],
                                                   modal.id, itemStaticCostProjectMilestoneTasks.task_name,
                                                   numberofunit, item.unit_id, itemStaticCostProjectMilestoneTasks.unit,
                                                   item.total_unit, itemStaticCostProjectMilestoneTasks.qty);
                                            }
                                        }
                                        else
                                        {
                                            tableCostMilestoneTask.Rows.Add(itemStaticCostProjectMilestone["id"],
                                               modal.id, itemStaticCostProjectMilestoneTasks.task_name,
                                               numberofunit, item.unit_id, itemStaticCostProjectMilestoneTasks.unit,
                                               item.total_unit, itemStaticCostProjectMilestoneTasks.qty);
                                        }
                                    }
                                }
                            }

                            #endregion Design

                            #region ExtraServices

                            foreach (DataRow itemStaticCostProjectMilestone in tableCostProjectMilestone.Select("milestone_name = 'Extra Services'"))
                            {
                                //get all static milestone tasks and save it with projectid in cost_milestone_tasks
                                var StaticCostProjectMilestoneTasks = await _unitOfWork.CostProjectTaskRepository
                                                                       .GetAllStaticMilestoneTasksByMilestoneID
                                                                       (itemStaticCostProjectMilestone["milestone_id"].ToString(), modal.org_id).ConfigureAwait(false);

                                foreach (var itemStaticCostProjectMilestoneTasks in StaticCostProjectMilestoneTasks)
                                {
                                    //convert the no_of_unit into interger.
                                    if (itemStaticCostProjectMilestoneTasks.task_name == item.unit_name)
                                    {
                                        string numberofunit = "";

                                        //if (itemStaticCostProjectMilestoneTasks.task_name == "BOQ")
                                        //{
                                        //    numberofunit = (Convert.ToDouble(item.total_unit) * Convert.ToDouble(itemStaticCostProjectMilestoneTasks.qty)).ToString();
                                        //    if (Convert.ToDouble(cost_up) > 0)
                                        //    {
                                        //        var PercentValue = Math.Round(Convert.ToDouble(itemStaticCostProjectMilestoneTasks.qty) / 100 * Convert.ToDouble(cost_up), 2);
                                        //        numberofunit = ((Convert.ToDouble(PercentValue) + Convert.ToDouble(numberofunit))).ToString();
                                        //    }
                                        //}
                                        //else
                                        //{
                                        numberofunit = (Convert.ToDouble(1) * Convert.ToDouble(itemStaticCostProjectMilestoneTasks.qty)).ToString();
                                        if (Convert.ToDouble(cost_up) > 0)
                                        {
                                            var PercentValue = Math.Round(Convert.ToDouble(itemStaticCostProjectMilestoneTasks.qty) / 100 * Convert.ToDouble(cost_up), 2);
                                            numberofunit = ((Convert.ToDouble(PercentValue) + Convert.ToDouble(numberofunit))).ToString();
                                        }
                                        //}
                                        if (tableCostMilestoneTask.Rows.Count > 0)
                                        {
                                            if (tableCostMilestoneTask.AsEnumerable().Where(c => c.Field<string>("task_name")
                                                                    .Equals(itemStaticCostProjectMilestoneTasks.task_name)).Any())
                                            {
                                                var qty = tableCostMilestoneTask.AsEnumerable()
                                                                                .Where(c => c.Field<string>("task_name") == itemStaticCostProjectMilestoneTasks.task_name)
                                                                                .Sum(x => x.Field<double>("qty"));

                                                if (numberofunit.Length > 0)
                                                    tableCostMilestoneTask.AsEnumerable()
                                                                                    .Where(c => c.Field<string>("task_name") == itemStaticCostProjectMilestoneTasks.task_name)
                                                                                    .ToList()
                                                                                    .ForEach(D => D.SetField("qty", qty + Convert.ToDouble(numberofunit)));
                                            }
                                            else
                                            {
                                                tableCostMilestoneTask.Rows.Add(itemStaticCostProjectMilestone["id"],
                                                   modal.id, itemStaticCostProjectMilestoneTasks.task_name,
                                                   numberofunit, item.unit_id, itemStaticCostProjectMilestoneTasks.unit,
                                                   item.total_unit, itemStaticCostProjectMilestoneTasks.qty);
                                            }
                                        }
                                        else
                                        {
                                            tableCostMilestoneTask.Rows.Add(itemStaticCostProjectMilestone["id"],
                                               modal.id, itemStaticCostProjectMilestoneTasks.task_name,
                                               numberofunit, item.unit_id, itemStaticCostProjectMilestoneTasks.unit,
                                               item.total_unit, itemStaticCostProjectMilestoneTasks.qty);
                                        }
                                    }
                                }
                            }

                            #endregion ExtraServices

                            #region ShopDrawing

                            if (item.unit_name == "Shop Drawing")
                            {
                                foreach (DataRow itemStaticCostProjectMilestone in tableCostProjectMilestone.Select("milestone_name = 'Shop Drawing'"))
                                {
                                    //get all static milestone tasks and save it with projectid in cost_milestone_tasks
                                    var StaticCostProjectMilestoneTasks = await _unitOfWork.CostProjectTaskRepository
                                                                           .GetAllStaticMilestoneTasksByMilestoneID
                                                                           (itemStaticCostProjectMilestone["milestone_id"].ToString(), modal.org_id).ConfigureAwait(false);

                                    foreach (var itemStaticCostProjectMilestoneTasks in StaticCostProjectMilestoneTasks)
                                    {
                                        //convert the no_of_unit into interger.
                                        double numberofunit = 0, floorOrUnit = 0;
                                        if (itemStaticCostProjectMilestoneTasks.unit == "Floors")
                                        {
                                            floorOrUnit = (Convert.ToDouble(projectViewModel.no_of_floors));
                                            numberofunit = (Convert.ToDouble(projectViewModel.no_of_floors) * Convert.ToDouble(itemStaticCostProjectMilestoneTasks.qty));
                                            if (Convert.ToDouble(cost_up) > 0)
                                            {
                                                var PercentValue = Math.Round(Convert.ToDouble(itemStaticCostProjectMilestoneTasks.qty) / 100 * Convert.ToDouble(cost_up), 2);
                                                numberofunit = ((Convert.ToDouble(PercentValue) + Convert.ToDouble(numberofunit)));
                                            }
                                        }
                                        else if (itemStaticCostProjectMilestoneTasks.unit == "Project")
                                        {
                                            floorOrUnit = (Convert.ToDouble(1));
                                            numberofunit = (Convert.ToDouble(1) * Convert.ToDouble(itemStaticCostProjectMilestoneTasks.qty));
                                            if (Convert.ToDouble(cost_up) > 0)
                                            {
                                                var PercentValue = Math.Round(Convert.ToDouble(itemStaticCostProjectMilestoneTasks.qty) / 100 * Convert.ToDouble(cost_up), 2);
                                                numberofunit = ((Convert.ToDouble(PercentValue) + Convert.ToDouble(numberofunit)));
                                            }
                                            //var totalMinutes = numberofunit;
                                            //var time = TimeSpan.FromMinutes(totalMinutes);
                                            //totalHrs = ("{0:00}:{1:00}", (int)time.TotalHours, time.Minutes).ToString();
                                        }
                                        else
                                        {
                                            floorOrUnit = (Convert.ToDouble(item.total_unit));
                                            numberofunit = (Convert.ToDouble(item.total_unit) * Convert.ToDouble(itemStaticCostProjectMilestoneTasks.qty));
                                            if (Convert.ToDouble(cost_up) > 0)
                                            {
                                                var PercentValue = Math.Round(Convert.ToDouble(itemStaticCostProjectMilestoneTasks.qty) / 100 * Convert.ToDouble(cost_up), 2);
                                                numberofunit = ((Convert.ToDouble(PercentValue) + Convert.ToDouble(numberofunit)));
                                            }
                                            //   var totalMinutes = numberofunit;
                                            //var time = TimeSpan.FromMinutes(totalMinutes);
                                            //totalHrs = ("{0:00}:{1:00}", (int)time.TotalHours, time.Minutes).ToString();
                                        }

                                        if (tableCostMilestoneTask.Rows.Count > 0)
                                        {
                                            if (tableCostMilestoneTask.AsEnumerable().Where(c => c.Field<string>("task_name")
                                                                    .Equals(itemStaticCostProjectMilestoneTasks.task_name)).Any())
                                            {
                                                var qty = tableCostMilestoneTask.AsEnumerable()
                                                                                .Where(c => c.Field<string>("task_name") == itemStaticCostProjectMilestoneTasks.task_name)
                                                                                .Sum(x => x.Field<double>("qty"));

                                                if (numberofunit > 0)
                                                    tableCostMilestoneTask.AsEnumerable()
                                                                                    .Where(c => c.Field<string>("task_name") == itemStaticCostProjectMilestoneTasks.task_name)
                                                                                    .ToList()
                                                                                    .ForEach(D => D.SetField("qty", qty + Convert.ToDouble(numberofunit)));
                                            }
                                            else
                                            {
                                                tableCostMilestoneTask.Rows.Add(itemStaticCostProjectMilestone["id"],
                                                   modal.id, itemStaticCostProjectMilestoneTasks.task_name,
                                                   numberofunit, item.unit_id, itemStaticCostProjectMilestoneTasks.unit,
                                                   floorOrUnit, itemStaticCostProjectMilestoneTasks.qty);
                                            }
                                        }
                                        else
                                        {
                                            tableCostMilestoneTask.Rows.Add(itemStaticCostProjectMilestone["id"],
                                               modal.id, itemStaticCostProjectMilestoneTasks.task_name,
                                               numberofunit, item.unit_id, itemStaticCostProjectMilestoneTasks.unit,
                                               floorOrUnit, itemStaticCostProjectMilestoneTasks.qty);
                                        }
                                    }
                                }

                                cost_up = 0;
                            }

                            #endregion ShopDrawing
                        }

                        _unitOfWork.ProjectUnitRepository.Add(projectUnit);
                    }

                #endregion ProjectUnit

                #region ProjectMilestone&Task

                foreach (DataRow itemStaticCostProjectMilestone in tableCostProjectMilestone.Rows)
                {
                    var _CostProjectMilestone = new CostProjectMilestone()
                    {
                        id = itemStaticCostProjectMilestone["id"].ToString(),
                        project_id = modal.id,
                        milestone_name = itemStaticCostProjectMilestone["milestone_name"].ToString(),
                        alias_name = itemStaticCostProjectMilestone["alias_name"].ToString(),
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
                        unit_type = itemtableCostMilestoneTask["unit_type"].ToString(),
                        total_unit = itemtableCostMilestoneTask["total_unit"].ToString(),
                        default_unit_hours = itemtableCostMilestoneTask["default_unit_hours"].ToString(),
                        createdby = projectViewModel.createdby,
                        created_date = _dateTime.ToString(),
                        is_deleted = false
                    };
                    _unitOfWork.CostProjectTaskRepository.Add(CostProjectMilestoneTask);
                }

                #endregion ProjectMilestone&Task

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
                CostProjectResponseViewModel modalCostProject = new CostProjectResponseViewModel();
                List<string> typeOfDesign1 = new List<string>();
                List<EntityContact> entityContact1 = new List<EntityContact>();
                List<ProjectUnit> ProjectUnit = new List<ProjectUnit>();
                List<ProjectUnit> ProjectUnitExtra = new List<ProjectUnit>();
                List<CostProjectMilestone> CostProjectMilestone = new List<CostProjectMilestone>();

                //CustomerProject customer = new CustomerProject();

                string _project_id = Utils.ID;
                var CostProject = await _unitOfWork.CostProjectRepository.FindByCostProjectID(_project_id).ConfigureAwait(false);

                var configCostProject = new AutoMapper.MapperConfiguration(m => m.CreateMap<CostProject, CostProjectResponseViewModel>());
                var mapperCostProject = configCostProject.CreateMapper();
                modalCostProject = mapperCostProject.Map<CostProjectResponseViewModel>(CostProject);

                typeOfDesign1 = (await _unitOfWork.ProjectDesignTypeRepository.GetProjectDesignTypeByProjectID(_project_id).ConfigureAwait(false))
                                                                        .Select(x => x.design_type_id).ToList();
                entityContact1 = (await _unitOfWork.EntityContactRepository.FindByEntityListID(_project_id).ConfigureAwait(false)).ToList();
                ProjectUnit = (await _unitOfWork.ProjectUnitRepository.FindByProjectID(_project_id).ConfigureAwait(false)).ToList();
                var Customer = await _unitOfWork.CustomerProjectRepository.FindByProjectID(_project_id).ConfigureAwait(false);

                if (Customer != null)
                    modalCostProject.cst_id = Customer.cst_id;


                for (int i = 0; i < ProjectUnit.Count; i++)
                {
                    List<string> projectDesignType = new List<string>();
                    List<ProjectTags> projectTags = new List<ProjectTags>();

                    projectDesignType = (await _unitOfWork.ProjectDesignTypeRepository.GetProjectDesignTypeByUnitID(ProjectUnit[i].id).ConfigureAwait(false))
                                                                                        .Select(x => x.design_type_id).ToList();
                    projectTags = (await _unitOfWork.ProjectTagsRepository.GetProjectTagsByUnitID(ProjectUnit[i].id).ConfigureAwait(false)).ToList();

                    ProjectUnit[i].ProjectDesignType_ID = projectDesignType;
                    ProjectUnit[i].ProjectTags = projectTags;
                }

                CostProjectMilestone = (await _unitOfWork.CostProjectMilestoneRepository.GetCostProjectMilestoneByProjectID(_project_id).ConfigureAwait(false)).ToList();

                for (int i = 0; i < CostProjectMilestone.Count; i++)
                {
                    List<CostProjectTask> costProjectTasks = new List<CostProjectTask>();
                    costProjectTasks = (await _unitOfWork.CostProjectTaskRepository.GetAllMilestoneTasksByMilestoneID(CostProjectMilestone[i].id, CostProjectMilestone[i].org_id).ConfigureAwait(false)).ToList();
                    CostProjectMilestone[i].CostProjectTask = costProjectTasks;
                }

                if (typeOfDesign1.Count > 0)
                    modalCostProject.TypeOfDesign = (typeOfDesign1);
                if (entityContact1.Count > 0)
                    modalCostProject.EntityContact = entityContact1;
                if (ProjectUnit.Count > 0)
                    modalCostProject.ProjectUnit = (ProjectUnit.Where(x => !x.is_extra).ToList());
                if (ProjectUnit.Count > 0)
                    modalCostProject.ProjectUnitExtra = (ProjectUnit.Where(x => x.is_extra).ToList());
                if (CostProjectMilestone.Count > 0)
                    modalCostProject.CostProjectMilestone = (CostProjectMilestone);

                return await System.Threading.Tasks.Task.FromResult<object>(JsonConvert.SerializeObject(modalCostProject, Formatting.Indented)).ConfigureAwait(false);
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
                var results = await _unitOfWork.CostProjectRepository.FetchAllCostProjectByOrgID(Utils.ID).ConfigureAwait(false);
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

                var results = await _unitOfWork.CostProjectRepository.Find(Utils.ID).ConfigureAwait(false);
                //var resultLocation = _unitOfWork.EntityLocationRepository.FindByEnitiyID(results.id);
                var resultContact = await _unitOfWork.EntityContactRepository.FindByEntityListID(results.id).ConfigureAwait(false);
                var resultCustomer = await _unitOfWork.CustomerRepository.FindCustomerByProjectID(results.id).ConfigureAwait(false);

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<CostProjectDetailViewModel, CostProject>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<CostProject>(results);

                //modal.EntityLocation = resultLocation;
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
        [Route("GetAllCostProject")]
        public async Task<object> GetAllCostProject(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = await _unitOfWork.CostProjectRepository.All().ConfigureAwait(false);
                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("GetLastAddedCostPrefixByOrgID")]
        public async Task<object> GetLastAddedCostPrefixByOrgID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = await _unitOfWork.CostProjectRepository.GetLastAddedCostPrefixByOrgID(Utils.ID).ConfigureAwait(false);
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
        [Route("GetMilestoneAndTasksByProjectID")]
        public async Task<object> GetMilestoneAndTasksByProjectID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils));

                List<CostProjectMilestone> CostProjectMilestone = new List<CostProjectMilestone>();
                CostProjectMilestone = (await _unitOfWork.CostProjectMilestoneRepository.GetCostProjectMilestoneByProjectID(Utils.ID).ConfigureAwait(false)).ToList();

                for (int i = 0; i < CostProjectMilestone.Count; i++)
                {
                    List<CostProjectTask> costProjectTasks = new List<CostProjectTask>();
                    costProjectTasks = (await _unitOfWork.CostProjectTaskRepository.GetAllMilestoneTasksByMilestoneID(CostProjectMilestone[i].id, CostProjectMilestone[i].org_id).ConfigureAwait(false)).ToList();
                    CostProjectMilestone[i].CostProjectTask = costProjectTasks;
                }

                return await System.Threading.Tasks.Task.FromResult<object>(JsonConvert.SerializeObject(CostProjectMilestone, Formatting.Indented)).ConfigureAwait(false);
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
                await _unitOfWork.EntityContactRepository.RemoveByEntityID(Utils.ID).ConfigureAwait(false);
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

        [HttpPost]
        [Route("RevisedQuotation")]
        public async Task<object> RevisedQuotation([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils));

                //RETURN ALL DATA FOR PROJECT
                CostProject modalCostProject = new CostProject();
                List<ProjectDesignType> ProjectDesignTypeList = new List<ProjectDesignType>();
                List<EntityContact> entityContact1 = new List<EntityContact>();
                List<ProjectUnit> ProjectUnit = new List<ProjectUnit>();
                List<ProjectUnit> ProjectUnitExtra = new List<ProjectUnit>();
                List<CostProjectMilestone> CostProjectMilestone = new List<CostProjectMilestone>();

                string _project_id = Utils.ID;
                var CostProject = await _unitOfWork.CostProjectRepository.Find(_project_id).ConfigureAwait(false);

                CostProject.id = Guid.NewGuid().ToString();
                CostProject.created_date = _dateTime.ToString();
                CostProject.is_deleted = false;

                var Customer = await _unitOfWork.CustomerProjectRepository.FindByProjectID(_project_id).ConfigureAwait(false);

                //customer_x_project
                if (Customer.id != null)
                {
                    var customerCostProject = new CustomerProject()
                    {
                        id = Guid.NewGuid().ToString(),
                        cst_id = Customer.cst_id,
                        project_id = CostProject.id,
                        createdby = CostProject.createdby,
                        created_date = _dateTime.ToString(),
                        is_deleted = false
                    };
                    _unitOfWork.CustomerProjectRepository.Add(customerCostProject);
                }

                ProjectDesignTypeList = (await _unitOfWork.ProjectDesignTypeRepository.GetProjectDesignTypeByProjectID(_project_id).ConfigureAwait(false)).ToList();
                foreach (var item in ProjectDesignTypeList)
                {
                    item.id = Guid.NewGuid().ToString();
                    item.created_date = _dateTime.ToString();
                    item.is_deleted = false;
                    item.project_id = CostProject.id;
                    _unitOfWork.ProjectDesignTypeRepository.Add(item);
                }

                entityContact1 = (await _unitOfWork.EntityContactRepository.FindByEntityListID(_project_id).ConfigureAwait(false)).ToList();
                foreach (var item in entityContact1)
                {
                    item.id = Guid.NewGuid().ToString();
                    item.created_date = _dateTime.ToString();
                    item.is_deleted = false;
                    item.entity_id = CostProject.id;
                    _unitOfWork.EntityContactRepository.Add(item);
                }

                ProjectUnit = (await _unitOfWork.ProjectUnitRepository.FindByProjectID(_project_id).ConfigureAwait(false)).ToList();
                foreach (var item in ProjectUnit)
                {
                    List<ProjectTags> projectTags = new List<ProjectTags>();

                    projectTags = (await _unitOfWork.ProjectTagsRepository.GetProjectTagsByUnitID(item.id).ConfigureAwait(false)).ToList();

                    item.id = Guid.NewGuid().ToString();
                    item.created_date = _dateTime.ToString();
                    item.is_deleted = false;
                    item.project_id = CostProject.id;

                    foreach (var itemProjectTags in projectTags)
                    {
                        var projectTagsx = new ProjectTags()
                        {
                            id = Guid.NewGuid().ToString(),
                            project_id = CostProject.id,
                            unit_id = item.id,
                            tags = itemProjectTags.tags,
                            createdby = itemProjectTags.createdby,
                            created_date = _dateTime.ToString(),
                            is_deleted = false
                        };
                        _unitOfWork.ProjectTagsRepository.Add(projectTagsx);
                    }

                    _unitOfWork.ProjectUnitRepository.Add(item);
                }

                CostProjectMilestone = (await _unitOfWork.CostProjectMilestoneRepository.GetCostProjectMilestoneByProjectID(_project_id).ConfigureAwait(false)).ToList();
                foreach (var item in CostProjectMilestone)
                {
                    List<CostProjectTask> costProjectTasks = new List<CostProjectTask>();
                    costProjectTasks = (await _unitOfWork.CostProjectTaskRepository.GetAllMilestoneTasksByMilestoneID(item.id, item.org_id).ConfigureAwait(false)).ToList();

                    item.id = Guid.NewGuid().ToString();
                    item.created_date = _dateTime.ToString();
                    item.is_deleted = false;
                    item.project_id = CostProject.id;

                    foreach (var itemx in costProjectTasks)
                    {
                        itemx.id = Guid.NewGuid().ToString();
                        itemx.created_date = _dateTime.ToString();
                        itemx.is_deleted = false;
                        itemx.project_id = CostProject.id;
                        itemx.milestone_id = item.id;

                        _unitOfWork.CostProjectTaskRepository.Add(itemx);
                    }

                    _unitOfWork.CostProjectMilestoneRepository.Add(item);
                }

                _project_id = CostProject.id;
                _unitOfWork.CostProjectRepository.Add(CostProject);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new Utils { ID = _project_id }).ConfigureAwait(false);
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
                int cost_up = 0;
                int cost_up_study = 0;
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (projectViewModel == null)
                    throw new ArgumentNullException(nameof(projectViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<CostProjectViewModel, CostProject>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<CostProject>(projectViewModel);
                modal.modified_date = _dateTime.ToString();

                await _unitOfWork.EntityContactRepository.RemoveByEntityID(modal.id).ConfigureAwait(false);
                //contact
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
                            city = projectViewModel.EntityContact[i].city,
                            country = projectViewModel.EntityContact[i].country,
                            createdby = projectViewModel.createdby,
                            created_date = _dateTime.ToString(),
                            is_deleted = false,
                            is_primary = projectViewModel.EntityContact[i].is_primary
                        };
                        _unitOfWork.EntityContactRepository.Add(entityContact);
                    }
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

                //ProjectDesignType
                var _ProjectDesignType = (await _unitOfWork.ProjectDesignTypeRepository.GetProjectDesignTypeByProjectID(modal.id).ConfigureAwait(false)).ToList();
                //.Select(x => x.design_type_id).ToList();

                if ((_ProjectDesignType.Count > 0))
                    await _unitOfWork.ProjectDesignTypeRepository.RemoveByProjectID(modal.id).ConfigureAwait(false);

                if (projectViewModel.TypeOfDesign != null)
                    foreach (var itemProjectDesignTypeID in projectViewModel.TypeOfDesign)
                    {
                        var projectDesignType = new ProjectDesignType()
                        {
                            id = Guid.NewGuid().ToString(),
                            org_id = modal.org_id,
                            project_id = modal.id,
                            unit_id = null,
                            design_type_id = itemProjectDesignTypeID,
                            createdby = projectViewModel.createdby,
                            created_date = _dateTime.ToString(),
                            is_deleted = false
                        };
                        _unitOfWork.ProjectDesignTypeRepository.Add(projectDesignType);
                    }

                #region RegionStatic

                //get all static milestone
                var StaticCostProjectMilestone = await _unitOfWork.CostProjectMilestoneRepository.GetAllStaticMilestoneByOrgID(modal.org_id).ConfigureAwait(false);
                DataTable tableCostProjectMilestone = new DataTable();
                DataTable tableCostMilestoneTask = StaticTasks(modal, StaticCostProjectMilestone, tableCostProjectMilestone);

                #endregion RegionStatic

                await _unitOfWork.ProjectUnitRepository.RemoveByProjectID(modal.id).ConfigureAwait(false);

                if (projectViewModel.ProjectUnit != null)
                    if (cost_up == 0 && projectViewModel.ProjectUnit[0].unit_qty_all != null)
                    {
                        cost_up = Convert.ToInt32(projectViewModel.ProjectUnit[0].unit_qty_all);
                        cost_up_study = Convert.ToInt32(projectViewModel.ProjectUnit[0].unit_qty_all);
                    }


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
                            is_extra = item.is_extra,
                            createdby = projectViewModel.createdby,
                            created_date = _dateTime.ToString(),
                            is_deleted = false
                        };

                        //tags
                        var _projectTags = (await _unitOfWork.ProjectTagsRepository.GetProjectTagsByUnitID(projectUnit.id).ConfigureAwait(false)).ToList();

                        if ((_ProjectDesignType.Count > 0))
                            await _unitOfWork.ProjectTagsRepository.RemoveByUnitID(projectUnit.id).ConfigureAwait(false);

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

                        //DEFAULT MILESTONE FOR ALL (Study)
                        if (projectUnit.is_extra == false)
                        {
                            #region Study

                            foreach (DataRow itemStaticCostProjectMilestone in tableCostProjectMilestone.Select("milestone_name = 'Study'"))
                            {
                                var StaticCostProjectMilestoneTasks = await _unitOfWork.CostProjectTaskRepository
                                                                       .GetAllStaticMilestoneTasksByMilestoneID
                                                                       (itemStaticCostProjectMilestone["milestone_id"].ToString(), modal.org_id).ConfigureAwait(false);

                                foreach (var itemStaticCostProjectMilestoneTasks in StaticCostProjectMilestoneTasks)
                                {

                                    double numberofunit = 0, floorOrUnit = 0;
                                    if (itemStaticCostProjectMilestoneTasks.unit == "Floors")
                                    {
                                        floorOrUnit = (Convert.ToDouble(projectViewModel.no_of_floors));
                                        numberofunit = (Convert.ToDouble(projectViewModel.no_of_floors) * Convert.ToDouble(itemStaticCostProjectMilestoneTasks.qty));
                                        if (Convert.ToDouble(cost_up) > 0)
                                        {
                                            var PercentValue = Math.Round(Convert.ToDouble(itemStaticCostProjectMilestoneTasks.qty) / 100 * Convert.ToDouble(cost_up), 2);
                                            numberofunit = ((Convert.ToDouble(PercentValue) + Convert.ToDouble(numberofunit)));
                                        }
                                    }
                                    else if (itemStaticCostProjectMilestoneTasks.unit == "Project")
                                    {
                                        floorOrUnit = (Convert.ToDouble(1));
                                        numberofunit = (Convert.ToDouble(1) * Convert.ToDouble(itemStaticCostProjectMilestoneTasks.qty));
                                        if (Convert.ToDouble(cost_up) > 0)
                                        {
                                            var PercentValue = Math.Round(Convert.ToDouble(itemStaticCostProjectMilestoneTasks.qty) / 100 * Convert.ToDouble(cost_up), 2);
                                            numberofunit = ((Convert.ToDouble(PercentValue) + Convert.ToDouble(numberofunit)));
                                        }
                                    }
                                    else
                                    {
                                        numberofunit = (Convert.ToDouble(1) * Convert.ToDouble(itemStaticCostProjectMilestoneTasks.qty));
                                        if (Convert.ToDouble(cost_up_study) > 0)
                                        {
                                            var PercentValue = Math.Round(Convert.ToDouble(numberofunit) / 100 * Convert.ToDouble(cost_up), 2);
                                            numberofunit = ((Convert.ToDouble(PercentValue) + Convert.ToDouble(numberofunit)));
                                        }
                                    }

                                    if (tableCostMilestoneTask.Rows.Count > 0)
                                    {
                                        if (tableCostMilestoneTask.AsEnumerable().Where(c => c.Field<string>("task_name")
                                                                .Equals(itemStaticCostProjectMilestoneTasks.task_name)).Any())
                                        {
                                            var qty = tableCostMilestoneTask.AsEnumerable()
                                                                            .Where(c => c.Field<string>("task_name") == itemStaticCostProjectMilestoneTasks.task_name)
                                                                            .Sum(x => x.Field<double>("qty"));

                                            if (numberofunit > 0)
                                                tableCostMilestoneTask.AsEnumerable()
                                                                                .Where(c => c.Field<string>("task_name") == itemStaticCostProjectMilestoneTasks.task_name)
                                                                                .ToList()
                                                                                .ForEach(D => D.SetField("qty", qty + Convert.ToDouble(numberofunit)));
                                        }
                                        else
                                        {
                                            tableCostMilestoneTask.Rows.Add(itemStaticCostProjectMilestone["id"],
                                               modal.id, itemStaticCostProjectMilestoneTasks.task_name,
                                               numberofunit, item.unit_id, itemStaticCostProjectMilestoneTasks.unit,
                                               projectViewModel.total_unit, itemStaticCostProjectMilestoneTasks.qty);
                                        }
                                    }
                                    else
                                    {
                                        tableCostMilestoneTask.Rows.Add(itemStaticCostProjectMilestone["id"],
                                           modal.id, itemStaticCostProjectMilestoneTasks.task_name,
                                           numberofunit, item.unit_id, itemStaticCostProjectMilestoneTasks.unit,
                                           projectViewModel.total_unit, itemStaticCostProjectMilestoneTasks.qty);
                                    }
                                }

                            }
                            cost_up_study = 0;

                            #endregion Study
                        }
                        if (projectUnit.is_extra == true)
                        {
                            #region Design

                            foreach (DataRow itemStaticCostProjectMilestone in tableCostProjectMilestone.Select("milestone_name = 'Design'"))
                            {
                                //get all static milestone tasks and save it with projectid in cost_milestone_tasks
                                var StaticCostProjectMilestoneTasks = await _unitOfWork.CostProjectTaskRepository
                                                                       .GetAllStaticMilestoneTasksByMilestoneID
                                                                       (itemStaticCostProjectMilestone["milestone_id"].ToString(), modal.org_id).ConfigureAwait(false);

                                foreach (var itemStaticCostProjectMilestoneTasks in StaticCostProjectMilestoneTasks)
                                {
                                    //convert the no_of_unit into interger.
                                    if (itemStaticCostProjectMilestoneTasks.task_name == item.unit_name)
                                    {
                                        double numberofunit = 0, floorOrUnit = 0;
                                        if (itemStaticCostProjectMilestoneTasks.unit == "Floors")
                                        {
                                            floorOrUnit = (Convert.ToDouble(projectViewModel.no_of_floors));
                                            numberofunit = (Convert.ToDouble(projectViewModel.no_of_floors) * Convert.ToDouble(itemStaticCostProjectMilestoneTasks.qty));
                                            if (Convert.ToDouble(cost_up) > 0)
                                            {
                                                var PercentValue = Math.Round(Convert.ToDouble(itemStaticCostProjectMilestoneTasks.qty) / 100 * Convert.ToDouble(cost_up), 2);
                                                numberofunit = ((Convert.ToDouble(PercentValue) + Convert.ToDouble(numberofunit)));
                                            }
                                        }
                                        else if (itemStaticCostProjectMilestoneTasks.unit == "Project")
                                        {
                                            floorOrUnit = (Convert.ToDouble(1));
                                            numberofunit = (Convert.ToDouble(1) * Convert.ToDouble(itemStaticCostProjectMilestoneTasks.qty));
                                            if (Convert.ToDouble(cost_up) > 0)
                                            {
                                                var PercentValue = Math.Round(Convert.ToDouble(itemStaticCostProjectMilestoneTasks.qty) / 100 * Convert.ToDouble(cost_up), 2);
                                                numberofunit = ((Convert.ToDouble(PercentValue) + Convert.ToDouble(numberofunit)));
                                            }
                                        }
                                        else
                                        {
                                            numberofunit = (Convert.ToDouble(item.total_unit) * Convert.ToDouble(itemStaticCostProjectMilestoneTasks.qty));
                                            if (Convert.ToDouble(cost_up) > 0)
                                            {
                                                var PercentValue = Math.Round(Convert.ToDouble(itemStaticCostProjectMilestoneTasks.qty) / 100 * Convert.ToDouble(cost_up), 2);
                                                numberofunit = ((Convert.ToDouble(PercentValue) + Convert.ToDouble(numberofunit)));
                                            }
                                        }
                                        if (tableCostMilestoneTask.Rows.Count > 0)
                                        {
                                            if (tableCostMilestoneTask.AsEnumerable().Where(c => c.Field<string>("task_name")
                                                                    .Equals(itemStaticCostProjectMilestoneTasks.task_name)).Any())
                                            {
                                                var qty = tableCostMilestoneTask.AsEnumerable()
                                                                                .Where(c => c.Field<string>("task_name") == itemStaticCostProjectMilestoneTasks.task_name)
                                                                                .Sum(x => x.Field<double>("qty"));

                                                if (numberofunit > 0)
                                                    tableCostMilestoneTask.AsEnumerable()
                                                                                    .Where(c => c.Field<string>("task_name") == itemStaticCostProjectMilestoneTasks.task_name)
                                                                                    .ToList()
                                                                                    .ForEach(D => D.SetField("qty", qty + Convert.ToDouble(numberofunit)));
                                            }
                                            else
                                            {
                                                tableCostMilestoneTask.Rows.Add(itemStaticCostProjectMilestone["id"],
                                                   modal.id, itemStaticCostProjectMilestoneTasks.task_name,
                                                   numberofunit, item.unit_id, itemStaticCostProjectMilestoneTasks.unit,
                                                   item.total_unit, itemStaticCostProjectMilestoneTasks.qty);
                                            }
                                        }
                                        else
                                        {
                                            tableCostMilestoneTask.Rows.Add(itemStaticCostProjectMilestone["id"],
                                               modal.id, itemStaticCostProjectMilestoneTasks.task_name,
                                               numberofunit, item.unit_id, itemStaticCostProjectMilestoneTasks.unit,
                                               item.total_unit, itemStaticCostProjectMilestoneTasks.qty);
                                        }
                                    }
                                }
                            }

                            #endregion Design

                            #region ExtraServices

                            foreach (DataRow itemStaticCostProjectMilestone in tableCostProjectMilestone.Select("milestone_name = 'Extra Services'"))
                            {
                                //get all static milestone tasks and save it with projectid in cost_milestone_tasks
                                var StaticCostProjectMilestoneTasks = await _unitOfWork.CostProjectTaskRepository
                                                                       .GetAllStaticMilestoneTasksByMilestoneID
                                                                       (itemStaticCostProjectMilestone["milestone_id"].ToString(), modal.org_id).ConfigureAwait(false);

                                foreach (var itemStaticCostProjectMilestoneTasks in StaticCostProjectMilestoneTasks)
                                {
                                    //convert the no_of_unit into interger.
                                    if (itemStaticCostProjectMilestoneTasks.task_name == item.unit_name)
                                    {
                                        string numberofunit = "";

                                        //if (itemStaticCostProjectMilestoneTasks.task_name == "BOQ")
                                        //{
                                        //    numberofunit = (Convert.ToDouble(item.total_unit) * Convert.ToDouble(itemStaticCostProjectMilestoneTasks.qty)).ToString();
                                        //    if (Convert.ToDouble(cost_up) > 0)
                                        //    {
                                        //        var PercentValue = Math.Round(Convert.ToDouble(itemStaticCostProjectMilestoneTasks.qty) / 100 * Convert.ToDouble(cost_up), 2);
                                        //        numberofunit = ((Convert.ToDouble(PercentValue) + Convert.ToDouble(numberofunit))).ToString();
                                        //    }
                                        //}
                                        //else
                                        //{
                                        numberofunit = (Convert.ToDouble(1) * Convert.ToDouble(itemStaticCostProjectMilestoneTasks.qty)).ToString();
                                        if (Convert.ToDouble(cost_up) > 0)
                                        {
                                            var PercentValue = Math.Round(Convert.ToDouble(itemStaticCostProjectMilestoneTasks.qty) / 100 * Convert.ToDouble(cost_up), 2);
                                            numberofunit = ((Convert.ToDouble(PercentValue) + Convert.ToDouble(numberofunit))).ToString();
                                        }
                                        //}
                                        if (tableCostMilestoneTask.Rows.Count > 0)
                                        {
                                            if (tableCostMilestoneTask.AsEnumerable().Where(c => c.Field<string>("task_name")
                                                                    .Equals(itemStaticCostProjectMilestoneTasks.task_name)).Any())
                                            {
                                                var qty = tableCostMilestoneTask.AsEnumerable()
                                                                                .Where(c => c.Field<string>("task_name") == itemStaticCostProjectMilestoneTasks.task_name)
                                                                                .Sum(x => x.Field<double>("qty"));

                                                if (numberofunit.Length > 0)
                                                    tableCostMilestoneTask.AsEnumerable()
                                                                                    .Where(c => c.Field<string>("task_name") == itemStaticCostProjectMilestoneTasks.task_name)
                                                                                    .ToList()
                                                                                    .ForEach(D => D.SetField("qty", qty + Convert.ToDouble(numberofunit)));
                                            }
                                            else
                                            {
                                                tableCostMilestoneTask.Rows.Add(itemStaticCostProjectMilestone["id"],
                                                   modal.id, itemStaticCostProjectMilestoneTasks.task_name,
                                                   numberofunit, item.unit_id, itemStaticCostProjectMilestoneTasks.unit,
                                                   item.total_unit, itemStaticCostProjectMilestoneTasks.qty);
                                            }
                                        }
                                        else
                                        {
                                            tableCostMilestoneTask.Rows.Add(itemStaticCostProjectMilestone["id"],
                                               modal.id, itemStaticCostProjectMilestoneTasks.task_name,
                                               numberofunit, item.unit_id, itemStaticCostProjectMilestoneTasks.unit,
                                               item.total_unit, itemStaticCostProjectMilestoneTasks.qty);
                                        }
                                    }
                                }
                            }

                            #endregion ExtraServices

                            #region ShopDrawing

                            if (item.unit_name == "Shop Drawing")
                            {
                                foreach (DataRow itemStaticCostProjectMilestone in tableCostProjectMilestone.Select("milestone_name = 'Shop Drawing'"))
                                {
                                    //get all static milestone tasks and save it with projectid in cost_milestone_tasks
                                    var StaticCostProjectMilestoneTasks = await _unitOfWork.CostProjectTaskRepository
                                                                           .GetAllStaticMilestoneTasksByMilestoneID
                                                                           (itemStaticCostProjectMilestone["milestone_id"].ToString(), modal.org_id).ConfigureAwait(false);

                                    foreach (var itemStaticCostProjectMilestoneTasks in StaticCostProjectMilestoneTasks)
                                    {
                                        //convert the no_of_unit into interger.
                                        double numberofunit = 0, floorOrUnit = 0;
                                        if (itemStaticCostProjectMilestoneTasks.unit == "Floors")
                                        {
                                            floorOrUnit = (Convert.ToDouble(projectViewModel.no_of_floors));
                                            numberofunit = (Convert.ToDouble(projectViewModel.no_of_floors) * Convert.ToDouble(itemStaticCostProjectMilestoneTasks.qty));
                                            if (Convert.ToDouble(cost_up) > 0)
                                            {
                                                var PercentValue = Math.Round(Convert.ToDouble(itemStaticCostProjectMilestoneTasks.qty) / 100 * Convert.ToDouble(cost_up), 2);
                                                numberofunit = ((Convert.ToDouble(PercentValue) + Convert.ToDouble(numberofunit)));
                                            }
                                        }
                                        else if (itemStaticCostProjectMilestoneTasks.unit == "Project")
                                        {
                                            floorOrUnit = (Convert.ToDouble(1));
                                            numberofunit = (Convert.ToDouble(1) * Convert.ToDouble(itemStaticCostProjectMilestoneTasks.qty));
                                            if (Convert.ToDouble(cost_up) > 0)
                                            {
                                                var PercentValue = Math.Round(Convert.ToDouble(itemStaticCostProjectMilestoneTasks.qty) / 100 * Convert.ToDouble(cost_up), 2);
                                                numberofunit = ((Convert.ToDouble(PercentValue) + Convert.ToDouble(numberofunit)));
                                            }
                                            //var totalMinutes = numberofunit;
                                            //var time = TimeSpan.FromMinutes(totalMinutes);
                                            //totalHrs = ("{0:00}:{1:00}", (int)time.TotalHours, time.Minutes).ToString();
                                        }
                                        else
                                        {
                                            floorOrUnit = (Convert.ToDouble(item.total_unit));
                                            numberofunit = (Convert.ToDouble(item.total_unit) * Convert.ToDouble(itemStaticCostProjectMilestoneTasks.qty));
                                            if (Convert.ToDouble(cost_up) > 0)
                                            {
                                                var PercentValue = Math.Round(Convert.ToDouble(itemStaticCostProjectMilestoneTasks.qty) / 100 * Convert.ToDouble(cost_up), 2);
                                                numberofunit = ((Convert.ToDouble(PercentValue) + Convert.ToDouble(numberofunit)));
                                            }
                                            //   var totalMinutes = numberofunit;
                                            //var time = TimeSpan.FromMinutes(totalMinutes);
                                            //totalHrs = ("{0:00}:{1:00}", (int)time.TotalHours, time.Minutes).ToString();
                                        }

                                        if (tableCostMilestoneTask.Rows.Count > 0)
                                        {
                                            if (tableCostMilestoneTask.AsEnumerable().Where(c => c.Field<string>("task_name")
                                                                    .Equals(itemStaticCostProjectMilestoneTasks.task_name)).Any())
                                            {
                                                var qty = tableCostMilestoneTask.AsEnumerable()
                                                                                .Where(c => c.Field<string>("task_name") == itemStaticCostProjectMilestoneTasks.task_name)
                                                                                .Sum(x => x.Field<double>("qty"));

                                                if (numberofunit > 0)
                                                    tableCostMilestoneTask.AsEnumerable()
                                                                                    .Where(c => c.Field<string>("task_name") == itemStaticCostProjectMilestoneTasks.task_name)
                                                                                    .ToList()
                                                                                    .ForEach(D => D.SetField("qty", qty + Convert.ToDouble(numberofunit)));
                                            }
                                            else
                                            {
                                                tableCostMilestoneTask.Rows.Add(itemStaticCostProjectMilestone["id"],
                                                   modal.id, itemStaticCostProjectMilestoneTasks.task_name,
                                                   numberofunit, item.unit_id, itemStaticCostProjectMilestoneTasks.unit,
                                                   floorOrUnit, itemStaticCostProjectMilestoneTasks.qty);
                                            }
                                        }
                                        else
                                        {
                                            tableCostMilestoneTask.Rows.Add(itemStaticCostProjectMilestone["id"],
                                               modal.id, itemStaticCostProjectMilestoneTasks.task_name,
                                               numberofunit, item.unit_id, itemStaticCostProjectMilestoneTasks.unit,
                                               floorOrUnit, itemStaticCostProjectMilestoneTasks.qty);
                                        }
                                    }
                                }

                                cost_up = 0;
                            }

                            #endregion ShopDrawing
                        }

                        _unitOfWork.ProjectUnitRepository.Add(projectUnit);
                    }

                // remove the old milestone and task by projectid;
                await _unitOfWork.CostProjectMilestoneRepository.RemoveByProjectID(modal.id).ConfigureAwait(false);
                await _unitOfWork.CostProjectTaskRepository.RemoveByProjectID(modal.id).ConfigureAwait(false);

                #region ProjectMilestone&Task

                foreach (DataRow itemStaticCostProjectMilestone in tableCostProjectMilestone.Rows)
                {
                    var _CostProjectMilestone = new CostProjectMilestone()
                    {
                        id = itemStaticCostProjectMilestone["id"].ToString(),
                        project_id = modal.id,
                        milestone_name = itemStaticCostProjectMilestone["milestone_name"].ToString(),
                        alias_name = itemStaticCostProjectMilestone["alias_name"].ToString(),
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
                        unit_type = itemtableCostMilestoneTask["unit_type"].ToString(),
                        total_unit = itemtableCostMilestoneTask["total_unit"].ToString(),
                        default_unit_hours = itemtableCostMilestoneTask["default_unit_hours"].ToString(),
                        createdby = projectViewModel.createdby,
                        created_date = _dateTime.ToString(),
                        is_deleted = false
                    };
                    _unitOfWork.CostProjectTaskRepository.Add(CostProjectMilestoneTask);
                }

                #endregion ProjectMilestone&Task

                string _project_id = modal.id;
                _unitOfWork.CostProjectRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new Utils { ID = _project_id }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }
        [HttpPost]
        [Route("UpdateCostProjectBudgetedHoursTaskID")]
        public async Task<object> UpdateCostProjectBudgetedHoursTaskID([FromBody] CostProjectTask Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils));

                await _unitOfWork.CostProjectTaskRepository.UpdateCostProjectBudgetedHoursTaskID(Utils).ConfigureAwait(false);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Task Budgeted Hours & Total Cost Updated Successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdateCostProjectDiscountAndProfitMarginByProjectID")]
        public async Task<object> UpdateCostProjectDiscountAndProfitMarginByProjectID([FromBody] UpdateCostProjectViewModel projectViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (projectViewModel == null)
                    throw new ArgumentNullException(nameof(projectViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<UpdateCostProjectViewModel, CostProject>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<CostProject>(projectViewModel);
                modal.modified_date = _dateTime.ToString();

                await _unitOfWork.CostProjectRepository.UpdateCostProjectDiscountAndProfitMarginByID(modal).ConfigureAwait(false);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Profit Margin & Discount Updated Successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("UpdateCostProjectDiscountAndTotalCostTaskID")]
        public async Task<object> UpdateCostProjectDiscountAndTotalCostTaskID([FromBody] CostProjectTask Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils));

                await _unitOfWork.CostProjectTaskRepository.UpdateCostProjectDiscountAndTotalCostTaskID(Utils).ConfigureAwait(false);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Task Discount & Total Cost Updated Successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdateCostProjectFinalValueByCostProjectID")]
        public async Task<object> UpdateCostProjectFinalValueByCostProjectID([FromBody] UpdateCostProjectFinalValueViewModel projectViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (projectViewModel == null)
                    throw new ArgumentNullException(nameof(projectViewModel));

                CostProject costProject = new CostProject()
                {
                    id = projectViewModel.id,
                    total_hours = projectViewModel.total_hours,
                    gross_total_amount = projectViewModel.gross_total_amount,
                    profit_margin_amount = projectViewModel.profit_margin_amount,
                    discount_amount = projectViewModel.discount_amount,
                    total_amount = projectViewModel.total_amount,
                    vat_amount = projectViewModel.vat_amount,
                    net_total_amount = projectViewModel.net_total_amount,
                    modifiedby = projectViewModel.createdby,
                    modified_date = _dateTime.ToString()
                };

                await _unitOfWork.CostProjectRepository.UpdateCostProjectFinalValueByCostProjectID(costProject).ConfigureAwait(false);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Value Updated Successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("UpdateCostProjectNotesQtyTaskID")]
        public async Task<object> UpdateCostProjectNotesQtyTaskID([FromBody] CostProjectTask Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils));

                await _unitOfWork.CostProjectTaskRepository.UpdateCostProjectNotesQtyTaskID(Utils).ConfigureAwait(false);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Task Notes Updated Successfully." }).ConfigureAwait(false);
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

                var status = _unitOfWork.ProjectStatusRepository.Find(projectViewModel.project_status_id);
                if (status.project_status_name.ToLower() == "open")
                {
                    //open job and move it to project.
                    var CostProject = await _unitOfWork.CostProjectRepository.Find(projectViewModel.id).ConfigureAwait(false);

                    var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<CostProject, Project>());
                    var mapper = config.CreateMapper();
                    var modal = mapper.Map<Project>(CostProject);

                    modal.id = Guid.NewGuid().ToString();

                    modal.created_date = _dateTime.ToString();
                    modal.is_deleted = false;
                    modal.project_status_id = status.id;
                    modal.project_prefix = projectViewModel.project_prefix;
                    modal.start_date = projectViewModel.start_date;
                    modal.end_date = projectViewModel.end_date;

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
                            createdby = modal.createdby,
                            created_date = _dateTime.ToString(),
                            is_deleted = false
                        };
                        _unitOfWork.EntityLocationRepository.Add(entityLocation);
                    }

                    var CostProjectMilestone = await _unitOfWork.CostProjectMilestoneRepository.GetCostProjectMilestoneByProjectID(projectViewModel.id).ConfigureAwait(false);

                    foreach (var item in CostProjectMilestone)
                    {
                        var configMilestone = new AutoMapper.MapperConfiguration(m => m.CreateMap<CostProjectMilestone, ProjectActivity>());
                        var mapperMilestone = configMilestone.CreateMapper();
                        var modalMilestone = mapperMilestone.Map<ProjectActivity>(item);
                        modalMilestone.id = Guid.NewGuid().ToString();
                        modalMilestone.activity_name = item.milestone_name;
                        modalMilestone.created_date = _dateTime.ToString();
                        modalMilestone.is_deleted = false;
                        modalMilestone.project_id = modal.id;
                        modalMilestone.start_date = projectViewModel.start_date;
                        modalMilestone.end_date = projectViewModel.end_date;

                        modalMilestone.status_id = _unitOfWork.ProjectStatusRepository.GetProjectStatusByOrgID("default")
                                              .Where(s => s.project_status_name.Equals("Open"))
                                              .Select(s => s.id)
                                              .FirstOrDefault();

                        var CostProjectTask = await _unitOfWork.CostProjectTaskRepository.GetAllMilestoneTasksByMilestoneID(item.id, item.org_id).ConfigureAwait(false);
                        foreach (var itemTask in CostProjectTask)
                        {
                            string _status_id = (await _unitOfWork.StatusRepository.GetStatusByOrgID("default").ConfigureAwait(false))
                                                .Where(s => s.status_name.Equals("Open"))
                                                .Select(s => s.id)
                                                .FirstOrDefault();

                            Domain.Entities.Tasks tasks = new Tasks()
                            {
                                id = Guid.NewGuid().ToString(),
                                empid = projectViewModel.emp_id,
                                project_id = modal.id,
                                activtity_id = itemTask.milestone_id,
                                task_name = itemTask.task_name,
                                task_desc = null,
                                priority_id = null,
                                status_id = _status_id,
                                unit = itemTask.unit,
                                qty = itemTask.qty,
                                created_date = _dateTime.ToString(),
                                createdby = itemTask.createdby,
                                is_deleted = false,
                            };


                            var ProjectTask = new ProjectActivityTask()
                            {
                                id = Guid.NewGuid().ToString(),
                                project_id = modal.id,
                                activity_id = modalMilestone.id,
                                task_id = tasks.id,
                                created_date = _dateTime.ToString(),
                                createdby = tasks.createdby,
                                is_deleted = false
                            };

                            var tags = (await _unitOfWork.ProjectTagsRepository.GetProjectTagsByProjectID(itemTask.project_id)
                                                                               .ConfigureAwait(false))
                                                                               .Where(x => x.tags != "All")
                                                                               .DistinctBy(x => x.tags)
                                                                               .ToList();
                            if (itemTask.unit_type == "Unit")
                            {
                                for (int i = 0; i < tags.Count; i++)
                                {
                                    SubTasks SubTasks = new SubTasks()
                                    {
                                        id = Guid.NewGuid().ToString(),
                                        task_id = tasks.id,
                                        sub_task_name = tags[i].tags,
                                        sub_task_desc = null,
                                        priority_id = null,
                                        status_id = _status_id,
                                        unit_type = itemTask.unit_type,
                                        qty = itemTask.default_unit_hours,
                                        created_date = _dateTime.ToString(),
                                        createdby = itemTask.createdby,
                                        is_deleted = false,
                                    };
                                    _unitOfWork.SubTaskRepository.Add(SubTasks);
                                }
                            }
                            else if (itemTask.unit_type == "Floors")
                            {
                                for (int i = 0; i < Convert.ToInt32(itemTask.total_unit); i++)
                                {
                                    SubTasks SubTasks = new SubTasks()
                                    {
                                        id = Guid.NewGuid().ToString(),
                                        task_id = tasks.id,
                                        sub_task_name = itemTask.task_name + " For" + " Floor- " + (i + 1),
                                        sub_task_desc = null,
                                        priority_id = null,
                                        status_id = _status_id,
                                        unit_type = "Floor",
                                        qty = itemTask.default_unit_hours,
                                        created_date = _dateTime.ToString(),
                                        createdby = itemTask.createdby,
                                        is_deleted = false,
                                    };
                                    _unitOfWork.SubTaskRepository.Add(SubTasks);
                                }
                            }
                            else if (itemTask.unit_type == "Project")
                            {
                                SubTasks SubTasks = new SubTasks()
                                {
                                    id = Guid.NewGuid().ToString(),
                                    task_id = tasks.id,
                                    sub_task_name = itemTask.task_name,
                                    sub_task_desc = null,
                                    priority_id = null,
                                    status_id = _status_id,
                                    unit_type = "Project",
                                    qty = itemTask.default_unit_hours,
                                    created_date = _dateTime.ToString(),
                                    createdby = itemTask.createdby,
                                    is_deleted = false,
                                };
                                _unitOfWork.SubTaskRepository.Add(SubTasks);
                            }
                            _unitOfWork.TaskRepository.Add(tasks);
                            _unitOfWork.ProjectActivityTaskRepository.Add(ProjectTask);
                        }

                        _unitOfWork.ProjectActivityRepository.Add(modalMilestone);
                    }

                    _unitOfWork.ProjectRepository.Add(modal);

                    await _unitOfWork.CostProjectRepository.UpdateCostProjectStatusByID(CostProject).ConfigureAwait(false);
                    _unitOfWork.Commit();
                }

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Cost Project Status Updated Successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("UpdateCostProjectTaskQtyTaskID")]
        public async Task<object> UpdateCostProjectTaskQtyTaskID([FromBody] CostProjectTask Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils));

                await _unitOfWork.CostProjectTaskRepository.UpdateCostProjectTaskQtyTaskID(Utils).ConfigureAwait(false);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Task Budgeted Hours Updated Successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdateIsQuotationByCostProjectID")]
        public async Task<object> UpdateIsQuotationByCostProjectID([FromBody] UpdateCostProjectIsQuotationViewModel projectViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (projectViewModel == null)
                    throw new ArgumentNullException(nameof(projectViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<UpdateCostProjectIsQuotationViewModel, CostProject>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<CostProject>(projectViewModel);
                modal.modified_date = _dateTime.ToString();

                await _unitOfWork.CostProjectRepository.UpdateIsQuotationByCostProjectID(modal).ConfigureAwait(false);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Converted To Quotation Successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("UpdateIsSelectedByTaskID")]
        public async Task<object> UpdateIsSelectedByTaskID([FromBody] CostProjectTask Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils));

                await _unitOfWork.CostProjectTaskRepository.UpdateIsSelectedByTaskID(Utils).ConfigureAwait(false);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Updated successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }
        private static DataTable StaticTasks(CostProject modal, IEnumerable<CostProjectMilestone> StaticCostProjectMilestone, DataTable tableCostProjectMilestone)
        {

            tableCostProjectMilestone.Columns.Add("id", typeof(string));
            tableCostProjectMilestone.Columns.Add("project_id", typeof(string));
            tableCostProjectMilestone.Columns.Add("milestone_id", typeof(string));
            tableCostProjectMilestone.Columns.Add("milestone_name", typeof(string));
            tableCostProjectMilestone.Columns.Add("alias_name", typeof(string));

            foreach (var itemStaticCostProjectMilestone in StaticCostProjectMilestone)
                tableCostProjectMilestone.Rows.Add(Guid.NewGuid().ToString(),
                                modal.id, itemStaticCostProjectMilestone.id,
                                itemStaticCostProjectMilestone.milestone_name,
                                itemStaticCostProjectMilestone.alias_name);

            DataTable tableCostMilestoneTask = new DataTable();
            tableCostMilestoneTask.Columns.Add("milestone_id", typeof(string));
            tableCostMilestoneTask.Columns.Add("project_id", typeof(string));
            tableCostMilestoneTask.Columns.Add("task_name", typeof(string));
            tableCostMilestoneTask.Columns.Add("qty", typeof(double));
            tableCostMilestoneTask.Columns.Add("unit_id", typeof(string));
            tableCostMilestoneTask.Columns.Add("unit_type", typeof(string));
            tableCostMilestoneTask.Columns.Add("total_unit", typeof(string));
            tableCostMilestoneTask.Columns.Add("default_unit_hours", typeof(string));
            return tableCostMilestoneTask;
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

                var results = await _unitOfWork.TypeOfDesignRepository.FetchAllTypeOfDesignByOrgID(Utils.ID).ConfigureAwait(false);

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

                var results = await _unitOfWork.TypeOfDesignRepository.Find(Utils.ID).ConfigureAwait(false);
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

                var result = await _unitOfWork.TypeOfDesignRepository.All().ConfigureAwait(false);
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

                var results = await _unitOfWork.UnitDescriptionRepository.FetchAllUnitDescriptionByOrgID(Utils.ID).ConfigureAwait(false);

                return await System.Threading.Tasks.Task.FromResult<object>(JsonConvert.SerializeObject(results, Formatting.Indented)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FetchAllUnitDescriptionExtraByOrgID")]
        public async Task<object> FetchAllUnitDescriptionExtraByOrgID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var results = await _unitOfWork.UnitDescriptionRepository.FetchAllUnitDescriptionExtraByOrgID(Utils.ID).ConfigureAwait(false);

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

                var results = await _unitOfWork.UnitDescriptionRepository.Find(Utils.ID).ConfigureAwait(false);
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

                var result = await _unitOfWork.UnitDescriptionRepository.All().ConfigureAwait(false);
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

        #region Static

        [HttpPost]
        [Route("GetAllStaticMilestoneByOrgID")]
        public async Task<object> GetAllStaticMilestoneByOrgID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                CostProjectDetailViewModel projectViewModel = new CostProjectDetailViewModel();

                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var MilestoneList = await _unitOfWork.CostProjectMilestoneRepository.GetAllStaticMilestoneByOrgID(Utils.ID).ConfigureAwait(false);

                return await Task.FromResult<object>(MilestoneList).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("GetAllStaticMilestoneTasksByMilestoneID")]
        public async Task<object> GetAllStaticMilestoneTasksByMilestoneID([FromBody] UtilsMilestoneIDAndOrgID Utils, CancellationToken cancellationToken)
        {
            try
            {
                CostProjectDetailViewModel projectViewModel = new CostProjectDetailViewModel();

                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var StaticCostProjectMilestoneTasks = await _unitOfWork.CostProjectTaskRepository
                                                       .GetAllStaticMilestoneTasksByMilestoneID
                                                       (Utils.ID, Utils.OrgID).ConfigureAwait(false);

                return await Task.FromResult<object>(StaticCostProjectMilestoneTasks).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("UpdateStaticCostTask")]
        public async Task<object> UpdateStaticCostTask([FromBody] List<CostProjectTask> Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils));

                foreach (var item in Utils)
                {
                    var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<CostProjectTask, CostProjectTask>());
                    var mapper = config.CreateMapper();
                    var modal = mapper.Map<CostProjectTask>(item);
                    modal.modified_date = _dateTime.ToString();

                    await _unitOfWork.CostProjectTaskRepository.UpdateStaticCostProjectTask(modal).ConfigureAwait(false);
                }
                _unitOfWork.Commit();
                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Quantity updated successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        #endregion Static

        #region Packages

        [HttpPost]
        [Route("AddPackages")]
        public async Task<object> AddPackages([FromBody] PackagesViewModel projectViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (projectViewModel == null)
                    throw new ArgumentNullException(nameof(projectViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<PackagesViewModel, Packages>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Packages>(projectViewModel);

                modal.id = Guid.NewGuid().ToString();
                modal.created_date = _dateTime.ToString();
                modal.is_deleted = false;

                _unitOfWork.PackagesRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Packages saved successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FetchAllPackagesByOrgID")]
        public async Task<object> FetchAllPackagesByOrgID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var results = _unitOfWork.PackagesRepository.FetchAllPackagesByOrgID(Utils.ID);

                return await System.Threading.Tasks.Task.FromResult<object>(JsonConvert.SerializeObject(results, Formatting.Indented)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindByPackagesID")]
        public async Task<object> FindByPackagesID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var results = _unitOfWork.PackagesRepository.Find(Utils.ID);
                return await Task.FromResult<object>(results).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetAllPackages")]
        public async Task<object> GetAllPackages(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.PackagesRepository.All();
                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemovePackagesByID")]
        public async Task<object> RemovePackagesByID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                _unitOfWork.PackagesRepository.Remove(Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Packages removed successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdatePackages")]
        public async Task<object> UpdatePackages([FromBody] PackagesViewModel projectViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (projectViewModel == null)
                    throw new ArgumentNullException(nameof(projectViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<PackagesViewModel, Packages>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Packages>(projectViewModel);
                modal.modified_date = _dateTime.ToString();

                _unitOfWork.PackagesRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Packages updated successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        #endregion Packages

        #region CostPerHour

        [HttpPost]
        [Route("AddCostPerHour")]
        public async Task<object> AddCostPerHour([FromBody] CostPerHourViewModel projectViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (projectViewModel == null)
                    throw new ArgumentNullException(nameof(projectViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<CostPerHourViewModel, CostPerHour>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<CostPerHour>(projectViewModel);

                modal.id = Guid.NewGuid().ToString();
                modal.created_date = _dateTime.ToString();
                modal.is_deleted = false;

                _unitOfWork.CostPerHourRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "CostPerHour saved successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FetchCostPerHourOrgID")]
        public async Task<object> FetchCostPerHourOrgID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var results = _unitOfWork.CostPerHourRepository.FetchCostPerHourOrgID(Utils.ID);

                return await System.Threading.Tasks.Task.FromResult<object>(JsonConvert.SerializeObject(results, Formatting.Indented)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindByCostPerHourID")]
        public async Task<object> FindByCostPerHourID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var results = _unitOfWork.CostPerHourRepository.Find(Utils.ID);
                return await Task.FromResult<object>(results).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetAllCostPerHour")]
        public async Task<object> GetAllCostPerHour(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.CostPerHourRepository.All();
                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemoveCostPerHourByID")]
        public async Task<object> RemoveCostPerHourByID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                _unitOfWork.CostPerHourRepository.Remove(Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "CostPerHour removed successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdateCostPerHour")]
        public async Task<object> UpdateCostPerHour([FromBody] CostPerHourViewModel projectViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (projectViewModel == null)
                    throw new ArgumentNullException(nameof(projectViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<CostPerHourViewModel, CostPerHour>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<CostPerHour>(projectViewModel);
                modal.modified_date = _dateTime.ToString();

                _unitOfWork.CostPerHourRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "CostPerHour updated successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        #endregion CostPerHour

        #region ProfitMargin

        [HttpPost]
        [Route("AddProfitMargin")]
        public async Task<object> AddProfitMargin([FromBody] ProfitMarginViewModel projectViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (projectViewModel == null)
                    throw new ArgumentNullException(nameof(projectViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<ProfitMarginViewModel, ProfitMargin>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<ProfitMargin>(projectViewModel);

                modal.id = Guid.NewGuid().ToString();
                modal.created_date = _dateTime.ToString();
                modal.is_deleted = false;

                _unitOfWork.ProfitMarginRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "ProfitMargin saved successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FetchProfitMarginOrgID")]
        public async Task<object> FetchProfitMarginOrgID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var results = _unitOfWork.ProfitMarginRepository.FetchProfitMarginOrgID(Utils.ID);

                return await System.Threading.Tasks.Task.FromResult<object>(JsonConvert.SerializeObject(results, Formatting.Indented)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindByProfitMarginID")]
        public async Task<object> FindByProfitMarginID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var results = _unitOfWork.ProfitMarginRepository.Find(Utils.ID);
                return await Task.FromResult<object>(results).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetAllProfitMargin")]
        public async Task<object> GetAllProfitMargin(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.ProfitMarginRepository.All();
                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemoveProfitMarginByID")]
        public async Task<object> RemoveProfitMarginByID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                _unitOfWork.ProfitMarginRepository.Remove(Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "ProfitMargin removed successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdateProfitMargin")]
        public async Task<object> UpdateProfitMargin([FromBody] ProfitMarginViewModel projectViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (projectViewModel == null)
                    throw new ArgumentNullException(nameof(projectViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<ProfitMarginViewModel, ProfitMargin>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<ProfitMargin>(projectViewModel);
                modal.modified_date = _dateTime.ToString();

                _unitOfWork.ProfitMarginRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "ProfitMargin updated successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        #endregion ProfitMargin
    }
}