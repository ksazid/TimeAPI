using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
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

        public UserController(IUnitOfWork unitOfWork, ILogger<EmployeeController> logger,
            IEmailSender emailSender,
           IOptions<ApplicationSettings> AppSettings)
        {
            _emailSender = emailSender;
            _logger = logger;
            _appSettings = AppSettings.Value;
            _unitOfWork = unitOfWork;
            _dateTime = InternetTime.GetCurrentTimeFromTimeZone().Value.DateTime;
        }

        [HttpPost]
        [Route("GetUserDataGroupByUserID")]
        public Task<object> GetUserDataGroupByUserID([FromBody] Utils UserID, CancellationToken cancellationToken)
        {
            if (cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(UserID.ID))
                throw new ArgumentNullException(nameof(UserID.ID));

            var Result = _unitOfWork.UserRepository.GetUserDataGroupByUserID(UserID.ID);
            return Task.FromResult<object>(Result);
        }
    }
}