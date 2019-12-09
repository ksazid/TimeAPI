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
using TimeAPI.API.Models.ReportingViewModels;
using System.Threading;
using TimeAPI.Domain.Entities;

namespace TimeAPI.API.Controllers
{
    //[ApiKeyAuth]
    //[EnableCors("CorsPolicy")]
    [Route("[controller]")]
    //[Authorize(Roles = "superadmin")]
    public class ReportingController : Controller
    {
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly ApplicationSettings _appSettings;
        private readonly IUnitOfWork _unitOfWork;
        public ReportingController(IUnitOfWork unitOfWork, ILogger<ReportingController> logger,
            IEmailSender emailSender,
            IOptions<ApplicationSettings> AppSettings)
        {
            _emailSender = emailSender;
            _logger = logger;
            _appSettings = AppSettings.Value;
            _unitOfWork = unitOfWork;
        }
        
        [HttpPost]
        [Route("AddReporting")]
        public async Task<object> AddReporting([FromBody] ReportingViewModel reportingViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (reportingViewModel == null)
                    throw new ArgumentNullException(nameof(reportingViewModel));

                reportingViewModel.id = Guid.NewGuid().ToString();
                reportingViewModel.created_date = DateTime.Now.ToString();
                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<ReportingViewModel, Reporting>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Reporting>(reportingViewModel);

                _unitOfWork.ReportingRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Reporting registered succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        
        [HttpPatch]
        [Route("UpdateReporting")]
        public async Task<object> UpdateReporting([FromBody] ReportingViewModel reportingViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (reportingViewModel == null)
                    throw new ArgumentNullException(nameof(reportingViewModel));

                reportingViewModel.modifiedby = reportingViewModel.createdby;
                reportingViewModel.modified_date = DateTime.Now.ToString();
                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<ReportingViewModel, Reporting>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Reporting>(reportingViewModel);


                _unitOfWork.ReportingRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Reporting updated succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        
        [HttpPost]
        [Route("RemoveReporting")]
        public async Task<object> RemoveReporting([FromBody] Utils _Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (_Utils == null)
                    throw new ArgumentNullException(nameof(_Utils.ID));

                _unitOfWork.ReportingRepository.Remove(_Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Reporting removed succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        
        [HttpGet]
        [Route("GetAllReporting")]
        public async Task<object> GetAllReporting(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.ReportingRepository.All();
                _unitOfWork.Commit();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        
        [HttpPost]
        [Route("FindByReportEmpID")]
        public async Task<object> FindByReportEmpID([FromBody] Utils _Utils, CancellationToken cancellationToken)
        {

            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (_Utils == null)
                    throw new ArgumentNullException(nameof(_Utils.ID));

                var result = _unitOfWork.ReportingRepository.FindByReportEmpID(_Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        
        [HttpPost]
        [Route("FindReportingHeadByEmpID")]
        public async Task<object> FindReportingHeadByEmpID([FromBody] Utils _Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (_Utils == null)
                    throw new ArgumentNullException(nameof(_Utils.ID));

                var result = _unitOfWork.ReportingRepository.FindReportingHeadByEmpID(_Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

    }
}
