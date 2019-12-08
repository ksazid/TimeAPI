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
using TimeAPI.API.Models.TaskViewModels;
using System.Collections.Generic;
using TimeAPI.API.Models.PriorityViewModels;
using TimeAPI.API.Models.StatusViewModels;
using TimeAPI.API.Models.EmployeeTypeViewModels;
using TimeAPI.API.Models.EmployeeStatusViewModels;
using TimeAPI.API.Models.EmployeeRoleViewModels;

namespace TimeAPI.API.Controllers
{
    //[ApiKeyAuth]
    [EnableCors("CorsPolicy")]
    [Route("[controller]")]
    //[Authorize(Roles = "superadmin")]
    public class SetupController : Controller
    {
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly ApplicationSettings _appSettings;
        private readonly IUnitOfWork _unitOfWork;
        public SetupController(IUnitOfWork unitOfWork, ILogger<SetupController> logger,
            IEmailSender emailSender,
            IOptions<ApplicationSettings> AppSettings)
        {
            _emailSender = emailSender;
            _logger = logger;
            _appSettings = AppSettings.Value;
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        [Route("GetAllCountries")]
        public async Task<object> GetAllCountries(CancellationToken cancellationToken)
        {

            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.SetupRepository.Country();
                _unitOfWork.Commit();

                return await System.Threading.Tasks.Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return System.Threading.Tasks.Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }


        [HttpGet]
        [Route("GetAllTimeZones")]
        public async Task<object> GetAllTimeZones(CancellationToken cancellationToken)
        {

            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.SetupRepository.Timezones();
                _unitOfWork.Commit();

                return await System.Threading.Tasks.Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return System.Threading.Tasks.Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetEmployeeRoles")]
        public async Task<object> GetEmployeeRoles(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.EmployeeRoleRepository.GetEmployeeRoles();
                _unitOfWork.Commit();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        #region Priority

        [HttpPost]
        [Route("AddPriority")]
        public async Task<object> AddPriority([FromBody] PriorityViewModel priorityingViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (priorityingViewModel == null)
                    throw new ArgumentNullException(nameof(priorityingViewModel));

                priorityingViewModel.id = Guid.NewGuid().ToString();
                priorityingViewModel.created_date = DateTime.Now.ToString();
                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<PriorityViewModel, Priority>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Priority>(priorityingViewModel);

                _unitOfWork.PriorityRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Priority registered succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }


        [HttpPatch]
        [Route("UpdatePriority")]
        public async Task<object> UpdatePriority([FromBody] PriorityViewModel priorityingViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (priorityingViewModel == null)
                    throw new ArgumentNullException(nameof(priorityingViewModel));

                priorityingViewModel.modifiedby = priorityingViewModel.createdby;
                priorityingViewModel.modified_date = DateTime.Now.ToString();
                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<PriorityViewModel, Priority>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Priority>(priorityingViewModel);


                _unitOfWork.PriorityRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Priority updated succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }


        [HttpPost]
        [Route("RemovePriority")]
        public async Task<object> RemovePriority([FromBody] Utils _Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (_Utils == null)
                    throw new ArgumentNullException(nameof(_Utils.ID));

                _unitOfWork.PriorityRepository.Remove(_Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Priority removed succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }


        [HttpGet]
        [Route("GetAllPriority")]
        public async Task<object> GetAllPriority(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.PriorityRepository.All();
                _unitOfWork.Commit();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }


        [HttpPost]
        [Route("FindByPriorityID")]
        public async Task<object> FindByPriorityID([FromBody] Utils _Utils, CancellationToken cancellationToken)
        {

            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (_Utils == null)
                    throw new ArgumentNullException(nameof(_Utils.ID));

                var result = _unitOfWork.PriorityRepository.Find(_Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        #endregion Priority

        #region Status

        [HttpPost]
        [Route("AddTaskStatus")]
        public async Task<object> AddTaskStatus([FromBody] StatusViewModel statusingViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (statusingViewModel == null)
                    throw new ArgumentNullException(nameof(statusingViewModel));

                statusingViewModel.id = Guid.NewGuid().ToString();
                statusingViewModel.created_date = DateTime.Now.ToString();
                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<StatusViewModel, Status>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Status>(statusingViewModel);

                _unitOfWork.StatusRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Status registered succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }


        [HttpPatch]
        [Route("UpdateTaskStatus")]
        public async Task<object> UpdateTaskStatus([FromBody] StatusViewModel statusingViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (statusingViewModel == null)
                    throw new ArgumentNullException(nameof(statusingViewModel));

                statusingViewModel.modifiedby = statusingViewModel.createdby;
                statusingViewModel.modified_date = DateTime.Now.ToString();
                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<StatusViewModel, Status>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Status>(statusingViewModel);


                _unitOfWork.StatusRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Status updated succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }


        [HttpPost]
        [Route("RemoveTaskStatus")]
        public async Task<object> RemoveTaskStatus([FromBody] Utils _Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (_Utils == null)
                    throw new ArgumentNullException(nameof(_Utils.ID));

                _unitOfWork.StatusRepository.Remove(_Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Status removed succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }


        [HttpGet]
        [Route("GetAllTaskStatus")]
        public async Task<object> GetAllTaskStatus(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.StatusRepository.All();
                _unitOfWork.Commit();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }


        [HttpPost]
        [Route("FindByTaskStatusID")]
        public async Task<object> FindByTaskStatusID([FromBody] Utils _Utils, CancellationToken cancellationToken)
        {

            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (_Utils == null)
                    throw new ArgumentNullException(nameof(_Utils.ID));

                var result = _unitOfWork.StatusRepository.Find(_Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        #endregion Status

        #region EmployeeType

        [HttpPost]
        [Route("AddEmployeeType")]
        public async Task<object> AddEmployeeType([FromBody] EmployeeTypeViewModel employeetypeingViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (employeetypeingViewModel == null)
                    throw new ArgumentNullException(nameof(employeetypeingViewModel));

                employeetypeingViewModel.id = Guid.NewGuid().ToString();
                employeetypeingViewModel.created_date = DateTime.Now.ToString();
                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<EmployeeTypeViewModel, EmployeeType>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<EmployeeType>(employeetypeingViewModel);

                _unitOfWork.EmployeeTypeRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "EmployeeType registered succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }


        [HttpPatch]
        [Route("UpdateEmployeeType")]
        public async Task<object> UpdateEmployeeType([FromBody] EmployeeTypeViewModel employeetypeingViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (employeetypeingViewModel == null)
                    throw new ArgumentNullException(nameof(employeetypeingViewModel));

                employeetypeingViewModel.modifiedby = employeetypeingViewModel.createdby;
                employeetypeingViewModel.modified_date = DateTime.Now.ToString();
                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<EmployeeTypeViewModel, EmployeeType>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<EmployeeType>(employeetypeingViewModel);


                _unitOfWork.EmployeeTypeRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "EmployeeType updated succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }


        [HttpPost]
        [Route("RemoveEmployeeType")]
        public async Task<object> RemoveEmployeeType([FromBody] Utils _Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (_Utils == null)
                    throw new ArgumentNullException(nameof(_Utils.ID));

                _unitOfWork.EmployeeTypeRepository.Remove(_Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "EmployeeType removed succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }


        [HttpGet]
        [Route("GetAllEmployeeType")]
        public async Task<object> GetAllEmployeeType(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.EmployeeTypeRepository.All();
                _unitOfWork.Commit();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }


        [HttpPost]
        [Route("FindByEmployeeTypeID")]
        public async Task<object> FindByEmployeeTypeID([FromBody] Utils _Utils, CancellationToken cancellationToken)
        {

            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (_Utils == null)
                    throw new ArgumentNullException(nameof(_Utils.ID));

                var result = _unitOfWork.EmployeeTypeRepository.Find(_Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        #endregion EmployeeType

        #region EmployeeStatus

        [HttpPost]
        [Route("AddEmployeeStatus")]
        public async Task<object> AddEmployeeStatus([FromBody] EmployeeStatusViewModel employeestatusViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (employeestatusViewModel == null)
                    throw new ArgumentNullException(nameof(employeestatusViewModel));

                employeestatusViewModel.id = Guid.NewGuid().ToString();
                employeestatusViewModel.created_date = DateTime.Now.ToString();
                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<EmployeeStatusViewModel, EmployeeStatus>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<EmployeeStatus>(employeestatusViewModel);

                _unitOfWork.EmployeeStatusRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "EmployeeStatus registered succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }


        [HttpPatch]
        [Route("UpdateEmployeeStatus")]
        public async Task<object> UpdateEmployeeStatus([FromBody] EmployeeStatusViewModel employeestatusViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (employeestatusViewModel == null)
                    throw new ArgumentNullException(nameof(employeestatusViewModel));

                employeestatusViewModel.modifiedby = employeestatusViewModel.createdby;
                employeestatusViewModel.modified_date = DateTime.Now.ToString();
                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<EmployeeStatusViewModel, EmployeeStatus>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<EmployeeStatus>(employeestatusViewModel);


                _unitOfWork.EmployeeStatusRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "EmployeeStatus updated succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }


        [HttpPost]
        [Route("RemoveEmployeeStatus")]
        public async Task<object> RemoveEmployeeStatus([FromBody] Utils _Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (_Utils == null)
                    throw new ArgumentNullException(nameof(_Utils.ID));

                _unitOfWork.EmployeeStatusRepository.Remove(_Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "EmployeeStatus removed succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }


        [HttpGet]
        [Route("GetAllEmployeeStatus")]
        public async Task<object> GetAllEmployeeStatus(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.EmployeeStatusRepository.All();
                _unitOfWork.Commit();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }


        [HttpPost]
        [Route("FindByEmployeeStatusID")]
        public async Task<object> FindByEmployeeStatusID([FromBody] Utils _Utils, CancellationToken cancellationToken)
        {

            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (_Utils == null)
                    throw new ArgumentNullException(nameof(_Utils.ID));

                var result = _unitOfWork.EmployeeStatusRepository.Find(_Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        #endregion EmployeeStatus

        #region IndustryType

        [HttpPost]
        [Route("AddIndustryType")]
        public async Task<object> AddIndustryType([FromBody] EmployeeStatusViewModel industrytypeViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (industrytypeViewModel == null)
                    throw new ArgumentNullException(nameof(industrytypeViewModel));

                industrytypeViewModel.id = Guid.NewGuid().ToString();
                industrytypeViewModel.created_date = DateTime.Now.ToString();
                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<EmployeeStatusViewModel, IndustryType>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<IndustryType>(industrytypeViewModel);

                _unitOfWork.IndustryTypeRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "IndustryType registered succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }


        [HttpPatch]
        [Route("UpdateIndustryType")]
        public async Task<object> UpdateIndustryType([FromBody] IndustryTypeViewModels industrytypeViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (industrytypeViewModel == null)
                    throw new ArgumentNullException(nameof(industrytypeViewModel));

                industrytypeViewModel.modifiedby = industrytypeViewModel.createdby;
                industrytypeViewModel.modified_date = DateTime.Now.ToString();
                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<EmployeeStatusViewModel, IndustryType>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<IndustryType>(industrytypeViewModel);


                _unitOfWork.IndustryTypeRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "IndustryType updated succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }


        [HttpPost]
        [Route("RemoveIndustryType")]
        public async Task<object> RemoveIndustryType([FromBody] Utils _Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (_Utils == null)
                    throw new ArgumentNullException(nameof(_Utils.ID));

                _unitOfWork.IndustryTypeRepository.Remove(_Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "IndustryType removed succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }


        [HttpGet]
        [Route("GetAllIndustryType")]
        public async Task<object> GetAllIndustryType(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.IndustryTypeRepository.All();
                _unitOfWork.Commit();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }


        [HttpPost]
        [Route("FindByIndustryTypeID")]
        public async Task<object> FindByIndustryTypeID([FromBody] Utils _Utils, CancellationToken cancellationToken)
        {

            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (_Utils == null)
                    throw new ArgumentNullException(nameof(_Utils.ID));

                var result = _unitOfWork.IndustryTypeRepository.Find(_Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        #endregion IndustryType

        
    }
}


#region EmployeeRole

//[HttpPost]
//[Route("AddEmployeeRole")]
//public async Task<object> AddEmployeeRole([FromBody] EmployeeRoleViewModel employeeroleViewModel, CancellationToken cancellationToken)
//{
//    try
//    {
//        if (cancellationToken != null)
//            cancellationToken.ThrowIfCancellationRequested();

//        if (employeeroleViewModel == null)
//            throw new ArgumentNullException(nameof(employeeroleViewModel));

//        employeeroleViewModel.id = Guid.NewGuid().ToString();
//        employeeroleViewModel.created_date = DateTime.Now.ToString();
//        var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<EmployeeRoleViewModel, EmployeeRole>());
//        var mapper = config.CreateMapper();
//        var modal = mapper.Map<EmployeeRole>(employeeroleViewModel);

//        _unitOfWork.EmployeeRoleRepository.Add(modal);
//        _unitOfWork.Commit();

//        return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "EmployeeRole registered succefully." }).ConfigureAwait(false);
//    }
//    catch (Exception ex)
//    {
//        return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
//    }
//}


//[HttpPatch]
//[Route("UpdateEmployeeRole")]
//public async Task<object> UpdateEmployeeRole([FromBody] EmployeeRoleViewModel employeeroleViewModel, CancellationToken cancellationToken)
//{
//    try
//    {
//        if (cancellationToken != null)
//            cancellationToken.ThrowIfCancellationRequested();

//        if (employeeroleViewModel == null)
//            throw new ArgumentNullException(nameof(employeeroleViewModel));

//        employeeroleViewModel.modifiedby = employeeroleViewModel.createdby;
//        employeeroleViewModel.modified_date = DateTime.Now.ToString();
//        var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<EmployeeRoleViewModel, EmployeeRole>());
//        var mapper = config.CreateMapper();
//        var modal = mapper.Map<EmployeeRole>(employeeroleViewModel);


//        _unitOfWork.EmployeeRoleRepository.Update(modal);
//        _unitOfWork.Commit();

//        return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "EmployeeRole updated succefully." }).ConfigureAwait(false);
//    }
//    catch (Exception ex)
//    {
//        return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
//    }
//}


//[HttpPost]
//[Route("RemoveEmployeeRole")]
//public async Task<object> RemoveEmployeeRole([FromBody] Utils _Utils, CancellationToken cancellationToken)
//{
//    try
//    {
//        if (cancellationToken != null)
//            cancellationToken.ThrowIfCancellationRequested();

//        if (_Utils == null)
//            throw new ArgumentNullException(nameof(_Utils.ID));

//        _unitOfWork.EmployeeRoleRepository.Remove(_Utils.ID);
//        _unitOfWork.Commit();

//        return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "EmployeeRole removed succefully." }).ConfigureAwait(false);
//    }
//    catch (Exception ex)
//    {
//        return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
//    }
//}


//[HttpGet]
//[Route("GetAllEmployeeRole")]
//public async Task<object> GetAllEmployeeRole(CancellationToken cancellationToken)
//{
//    try
//    {
//        if (cancellationToken != null)
//            cancellationToken.ThrowIfCancellationRequested();

//        var result = _unitOfWork.EmployeeRoleRepository.All();
//        _unitOfWork.Commit();

//        return await Task.FromResult<object>(result).ConfigureAwait(false);
//    }
//    catch (Exception ex)
//    {
//        return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
//    }
//}

#endregion EmployeeRole
