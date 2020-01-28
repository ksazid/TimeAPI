using TimeAPI.API.Models.AccountViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TimeAPI.API.Models;
using TimeAPI.API.Services;
using Microsoft.AspNetCore.Cors;
using TimeAPI.API.Filters;
using TimeAPI.Domain;
using System.Threading;
using TimeAPI.Domain.Entities;
using TimeAPI.API.Models.DesignationViewModels;
using TimeAPI.API.Models.TaskViewModels;
using System.Collections.Generic;
using System.Text.Json;
using System.Dynamic;
using System.Data;
using Newtonsoft.Json;
using System.Globalization;

namespace TimeAPI.API.Controllers
{
    //[ApiKeyAuth]
    //[EnableCors("CorsPolicy")]
    [Route("[controller]")]
    //[Authorize(Roles = "superadmin")]
    public class EmployeeTasksController : Controller
    {
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly ApplicationSettings _appSettings;
        private readonly IUnitOfWork _unitOfWork;
        public EmployeeTasksController(IUnitOfWork unitOfWork, ILogger<EmployeeTasksController> logger,
                IEmailSender emailSender, IOptions<ApplicationSettings> AppSettings)
        {
            _emailSender = emailSender;
            _logger = logger;
            _appSettings = AppSettings.Value;
            _unitOfWork = unitOfWork;
        }

        [HttpPost]
        [Route("AddTask")]
        public async Task<object> AddTask([FromBody]  TaskViewModel TaskViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (TaskViewModel == null)
                    throw new ArgumentNullException(nameof(TaskViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<TaskViewModel, Domain.Entities.Tasks>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Domain.Entities.Tasks>(TaskViewModel);

                modal.id = Guid.NewGuid().ToString();
                modal.created_date = DateTime.Now.ToString(CultureInfo.CurrentCulture);
                modal.is_deleted = false;


                if (TaskViewModel.employees != null)
                {
                    foreach (var item in TaskViewModel.employees.empid.Distinct())
                    {

                        modal.id = Guid.NewGuid().ToString();
                        modal.created_date = DateTime.Now.ToString(CultureInfo.CurrentCulture);
                        modal.is_deleted = false;


                        var TaskTeamMembers = new TaskTeamMember()
                        {
                            id = Guid.NewGuid().ToString(),
                            task_id = modal.id,
                            empid = item,
                            createdby = modal.createdby,
                            created_date = DateTime.Now.ToString(CultureInfo.CurrentCulture),
                            is_deleted = false

                        };
                        _unitOfWork.TaskTeamMembersRepository.Add(TaskTeamMembers);
                    }
                }

                _unitOfWork.TaskRepository.Add(modal);
                _unitOfWork.Commit();

                return await System.Threading.Tasks.Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Task registered succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return System.Threading.Tasks.Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdateTask")]
        public async Task<object> UpdateAddTask([FromBody] TaskViewModel TaskViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (TaskViewModel == null)
                    throw new ArgumentNullException(nameof(TaskViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<TaskViewModel, Domain.Entities.Tasks>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Domain.Entities.Tasks>(TaskViewModel);
                modal.modified_date = DateTime.Now.ToString(CultureInfo.CurrentCulture);

                _unitOfWork.TaskTeamMembersRepository.RemoveByTaskID(modal.id);

                if (TaskViewModel.employees != null)
                {
                    foreach (var item in TaskViewModel.employees.empid.Distinct())
                    {

                        modal.id = Guid.NewGuid().ToString();
                        modal.created_date = DateTime.Now.ToString(CultureInfo.CurrentCulture);
                        modal.is_deleted = false;


                        var TaskTeamMembers = new TaskTeamMember()
                        {
                            id = Guid.NewGuid().ToString(),
                            task_id = modal.id,
                            empid = item,
                            createdby = modal.createdby,
                            created_date = DateTime.Now.ToString(CultureInfo.CurrentCulture),
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
                _unitOfWork.Commit();

                return await System.Threading.Tasks.Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Task removed succefully." }).ConfigureAwait(false);
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

        [HttpPost]
        [Route("FetchGridDataByTaskEmpID")]
        public async Task<object> FetchGridDataByTaskEmpID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                oDataTable _oDataTable = new oDataTable();
                var results = _unitOfWork.TaskRepository.FindByTaskDetailsByEmpID(Utils.ID);
                var xResult = _oDataTable.ToDataTable(results);

                return await System.Threading.Tasks.Task.FromResult<object>(JsonConvert.SerializeObject(xResult, Formatting.Indented)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return System.Threading.Tasks.Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdateTaskStatus")]
        public async Task<object> UpdateTaskStatus([FromBody] TaskUpdateStatusViewModel TaskViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (TaskViewModel == null)
                    throw new ArgumentNullException(nameof(TaskViewModel));

                var modal = new Domain.Entities.Tasks()
                {
                    id = TaskViewModel.id,
                    status_id = TaskViewModel.status_id,
                    modifiedby = TaskViewModel.modifiedby,
                    modified_date = DateTime.Now.ToString(CultureInfo.CurrentCulture)
                };

                _unitOfWork.TaskRepository.UpdateTaskStatus(modal);
                _unitOfWork.Commit();

                return await System.Threading.Tasks.Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Task updated succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return System.Threading.Tasks.Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("GetAllTaskByEmpID")]
        public Task<object> GetAllTaskByEmpID([FromBody] Utils UserID, CancellationToken cancellationToken)
        {
            if (cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(UserID.ID))
                throw new ArgumentNullException(nameof(UserID.ID));

            var Result = _unitOfWork.TaskRepository.GetAllTaskByEmpID(UserID.ID);
            return Task.FromResult<object>(Result);
        }
    }
}
