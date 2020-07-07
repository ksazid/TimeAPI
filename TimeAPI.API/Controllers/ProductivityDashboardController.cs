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

namespace TimeAPI.API.Controllers
{
    //[ApiKeyAuth]
    //[EnableCors("CorsPolicy")]
    [Route("[controller]")]
    //[Authorize(Roles = "superadmin")]
    public class ProductivityDashboardController : Controller
    {
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly ApplicationSettings _appSettings;
        private readonly IUnitOfWork _unitOfWork;
        private static DateTime _dateTime;

        public ProductivityDashboardController(IUnitOfWork unitOfWork, ILogger<ProductivityDashboardController> logger,
            IEmailSender emailSender, IOptions<ApplicationSettings> AppSettings)
        {
            _emailSender = emailSender;
            _logger = logger;
            _appSettings = AppSettings.Value;
            _unitOfWork = unitOfWork;
            _dateTime = InternetTime.GetCurrentTimeFromTimeZone().Value.DateTime;
        }

        /// <summary>
        /// /
        /// </summary>
        /// <param name="Utils"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("EmployeeProductivityPerDateByEmpIDAndDate")]
        public async Task<object> EmployeeProductivityPerDateByEmpIDAndDate([FromBody] UtilsEmpIDAndDate Utils, CancellationToken cancellationToken)
        {
            try
            {

                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var results = _unitOfWork.ProductivityDashboardRepository.EmployeeProductivityPerDateByEmpIDAndDate(Utils.EmpID, Utils.StartDate, Utils.EndDate);
                return await Task.FromResult<object>(results).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("DesktopEmployeeProductivityPerDateByEmpIDAndDate")]
        public async Task<object> DesktopEmployeeProductivityPerDateByEmpIDAndDate([FromBody] UtilsEmpIDAndDate Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.ProductivityDashboardRepository.DesktopEmployeeProductivityPerDateByEmpIDAndDate(Utils.EmpID, Utils.StartDate, Utils.EndDate);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }


        [HttpPost]
        [Route("EmployeeProductivityTimeFrequencyByEmpIDAndDate")]
        public async Task<object> EmployeeProductivityTimeFrequencyByEmpIDAndDate([FromBody] UtilsEmpIDAndDate Utils, CancellationToken cancellationToken)
        {
            try
            { 
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.ProductivityDashboardRepository.EmployeeProductivityTimeFrequencyByEmpIDAndDate(Utils.EmpID, Utils.StartDate, Utils.EndDate);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }
    }
}