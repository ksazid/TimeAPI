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
using TimeAPI.API.Models.TeamViewModels;
using System.Globalization;
using TimeAPI.API.Models.StatusViewModels;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TimeAPI.API.Controllroers
{
    //[ApiKeyAuth]
    //[EnableCors("CorsPolicy")]
    [Route("[controller]")]
    //[Authorize(Roles = "superadmin")]
    public class TeamController : Controller
    {
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly ApplicationSettings _appSettings;
        private readonly IUnitOfWork _unitOfWork;
        public TeamController(IUnitOfWork unitOfWork, ILogger<TeamController> logger,
            IEmailSender emailSender,
            IOptions<ApplicationSettings> AppSettings)
        {
            _emailSender = emailSender;
            _logger = logger;
            _appSettings = AppSettings.Value;
            _unitOfWork = unitOfWork;
        }

        [HttpPost]
        [Route("AddTeam")]
        public async Task<object> AddTeam([FromBody] TeamViewModel teamViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (teamViewModel == null)
                    throw new ArgumentNullException(nameof(teamViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<TeamViewModel, Team>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Team>(teamViewModel);

                modal.id = Guid.NewGuid().ToString();
                modal.created_date = DateTime.Now.ToString(CultureInfo.CurrentCulture);
                modal.is_deleted = false;

                _unitOfWork.TeamRepository.Add(modal);

                List<string> TeamMembersList = teamViewModel.teammember_empids.Cast<string>().ToList();

                //current user
                if (teamViewModel.is_addme_as_team)
                {
                    TeamMembersList.Add(teamViewModel.current_user_empid);
                }

                TeamMembersList.Add(teamViewModel.team_lead_empid);

                foreach (var item in TeamMembersList)
                {
                    bool isTeamLead = false;
                    if (teamViewModel.team_lead_empid == item)
                        isTeamLead = true;

                    TeamMembers teamMembers = new TeamMembers
                    {
                        id = Guid.NewGuid().ToString(),
                        team_id = modal.id,
                        emp_id = item,
                        created_date = DateTime.Now.ToString(CultureInfo.CurrentCulture),
                        createdby = teamViewModel.createdby,
                        is_deleted = false,
                        is_teamlead = isTeamLead
                    };

                    _unitOfWork.TeamMemberRepository.Add(teamMembers);
                }
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Team saved successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdateTeam")]
        public async Task<object> UpdateTeam([FromBody] TeamViewModel teamViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (teamViewModel == null)
                    throw new ArgumentNullException(nameof(teamViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<TeamViewModel, Team>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Team>(teamViewModel);
                modal.modified_date = DateTime.Now.ToString(CultureInfo.CurrentCulture);

                _unitOfWork.TeamRepository.Update(modal);

                foreach (var item in teamViewModel.teammember_empids)
                {
                    var Employee = _unitOfWork.EmployeeRepository.Find(item);

                    TeamMembers teamMembers = new TeamMembers
                    {
                        emp_id = Employee.id,
                        modified_date = DateTime.Now.ToString(CultureInfo.CurrentCulture),
                        modifiedby = teamViewModel.createdby,
                        is_deleted = false
                    };

                    _unitOfWork.TeamMemberRepository.Update(teamMembers);
                }

                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Team updated succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemoveTeamByID")]
        public async Task<object> RemoveTeamByID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                _unitOfWork.TeamRepository.Remove(Utils.ID);
                _unitOfWork.TeamMemberRepository.RemoveByTeamID(Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Team removed succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetAllTeam")]
        public async Task<object> GetAllTeam(CancellationToken cancellationToken)
        {

            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.TeamRepository.All();
                _unitOfWork.Commit();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindTeamsByOrgID")]
        public async Task<object> FindTeamsByOrgID([FromBody] UtilsOrgID Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.OrgID));

                var result = _unitOfWork.TeamRepository.FindTeamsByOrgID(Utils.OrgID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindByTeamID")]
        public async Task<object> FindByTeamID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                oDataTable _oDataTable = new oDataTable();
                dynamic results = _unitOfWork.TeamRepository.FindByTeamID(Utils.ID);
                return await System.Threading.Tasks.Task.FromResult<object>(JsonConvert.SerializeObject(results, Formatting.Indented)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        //[HttpPost]
        //[Route("FetchByAllTeamMembersTeamID")]
        //public async Task<object> FetchByAllTeamMembersTeamID([FromBody] Utils Utils, CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        if (cancellationToken != null)
        //            cancellationToken.ThrowIfCancellationRequested();

        //        if (Utils == null)
        //            throw new ArgumentNullException(nameof(Utils.ID));

        //        oDataTable _oDataTable = new oDataTable();
        //        IEnumerable<dynamic> results = _unitOfWork.TeamRepository.FetchByAllTeamMembersTeamID(Utils.ID);
        //        var xResult = _oDataTable.ToDataTable(results);

        //        return await System.Threading.Tasks.Task.FromResult<object>(JsonConvert.SerializeObject(xResult, Formatting.Indented)).ConfigureAwait(false);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
        //    }
        //}

        [HttpPost]
        [Route("FetchAllTeamsByOrgID")]
        public async Task<object> FetchAllTeamsByOrgID([FromBody] UtilsOrgID Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.OrgID));

                oDataTable _oDataTable = new oDataTable();
                IEnumerable<dynamic> results = _unitOfWork.TeamRepository.FetchAllTeamsByOrgID(Utils.OrgID);
                var xResult = _oDataTable.ToDataTable(results);
                return await System.Threading.Tasks.Task.FromResult<object>(JsonConvert.SerializeObject(xResult, Formatting.Indented)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FetchAllTeamMembersByTeamID")]
        public async Task<object> FetchAllTeamMembersByTeamID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                oDataTable _oDataTable = new oDataTable();
                IEnumerable<dynamic> results = _unitOfWork.TeamRepository.FetchAllTeamMembersByTeamID(Utils.ID);
                var xResult = _oDataTable.ToDataTable(results);
                return await System.Threading.Tasks.Task.FromResult<object>(JsonConvert.SerializeObject(xResult, Formatting.Indented)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

    }
}