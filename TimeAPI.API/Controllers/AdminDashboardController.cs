using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;
using TimeAPI.API.Models;
using TimeAPI.API.Services;
using TimeAPI.Domain;

namespace TimeAPI.API.Controllers
{
    //[ApiKeyAuth]
    //[EnableCors("CorsPolicy")]
    [Route("[controller]")]
    //[Authorize(Roles = "superadmin")]
    public class AdminDashboardController : Controller
    {
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly ApplicationSettings _appSettings;
        private readonly IUnitOfWork _unitOfWork;
        private static DateTime _dateTime;

        public AdminDashboardController(IUnitOfWork unitOfWork, ILogger<AdminController> logger,
            IEmailSender emailSender, IOptions<ApplicationSettings> AppSettings)
        {
            _emailSender = emailSender;
            _logger = logger;
            _appSettings = AppSettings.Value;
            _unitOfWork = unitOfWork;
            _dateTime = InternetTime.GetCurrentTimeFromTimeZone().Value.DateTime;
        }

        [HttpPost]
        [Route("GetTimesheetDashboardDataByOrgIDAndDate")]
        public async Task<object> GetTimesheetDashboardDataByOrgIDAndDate([FromBody] UtilsOrgIDAndDate Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                oDataTable _oDataTable = new oDataTable();
                var results = _unitOfWork.AdminDashboardRepository.TotalEmpAttentedCountByOrgIDAndDate(Utils.OrgID, Utils.fromDate, Utils.toDate);
                var xResult = _oDataTable.ToDataTable(results);

                return await System.Threading.Tasks.Task.FromResult<object>(JsonConvert.SerializeObject(xResult, Formatting.Indented)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("TotalEmployeeDashboardDataByOrgID")]
        public async Task<object> TotalEmployeeDashboardDataByOrgID([FromBody] UtilsOrgIDAndDate Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.AdminDashboardRepository.TotalDefaultEmpCountByOrgID(Utils.OrgID);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("TotalEmployeeAbsentDashboardDataByOrgID")]
        public async Task<object> TotalEmployeeAbsentDashboardDataByOrgID([FromBody] UtilsOrgIDAndDate Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.AdminDashboardRepository.TotalEmpAbsentCountByOrgIDAndDate(Utils.OrgID, Utils.fromDate, Utils.toDate);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("GetTimesheetDashboardGridDataByOrgIDAndDate")]
        public async Task<object> GetTimesheetDashboardGridDataByOrgIDAndDate([FromBody] UtilsOrgIDAndDate Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.AdminDashboardRepository.GetTimesheetDashboardGridDataByOrgIDAndDate(Utils.OrgID, Utils.fromDate, Utils.toDate);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("GetTimesheetDashboardGridAbsentDataByOrgIDAndDate")]
        public async Task<object> GetTimesheetDashboardGridAbsentDataByOrgIDAndDate([FromBody] UtilsOrgIDAndDate Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.AdminDashboardRepository.GetTimesheetDashboardGridAbsentDataByOrgIDAndDate(Utils.OrgID, Utils.fromDate, Utils.toDate);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("GetCheckOutLocationByGroupID")]
        public async Task<object> GetCheckOutLocationByGroupID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.AdminDashboardRepository.GetCheckOutLocationByGroupID(Utils.ID);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("GetTimesheetActivityByGroupAndDate")]
        public async Task<object> GetTimesheetActivityByGroupAndDate([FromBody] UtilsGroupIDAndDate Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.AdminDashboardRepository.GetTimesheetActivityByGroupAndDate(Utils.ID, Utils.Date);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }


        [HttpPost]
        [Route("AllProjectRatioByOrgID")]
        public async Task<object> AllProjectRatioByOrgID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.AdminDashboardRepository.AllProjectRatioByOrgID(Utils.ID);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }


        [HttpPost]
        [Route("GetAllTimesheetByOrgID")]
        public async Task<object> GetAllTimesheetByOrgID([FromBody] UtilsOrgIDAndDate Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.UserRepository.GetAllTimesheetByOrgID(Utils.OrgID, Utils.fromDate, Utils.toDate);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }


        [HttpPost]
        [Route("GetAllTaskByOrgAndEmpID")]
        public Task<object> GetAllTaskByOrgAndEmpID([FromBody] UtilsOrgAndEmpID UserID, CancellationToken cancellationToken)
        {
            if (cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(UserID.OrgID))
                throw new ArgumentNullException(nameof(UserID.OrgID));

            var Result = _unitOfWork.TaskRepository.GetAllTaskByOrgAndEmpID(UserID.OrgID, UserID.EmpID);
            return Task.FromResult<object>(Result);
        }
    }
}