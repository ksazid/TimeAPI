﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TimeAPI.API.Models;
using TimeAPI.API.Models.TaskViewModels;
using TimeAPI.API.Services;
using TimeAPI.Domain;
using TimeAPI.Domain.Entities;

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
        private readonly DateTime _dateTime;

        public EmployeeTasksController(IUnitOfWork unitOfWork, ILogger<EmployeeTasksController> logger,
                IEmailSender emailSender, IOptions<ApplicationSettings> AppSettings)
        {
            _emailSender = emailSender;
            _logger = logger;
            _appSettings = AppSettings.Value;
            _unitOfWork = unitOfWork;
            _dateTime = InternetTime.GetCurrentTimeFromTimeZone().Value.DateTime;
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

                _unitOfWork.TaskRepository.Add(modal);
                _unitOfWork.Commit();

                return await System.Threading.Tasks.Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Task registered successfully." }).ConfigureAwait(false);
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

                modal.modified_date = _dateTime.ToString();
                modal.is_deleted = false;

                if (TaskViewModel.employees != null)
                {
                    await _unitOfWork.TaskTeamMembersRepository.RemoveByTaskID(modal.id).ConfigureAwait(false);
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

                return await System.Threading.Tasks.Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Task removed successfully." }).ConfigureAwait(false);
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

                var result = await _unitOfWork.TaskRepository.All().ConfigureAwait(false);

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

                var results = await _unitOfWork.TaskRepository.Find(Utils.ID).ConfigureAwait(false);
                results.employees = (await _unitOfWork.TaskTeamMembersRepository.FindByTaskID(Utils.ID).ConfigureAwait(false)).ToList();

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
                var results = await _unitOfWork.TaskRepository.FindByTaskDetailsByEmpID(Utils.ID).ConfigureAwait(false);
                var xResult = _oDataTable.ToDataTable(results);

                return await System.Threading.Tasks.Task.FromResult<object>(xResult).ConfigureAwait(false);
                //return await System.Threading.Tasks.Task.FromResult<object>(JsonConvert.SerializeObject(xResult, Formatting.Indented)).ConfigureAwait(false);
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
                    modified_date = _dateTime.ToString()
                };

                await _unitOfWork.TaskRepository.UpdateTaskStatus(modal).ConfigureAwait(false);
                _unitOfWork.Commit();

                return await System.Threading.Tasks.Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Task updated successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return System.Threading.Tasks.Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("GetAllTaskByEmpID")]
        public async Task<object> GetAllTaskByEmpID([FromBody] Utils UserID, CancellationToken cancellationToken)
        {
            if (cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(UserID.ID))
                throw new ArgumentNullException(nameof(UserID.ID));

            var Result = await _unitOfWork.TaskRepository.GetAllTaskByEmpID(UserID.ID, _dateTime.ToString()).ConfigureAwait(false);
            return await Task.FromResult<object>(Result).ConfigureAwait(false);
        }
    }
}