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
using TimeAPI.API.Models.TeamViewModels;
using TimeAPI.API.Services;
using TimeAPI.Domain;
using TimeAPI.Domain.Entities;

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
        private readonly DateTime _dateTime;

        public TeamController(IUnitOfWork unitOfWork, ILogger<TeamController> logger,
            IEmailSender emailSender, IOptions<ApplicationSettings> AppSettings)
        {
            _emailSender = emailSender;
            _logger = logger;
            _appSettings = AppSettings.Value;
            _unitOfWork = unitOfWork;
            _dateTime = InternetTime.GetCurrentTimeFromTimeZone().Value.DateTime;
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
                modal.created_date = _dateTime.ToString();
                modal.is_deleted = false;

                List<string> TeamMembersList = teamViewModel.teammember_empids.Cast<string>().ToList();

                //current user
                if (teamViewModel.is_addme_as_team)
                    TeamMembersList.Add(teamViewModel.current_user_empid);

                TeamMembersList.Add(teamViewModel.team_lead_empid);

                foreach (var item in TeamMembersList.Distinct())
                {
                    bool isTeamLead = false;
                    if (teamViewModel.team_lead_empid == item)
                        isTeamLead = true;

                    var teamMembers = new TeamMembers
                    {
                        id = Guid.NewGuid().ToString(),
                        team_id = modal.id,
                        emp_id = item,
                        created_date = _dateTime.ToString(),
                        createdby = teamViewModel.createdby,
                        is_deleted = false,
                        is_teamlead = isTeamLead
                    };
                    _unitOfWork.TeamMemberRepository.Add(teamMembers);
                }
                _unitOfWork.TeamRepository.Add(modal);
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
                modal.modified_date = _dateTime.ToString();

                _unitOfWork.TeamRepository.Update(modal);

                foreach (var item in teamViewModel.teammember_empids)
                {
                    var Employee = _unitOfWork.EmployeeRepository.Find(item);

                    TeamMembers teamMembers = new TeamMembers
                    {
                        emp_id = Employee.id,
                        modified_date = _dateTime.ToString(),
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

                var results = _unitOfWork.TeamRepository.FindByTeamID(Utils.ID);

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
                var results = _unitOfWork.TeamRepository.FetchAllTeamsByOrgID(Utils.OrgID);
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
                var results = _unitOfWork.TeamRepository.FetchAllTeamMembersByTeamID(Utils.ID);
                var xResult = _oDataTable.ToDataTable(results);

                return await System.Threading.Tasks.Task.FromResult<object>(JsonConvert.SerializeObject(xResult, Formatting.Indented)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("GetAllTeamMembersByTeamID")]
        public async Task<object> GetAllTeamMembersByTeamID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                oDataTable _oDataTable = new oDataTable();
                var results = _unitOfWork.TeamRepository.GetAllTeamMembersByTeamID(Utils.ID);
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