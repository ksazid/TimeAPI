using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using TimeAPI.API.Cache;
using TimeAPI.API.Models;
using TimeAPI.API.Services;
using TimeAPI.Domain;

namespace TimeAPI.API.Controllers
{
    //[ApiKeyAuth]
    //[EnableCors("CorsPolicy")]
    [Route("[controller]")]
    //[Authorize(Roles = "superadmin")]
    public class AdminProductivityDashboardController : Controller
    {
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly ApplicationSettings _appSettings;
        private readonly IUnitOfWork _unitOfWork;
        private static DateTime _dateTime;
        private readonly ICacheService _cacheService;

        public AdminProductivityDashboardController(IUnitOfWork unitOfWork, ILogger<AdminProductivityDashboardController> logger,
                                                    IEmailSender emailSender, IOptions<ApplicationSettings> AppSettings,
                                                    ICacheService cacheService)
        {
            _emailSender = emailSender;
            _logger = logger;
            _appSettings = AppSettings.Value;
            _unitOfWork = unitOfWork;
            _dateTime = InternetTime.GetCurrentTimeFromTimeZone().Value.DateTime;
            _cacheService = cacheService;
        }

        [HttpPost]
        [Route("EmployeeProductivityPerDateByOrgIDAndDate")]
        public async Task<object> EmployeeProductivityPerDateByOrgIDAndDate([FromBody] UtilsDateAndOrgID Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var results = await _unitOfWork.AdminProductivityDashboardRepository.EmployeeProductivityPerDateByOrgIDAndDate
                                                    (Utils.OrgID, Utils.StartDate, Utils.EndDate).ConfigureAwait(false);

                return await Task.FromResult<object>(results).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("EmployeeProductivityTimeFrequencyByOrgIDAndDate")]
        public async Task<object> EmployeeProductivityTimeFrequencyByOrgIDAndDate([FromBody] UtilsDateAndOrgID Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = await _unitOfWork.AdminProductivityDashboardRepository
                                    .EmployeeProductivityTimeFrequencyByOrgIDAndDate(Utils.OrgID, Utils.StartDate, Utils.EndDate).ConfigureAwait(false);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("EmployeeScreenshotsPerDateByOrgIDAndDate")]
        public async Task<object> EmployeeScreenshotsPerDateByOrgIDAndDate([FromBody] UtilsDateAndOrgID Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var results = await _unitOfWork.AdminProductivityDashboardRepository
                                    .ScreenshotByOrgIDAndDate(Utils.OrgID, Utils.StartDate, Utils.EndDate).ConfigureAwait(false);

                return await Task.FromResult<object>(results).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("GetEmployeeTasksTimesheetByOrgID")]
        public async Task<object> GetEmployeeTasksTimesheetByOrgID([FromBody] UtilsDateAndOrgID Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = await _unitOfWork.UserRepository.GetEmployeeTasksTimesheetByOrgID(Utils.OrgID, Utils.StartDate, Utils.EndDate).ConfigureAwait(false);
                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }
    }
}