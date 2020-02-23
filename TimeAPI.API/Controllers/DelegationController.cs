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
using TimeAPI.Domain.Entities;
using TimeAPI.API.Models.PlanViewModels;
using TimeAPI.API.Models.DelegationsViewModels;

namespace TimeAPI.API.Controllers
{
    //[ApiKeyAuth]
    //[EnableCors("CorsPolicy")]
    [Route("[controller]")]
    //[Authorize(Roles = "superadmin")]
    public class DelegationController : Controller
    {
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly ApplicationSettings _appSettings;
        private readonly IUnitOfWork _unitOfWork;
        private static DateTime _dateTime;

        public DelegationController(IUnitOfWork unitOfWork, ILogger<DelegationController> logger,
            IEmailSender emailSender, IOptions<ApplicationSettings> AppSettings)
        {
            _emailSender = emailSender;
            _logger = logger;
            _appSettings = AppSettings.Value;
            _unitOfWork = unitOfWork;
            _dateTime = InternetTime.GetCurrentTimeFromTimeZone().Value.DateTime;
        }

        #region Delegations

        [HttpPost]
        [Route("AddDelegations")]
        public async Task<object> AddDelegations([FromBody] DelegationsViewModel planViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (planViewModel == null)
                    throw new ArgumentNullException(nameof(planViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<DelegationsViewModel, Delegations>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Delegations>(planViewModel);

                modal.id = Guid.NewGuid().ToString();
                modal.created_date = _dateTime.ToString();
                modal.is_deleted = false;

                foreach (var item in planViewModel.delegatee_emp_id)
                {
                    var Delegatee = new DelegationsDelegatee()
                    {
                        id = Guid.NewGuid().ToString(),
                        delegator_id = modal.id,
                        delegatee_id = item,
                        createdby = modal.createdby,
                        created_date = _dateTime.ToString(),
                        is_deleted = false
                    };

                    _unitOfWork.EmployeeRepository.SetCustomerAsAdminByEmpID(item);
                    _unitOfWork.DelegationsDelegateeRepository.Add(Delegatee);
                }

                _unitOfWork.DelegationsRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Delegations Added succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdateDelegations")]
        public async Task<object> UpdateDelegations([FromBody] DelegationsViewModel planViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (planViewModel == null)
                    throw new ArgumentNullException(nameof(planViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<DelegationsViewModel, Delegations>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Delegations>(planViewModel);
                modal.modified_date = _dateTime.ToString();

                _unitOfWork.DelegationsDelegateeRepository.RemoveByDelegator(modal.id);
                foreach (var item in planViewModel.delegatee_emp_id)
                {
                    var Delegatee = new DelegationsDelegatee()
                    {
                        id = Guid.NewGuid().ToString(),
                        delegator_id = modal.id,
                        delegatee_id = item,
                        createdby = modal.createdby,
                        created_date = _dateTime.ToString(),
                        is_deleted = false
                    };
                    _unitOfWork.DelegationsDelegateeRepository.Add(Delegatee);
                }

                _unitOfWork.DelegationsRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Delegations updated succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemoveDelegations")]
        public async Task<object> RemoveDelegations([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                _unitOfWork.DelegationsRepository.Remove(Utils.ID);
                _unitOfWork.DelegationsDelegateeRepository.RemoveByDelegator(Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Employee removed succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetAllDelegations")]
        public async Task<object> GetAllDelegations(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.DelegationsRepository.All();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindByDelegationsID")]
        public async Task<object> FindByDelegationsID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = _unitOfWork.DelegationsRepository.Find(Utils.ID);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("GetAllDelegateeByOrgIDAndEmpID")]
        public async Task<object> GetAllDelegateeByOrgIDAndEmpID([FromBody] UtilsOrgAndEmpID Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils));

                var result = _unitOfWork.DelegationsRepository.GetAllDelegateeByOrgIDAndEmpID(Utils.OrgID, Utils.EmpID);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemoveAdminRightByEmpID")]
        public async Task<object> RemoveAdminRightByEmpID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                _unitOfWork.DelegationsDelegateeRepository.RemoveByDelegateeID(Utils.ID);
                _unitOfWork.EmployeeRepository.RemoveAdminRightByEmpID(Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Rights removed succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        #endregion Delegations
    }
}