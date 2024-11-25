﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;
using TimeAPI.API.Cache;
using TimeAPI.API.Models;
using TimeAPI.API.Models.AdministrativeViewModels;
using TimeAPI.API.Models.EmployeeStatusViewModels;
using TimeAPI.API.Models.EmployeeTypeViewModels;
using TimeAPI.API.Models.LocalActivityViewModels;
using TimeAPI.API.Models.PriorityViewModels;
using TimeAPI.API.Models.StatusViewModels;
using TimeAPI.API.Services;
using TimeAPI.Domain;
using TimeAPI.Domain.Entities;

namespace TimeAPI.API.Controllers
{
    //[ApiKeyAuth]
    //[EnableCors("CorsPolicy")]
    [Route("[controller]")]
    //[Authorize(Roles = "superadmin")]
    public class SetupController : Controller
    {
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly ApplicationSettings _appSettings;
        private readonly IUnitOfWork _unitOfWork;
        private readonly DateTime _dateTime;
        private readonly ICacheService _cacheService;
        private readonly JsonSerializerSettings _JsonSerializerSettings;
        public SetupController(IUnitOfWork unitOfWork, ILogger<SetupController> logger,
                        IEmailSender emailSender, IOptions<ApplicationSettings> AppSettings, ICacheService cacheService)
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

        [HttpGet]
        [Route("GetAllCountries")]
        public async Task<object> GetAllCountries(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();


                if (!_cacheService.IsCached("Country"))
                {
                    var result = _unitOfWork.SetupRepository.Country();
                    string output = JsonConvert.SerializeObject(result, _JsonSerializerSettings);
                    await _cacheService.SetCacheValueAsync("Country", output).ConfigureAwait(false);
                    return await Task.FromResult<object>(result).ConfigureAwait(false);
                }
                else
                {
                    var result = _cacheService.GetCacheValueAsync("Country");
                    object deserializedProduct = JsonConvert.DeserializeObject<object>(result.Result, _JsonSerializerSettings);
                    return await Task.FromResult<object>(deserializedProduct).ConfigureAwait(false);
                }
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

                return await System.Threading.Tasks.Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return System.Threading.Tasks.Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetAllPhoneCode")]
        public async Task<object> GetAllPhoneCode(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();


                if (!_cacheService.IsCached("PhoneCodes"))
                {

                    var result = _unitOfWork.SetupRepository.PhoneCodes();
                    string output = JsonConvert.SerializeObject(result, _JsonSerializerSettings);
                    await _cacheService.SetCacheValueAsync("PhoneCodes", output).ConfigureAwait(false);
                    return await Task.FromResult<object>(result).ConfigureAwait(false);
                }
                else
                {
                    var result = _cacheService.GetCacheValueAsync("PhoneCodes");
                    object deserializedProduct = JsonConvert.DeserializeObject<object>(result.Result, _JsonSerializerSettings);
                    return await Task.FromResult<object>(deserializedProduct).ConfigureAwait(false);
                }
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

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("IsPhoneValid")]
        public async Task<object> IsPhoneValid([FromBody] UtilPhone UtilPhone, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (UtilPhone == null)
                    throw new ArgumentNullException(nameof(UtilPhone));

                string Result = UserHelpers.ValidatePhoneNumber(UtilPhone.PhoneNumber);
                if (Result.Equals("VALID"))
                {
                    return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "The entered phone no is valid" }).ConfigureAwait(false);
                }
                else
                {
                    return await Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = "Error", Desc = "The entered phone no is invalid" }).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        #region Administrative

