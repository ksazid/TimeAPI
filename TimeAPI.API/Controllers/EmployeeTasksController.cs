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

namespace TimeAPI.API.Controllers
{
    //[ApiKeyAuth]
    [EnableCors("CorsPolicy")]
    [Route("[controller]")]
    //[Authorize(Roles = "superadmin")]
    public class EmployeeTasksController : Controller
    {
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly ApplicationSettings _appSettings;
        private readonly IUnitOfWork _unitOfWork;
        public EmployeeTasksController(IUnitOfWork unitOfWork, ILogger<EmployeeTasksController> logger,
            IEmailSender emailSender,
            IOptions<ApplicationSettings> AppSettings)
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

                TaskViewModel.id = Guid.NewGuid().ToString();
                TaskViewModel.created_date = DateTime.Now.ToString();
                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<TaskViewModel, Domain.Entities.Tasks>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Domain.Entities.Tasks>(TaskViewModel);

                _unitOfWork.TaskRepository.Add(modal);
                _unitOfWork.Commit();

                return await System.Threading.Tasks.Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Task registered succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return System.Threading.Tasks.Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPut]
        [Route("UpdateTask")]
        public async Task<object> UpdateAddTask([FromBody] TaskViewModel TaskViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (TaskViewModel == null)
                    throw new ArgumentNullException(nameof(TaskViewModel));

                TaskViewModel.modified_date = DateTime.Now.ToString();
                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<TaskViewModel, Domain.Entities.Tasks>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Domain.Entities.Tasks>(TaskViewModel);


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
        public async Task<object> RemoveTask([FromBody] Utils _Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (_Utils == null)
                    throw new ArgumentNullException(nameof(_Utils.ID));

                _unitOfWork.TaskRepository.Remove(_Utils.ID);
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
                _unitOfWork.Commit();

                return await System.Threading.Tasks.Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return System.Threading.Tasks.Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpGet]
        [Route("FindByTasksId")]
        public async Task<object> FindByTasksID([FromBody] Utils _Utils, CancellationToken cancellationToken)
        {

            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (_Utils == null)
                    throw new ArgumentNullException(nameof(_Utils.ID));

                var result = _unitOfWork.TaskRepository.Find(_Utils.ID);
                _unitOfWork.Commit();

                return await System.Threading.Tasks.Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return System.Threading.Tasks.Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        //[HttpPost]
        //[Route("CheckOutByEmpID")]
        //public async Task<object> CheckOutByEmpID([FromBody] Utils _Utils, CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        if (cancellationToken != null)
        //            cancellationToken.ThrowIfCancellationRequested();

        //        if (_Utils == null)
        //            throw new ArgumentNullException(nameof(_Utils.ID));

        //        var result = _unitOfWork.TaskRepository.Find(_Utils.ID);
        //        _unitOfWork.Commit();

        //        return await System.Threading.Tasks.Task.FromResult<object>(result).ConfigureAwait(false);
        //    }
        //    catch (Exception ex)
        //    {
        //        return System.Threading.Tasks.Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
        //    }
        //}

        //[HttpPost]
        //[Route("FindByEmpName")]
        //public async Task<object> FindByEmpName([FromBody] UtilsName _UtilsName, CancellationToken cancellationToken)
        //{

        //    try
        //    {
        //        if (cancellationToken != null)
        //            cancellationToken.ThrowIfCancellationRequested();

        //        if (_UtilsName == null)
        //            throw new ArgumentNullException(nameof(_UtilsName));

        //        var result = _unitOfWork.TaskRepository.FindByEmpName(_UtilsName.FullName);
        //        _unitOfWork.Commit();

        //        return await Task.FromResult<object>(result).ConfigureAwait(false);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
        //    }
        //}

    }
}
