﻿using TimeAPI.API.Models.AccountViewModels;
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
using TimeAPI.API.Models.ProjectViewModels;
using System.Globalization;
using System.Collections.Generic;
using Newtonsoft.Json;
using TimeAPI.API.Models.TeamViewModels;
using TimeAPI.API.Models.StatusViewModels;

namespace TimeAPI.API.Controllroers
{
    //[ApiKeyAuth]
    //[EnableCors("CorsPolicy")]
    [Route("[controller]")]
    //[Authorize(Roles = "superadmin")]
    public class ProjectController : Controller
    {
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly ApplicationSettings _appSettings;
        private readonly IUnitOfWork _unitOfWork;
        public ProjectController(IUnitOfWork unitOfWork, ILogger<ProjectController> logger,
            IEmailSender emailSender, IOptions<ApplicationSettings> AppSettings)
        {
            _emailSender = emailSender;
            _logger = logger;
            _appSettings = AppSettings.Value;
            _unitOfWork = unitOfWork;
        }

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
                modal.created_date = DateTime.Now.ToString(CultureInfo.CurrentCulture);
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
                modal.modified_date = DateTime.Now.ToString(CultureInfo.CurrentCulture);

                _unitOfWork.ProjectRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Project updated succefully." }).ConfigureAwait(false);
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
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Project removed succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

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
                modal.created_date = DateTime.Now.ToString(CultureInfo.CurrentCulture);
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


                statusingViewModel.modified_date = DateTime.Now.ToString(CultureInfo.CurrentCulture);
                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<ProjectStatusViewModel, ProjectStatus>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<ProjectStatus>(statusingViewModel);

                modal.modified_date = DateTime.Now.ToString(CultureInfo.CurrentCulture);

                _unitOfWork.ProjectStatusRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "ProjectStatus updated succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemoveProjectStatus")]
        public async Task<object> RemoveTaskProjectStatus([FromBody] Utils Utils, CancellationToken cancellationToken)
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

        #endregion ProjectStatus
    }
}