        [HttpPost]
        [Route("AddAdministrative")]
        public async Task<object> AddAdministrative([FromBody] AdministrativeViewModel administrativeViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (administrativeViewModel == null)
                    throw new ArgumentNullException(nameof(administrativeViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<AdministrativeViewModel, Administrative>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Administrative>(administrativeViewModel);

                modal.id = Guid.NewGuid().ToString();
                modal.created_date = _dateTime.ToString();
                modal.is_deleted = false;

                _unitOfWork.AdministrativeRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Administrative registered successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdateAdministrative")]
        public async Task<object> UpdateAdministrative([FromBody] AdministrativeViewModel administrativeViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (administrativeViewModel == null)
                    throw new ArgumentNullException(nameof(administrativeViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<AdministrativeViewModel, Administrative>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Administrative>(administrativeViewModel);

                modal.modified_date = _dateTime.ToString();

                _unitOfWork.AdministrativeRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Administrative updated successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemoveAdministrative")]
        public async Task<object> RemoveAdministrative([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                _unitOfWork.AdministrativeRepository.Remove(Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Administrative removed successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetAllAdministrative")]
        public async Task<object> GetAllAdministrative(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.AdministrativeRepository.All();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindByAdministrativeID")]
        public async Task<object> FindByAdministrativeID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = _unitOfWork.AdministrativeRepository.Find(Utils.ID);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("GetAdministrativeByOrgID")]
        public async Task<object> GetAdministrativeByOrgID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var results = _unitOfWork.AdministrativeRepository.GetByOrgID(Utils.ID);
                return await System.Threading.Tasks.Task.FromResult<object>(JsonConvert.SerializeObject(results, Formatting.Indented)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("GetAdministrativeTaskByOrgID")]
        public async Task<object> GetAdministrativeTaskByOrgID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = _unitOfWork.AdministrativeRepository.GetAdministrativeTaskByOrgID(Utils.ID);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        #endregion Administrative

        #region Priority

        [HttpPost]
        [Route("AddPriority")]
        public async Task<object> AddPriority([FromBody] ProfileImageViewModel administrativeViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (administrativeViewModel == null)
                    throw new ArgumentNullException(nameof(administrativeViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<ProfileImageViewModel, Priority>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Priority>(administrativeViewModel);

                modal.id = Guid.NewGuid().ToString();
                modal.created_date = _dateTime.ToString();
                modal.is_deleted = false;

                _unitOfWork.PriorityRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Priority registered successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdatePriority")]
        public async Task<object> UpdatePriority([FromBody] ProfileImageViewModel administrativeViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (administrativeViewModel == null)
                    throw new ArgumentNullException(nameof(administrativeViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<ProfileImageViewModel, Priority>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Priority>(administrativeViewModel);

                modal.modified_date = _dateTime.ToString();

                _unitOfWork.PriorityRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Priority updated successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemovePriority")]
        public async Task<object> RemovePriority([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                _unitOfWork.PriorityRepository.Remove(Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Priority removed successfully." }).ConfigureAwait(false);
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

                var result = await _unitOfWork.PriorityRepository.All().ConfigureAwait(false);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindByPriorityID")]
        public async Task<object> FindByPriorityID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = await _unitOfWork.PriorityRepository.Find(Utils.ID).ConfigureAwait(false);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("GetPriorityByOrgID")]
        public async Task<object> GetPriorityByOrgID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = await _unitOfWork.PriorityRepository.GetPriorityByOrgID(Utils.ID).ConfigureAwait(false);

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

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<StatusViewModel, Status>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Status>(statusingViewModel);

                modal.id = Guid.NewGuid().ToString();
                modal.created_date = _dateTime.ToString();
                modal.is_deleted = false;

                _unitOfWork.StatusRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Status registered successfully." }).ConfigureAwait(false);
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

                statusingViewModel.modified_date = _dateTime.ToString();
                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<StatusViewModel, Status>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Status>(statusingViewModel);

                modal.modified_date = _dateTime.ToString();

                _unitOfWork.StatusRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Status updated successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemoveTaskStatus")]
        public async Task<object> RemoveTaskStatus([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                _unitOfWork.StatusRepository.Remove(Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Status removed successfully." }).ConfigureAwait(false);
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

                var result = await _unitOfWork.StatusRepository.All().ConfigureAwait(false);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindByTaskStatusID")]
        public async Task<object> FindByTaskStatusID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = await _unitOfWork.StatusRepository.Find(Utils.ID).ConfigureAwait(false);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("GetStatusByOrgID")]
        public async Task<object> GetStatusByOrgID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = await _unitOfWork.StatusRepository.GetStatusByOrgID(Utils.ID).ConfigureAwait(false);

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

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<EmployeeTypeViewModel, EmployeeType>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<EmployeeType>(employeetypeingViewModel);

                modal.id = Guid.NewGuid().ToString();
                modal.created_date = _dateTime.ToString();
                modal.is_deleted = false;

                _unitOfWork.EmployeeTypeRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "EmployeeType registered successfully." }).ConfigureAwait(false);
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

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<EmployeeTypeViewModel, EmployeeType>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<EmployeeType>(employeetypeingViewModel);
                modal.modified_date = _dateTime.ToString();

                _unitOfWork.EmployeeTypeRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "EmployeeType updated successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemoveEmployeeType")]
        public async Task<object> RemoveEmployeeType([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                _unitOfWork.EmployeeTypeRepository.Remove(Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "EmployeeType removed successfully." }).ConfigureAwait(false);
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

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindByEmployeeTypeID")]
        public async Task<object> FindByEmployeeTypeID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = _unitOfWork.EmployeeTypeRepository.Find(Utils.ID);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("GetEmployeeTypeByOrgID")]
        public async Task<object> GetEmployeeTypeByOrgID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = _unitOfWork.EmployeeTypeRepository.GetEmployeeTypeByOrgID(Utils.ID);

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

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<EmployeeStatusViewModel, EmployeeStatus>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<EmployeeStatus>(employeestatusViewModel);

                modal.id = Guid.NewGuid().ToString();
                modal.created_date = _dateTime.ToString();
                modal.is_deleted = false;

                _unitOfWork.EmployeeStatusRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "EmployeeStatus registered successfully." }).ConfigureAwait(false);
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

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<EmployeeStatusViewModel, EmployeeStatus>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<EmployeeStatus>(employeestatusViewModel);
                modal.modified_date = _dateTime.ToString();

                _unitOfWork.EmployeeStatusRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "EmployeeStatus updated successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemoveEmployeeStatus")]
        public async Task<object> RemoveEmployeeStatus([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                _unitOfWork.EmployeeStatusRepository.Remove(Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "EmployeeStatus removed successfully." }).ConfigureAwait(false);
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

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindByEmployeeStatusID")]
        public async Task<object> FindByEmployeeStatusID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = _unitOfWork.EmployeeStatusRepository.Find(Utils.ID);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("GetEmployeeStatusByOrgID")]
        public async Task<object> GetEmployeeStatusByOrgID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = _unitOfWork.EmployeeStatusRepository.GetEmployeeStatusByOrgID(Utils.ID);

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
        public async Task<object> AddIndustryType([FromBody] IndustryTypeViewModel industrytypeViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (industrytypeViewModel == null)
                    throw new ArgumentNullException(nameof(industrytypeViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<IndustryTypeViewModel, IndustryType>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<IndustryType>(industrytypeViewModel);

                modal.id = Guid.NewGuid().ToString();
                modal.created_date = _dateTime.ToString();
                modal.is_deleted = false;

                _unitOfWork.IndustryTypeRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "IndustryType registered successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdateIndustryType")]
        public async Task<object> UpdateIndustryType([FromBody] IndustryTypeViewModel industrytypeViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (industrytypeViewModel == null)
                    throw new ArgumentNullException(nameof(industrytypeViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<IndustryTypeViewModel, IndustryType>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<IndustryType>(industrytypeViewModel);
                modal.modified_date = _dateTime.ToString();

                _unitOfWork.IndustryTypeRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "IndustryType updated successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemoveIndustryType")]
        public async Task<object> RemoveIndustryType([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                _unitOfWork.IndustryTypeRepository.Remove(Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "IndustryType removed successfully." }).ConfigureAwait(false);
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

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindByIndustryTypeID")]
        public async Task<object> FindByIndustryTypeID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = _unitOfWork.IndustryTypeRepository.Find(Utils.ID);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        #endregion IndustryType

        #region LocalActivity

        [HttpPost]
        [Route("AddLocalActivity")]
        public async Task<object> AddLocalActivity([FromBody] LocalActivityViewModel socialViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (socialViewModel == null)
                    throw new ArgumentNullException(nameof(socialViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<LocalActivityViewModel, LocalActivity>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<LocalActivity>(socialViewModel);

                modal.id = Guid.NewGuid().ToString();
                modal.created_date = _dateTime.ToString();
                modal.is_deleted = false;

                _unitOfWork.LocalActivityRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Notes saved successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdateLocalActivity")]
        public async Task<object> UpdateLocalActivity([FromBody] LocalActivityViewModel socialViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (socialViewModel == null)
                    throw new ArgumentNullException(nameof(socialViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<LocalActivityViewModel, LocalActivity>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<LocalActivity>(socialViewModel);
                modal.modified_date = _dateTime.ToString();

                _unitOfWork.LocalActivityRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Notes updated successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemoveLocalActivity")]
        public async Task<object> RemoveLocalActivity([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                _unitOfWork.LocalActivityRepository.Remove(Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Notes removed successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetAllLocalActivity")]
        public async Task<object> GetAllLocalActivity(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.LocalActivityRepository.All();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindByLocalActivityID")]
        public async Task<object> FindByLocalActivityID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = _unitOfWork.LocalActivityRepository.Find(Utils.ID);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FetchLocalActivityOrgID")]
        public async Task<object> FetchLocalActivityOrgID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = _unitOfWork.LocalActivityRepository.LocalActivityByOrgID(Utils.ID);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        #endregion LocalActivity
    }
}