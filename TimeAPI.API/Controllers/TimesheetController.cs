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
using TimeAPI.API.Models.TimesheetViewModels;
using System.Collections.Generic;

namespace TimeAPI.API.Controllers
{
    //[ApiKeyAuth]
    [EnableCors("CorsPolicy")]
    [Route("[controller]")]
    //[Authorize(Roles = "superadmin")]
    public class TimesheetController : Controller
    {
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly ApplicationSettings _appSettings;
        private readonly IUnitOfWork _unitOfWork;
        public TimesheetController(IUnitOfWork unitOfWork, ILogger<DesignationController> logger,
            IEmailSender emailSender,
            IOptions<ApplicationSettings> AppSettings)
        {
            _emailSender = emailSender;
            _logger = logger;
            _appSettings = AppSettings.Value;
            _unitOfWork = unitOfWork;
        }

        [HttpPost]
        [Route("AddTimesheet")]
        public async Task<object> AddTimesheet([FromBody]  TimesheetViewModel timesheetViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (timesheetViewModel == null)
                    throw new ArgumentNullException(nameof(timesheetViewModel));

                timesheetViewModel.id = Guid.NewGuid().ToString();
                timesheetViewModel.created_date = DateTime.Now.ToString();
                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<TimesheetViewModel, Timesheet>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Timesheet>(timesheetViewModel);

                _unitOfWork.TimesheetRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Timesheet registered succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPut]
        [Route("UpdateTimesheet")]
        public async Task<object> UpdateAddTimesheet([FromBody] TimesheetViewModel timesheetViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (timesheetViewModel == null)
                    throw new ArgumentNullException(nameof(timesheetViewModel));

                timesheetViewModel.modified_date = DateTime.Now.ToString();
                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<TimesheetViewModel, Timesheet>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Timesheet>(timesheetViewModel);


                _unitOfWork.TimesheetRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Timesheet updated succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemoveTimesheet")]
        public async Task<object> RemoveTimesheet([FromBody] Utils _Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (_Utils == null)
                    throw new ArgumentNullException(nameof(_Utils.ID));

                _unitOfWork.TimesheetRepository.Remove(_Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Timesheet removed succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetAllTimesheets")]
        public async Task<object> GetAllTimesheets(CancellationToken cancellationToken)
        {

            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.TimesheetRepository.All();
                _unitOfWork.Commit();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("CheckOutByEmpID")]
        public async Task<object> CheckOutByEmpID([FromBody] Utils _Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (_Utils == null)
                    throw new ArgumentNullException(nameof(_Utils.ID));

                var result = _unitOfWork.TimesheetRepository.Find(_Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

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

        //        var result = _unitOfWork.TimesheetRepository.FindByEmpName(_UtilsName.FullName);
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
