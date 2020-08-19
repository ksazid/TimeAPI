using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Threading;
using System.Threading.Tasks;
using TimeAPI.API.Cache;
using TimeAPI.API.Models;
using TimeAPI.API.Services;
using TimeAPI.Domain;

namespace TimeAPI.API.Controllers
{
    //[EnableCors("CorsPolicy")]
    [Route("[controller]")]
    //[Authorize]
    public class UserController : Controller
    {
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly ApplicationSettings _appSettings;
        private readonly IUnitOfWork _unitOfWork;
        private readonly DateTime _dateTime;
        private readonly ICacheService _cacheService;
        private readonly JsonSerializerSettings _JsonSerializerSettings;


        public UserController(IUnitOfWork unitOfWork, ILogger<EmployeeController> logger,
            IEmailSender emailSender, IOptions<ApplicationSettings> AppSettings,
            ICacheService cacheService)
        {
            _emailSender = emailSender;
            _logger = logger;
            _appSettings = AppSettings.Value;
            _unitOfWork = unitOfWork;
            _dateTime = InternetTime.GetCurrentTimeFromTimeZone().Value.DateTime;
            _cacheService = cacheService;

            _JsonSerializerSettings = new JsonSerializerSettings();
            _JsonSerializerSettings.ContractResolver = new LowercaseContractResolver();
        }

        [HttpPost]
        [Route("GetUserDataGroupByUserID")]
        public Task<object> GetUserDataGroupByUserID([FromBody] Utils UserID, CancellationToken cancellationToken)
        {
            if (cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(UserID.ID))
                throw new ArgumentNullException(nameof(UserID.ID));

            if (!_cacheService.IsCached(UserID.ID))
            {
                var Result = _unitOfWork.UserRepository.GetUserDataGroupByUserID(UserID.ID, _dateTime.ToString());
                string output = JsonConvert.SerializeObject(Result, _JsonSerializerSettings);
                _cacheService.SetCacheValueAsync(UserID.ID, output);
                return Task.FromResult<object>(Result);
            }
            else
            {
                var Result = _cacheService.GetCacheValueAsync(UserID.ID);
                object deserializedProduct = JsonConvert.DeserializeObject<object>(Result.Result, _JsonSerializerSettings);
                return Task.FromResult<object>(deserializedProduct);
            }
        }

        [HttpPost]
        [Route("GetAllTimesheetByEmpID")]
        public Task<object> GetAllTimesheetByEmpID([FromBody] Utils UserID, CancellationToken cancellationToken)
        {
            if (cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(UserID.ID))
                throw new ArgumentNullException(nameof(UserID.ID));

            var Result = _unitOfWork.UserRepository.GetAllTimesheetByEmpID(UserID.ID, _dateTime.ToString());
            return Task.FromResult<object>(Result);
        }

        [HttpPost]
        [Route("LastCheckinByEmpID")]
        public Task<object> LastCheckinByEmpID([FromBody] Utils UserID, CancellationToken cancellationToken)
        {
            if (cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(UserID.ID))
                throw new ArgumentNullException(nameof(UserID.ID));

            var Result = _unitOfWork.UserRepository.LastCheckinByEmpID(UserID.ID, _dateTime.ToString());
            return Task.FromResult<object>(Result);
        }
    }
}