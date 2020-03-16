using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using TimeAPI.API.Models;
using TimeAPI.API.Models.DelegationsViewModels;
using TimeAPI.API.Services;
using TimeAPI.Domain;
using TimeAPI.Domain.Entities;

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

                var Role = _unitOfWork.RoleRepository.Find(planViewModel.role_id);

                foreach (var item in planViewModel.delegatee_emp_id)
                {
                    var Delegatee = new DelegationsDelegatee()
                    {
                        id = Guid.NewGuid().ToString(),
                        delegator_id = modal.id,
                        delegatee_id = item,
                        is_type_temporary = planViewModel.is_type_temporary,
                        expires_on = planViewModel.expires_on,
                        is_type_permanent = planViewModel.is_type_permanent,
                        is_notify_delegator_and_delegatee = planViewModel.is_notify_delegator_and_delegatee,
                        is_notify_delegatee = planViewModel.is_notify_delegatee,
                        role_id = planViewModel.role_id,
                        createdby = modal.createdby,
                        created_date = _dateTime.ToString(),
                        is_deleted = false
                    };

                    if (Role.NormalizedName.Equals("ADMIN"))
                        _unitOfWork.EmployeeRepository.SetDelegateeAsAdminByEmpID(item);

                    if (Role.NormalizedName.Equals("SUPERADMIN"))
                        _unitOfWork.EmployeeRepository.SetDelegateeAsSuperAdminByEmpID(item);

                    _unitOfWork.DelegationsDelegateeRepository.Add(Delegatee);
                }

                foreach (var item in planViewModel.invitees)
                {
                    string entityid, emp_id = string.Empty;
                    entityid = Guid.NewGuid().ToString();
                    emp_id = Guid.NewGuid().ToString();

                    var Delegatee = new DelegationsDelegatee()
                    {
                        id = entityid,
                        delegator_id = modal.id,
                        delegatee_id = emp_id,
                        is_type_temporary = planViewModel.is_type_temporary,
                        expires_on = planViewModel.expires_on,
                        is_type_permanent = planViewModel.is_type_permanent,
                        is_notify_delegator_and_delegatee = planViewModel.is_notify_delegator_and_delegatee,
                        is_notify_delegatee = planViewModel.is_notify_delegatee,
                        role_id = planViewModel.role_id,
                        createdby = modal.createdby,
                        created_date = _dateTime.ToString(),
                        is_deleted = false
                    };

                    var EntityInvitation = new EntityInvitation()
                    {
                        id = Guid.NewGuid().ToString(),
                        entity_id = entityid,
                        org_id = modal.org_id,
                        emp_id = emp_id,
                        email = item,
                        role_id = planViewModel.role_id,
                        createdby = modal.createdby,
                        created_date = _dateTime.ToString(),
                        is_deleted = false

                    };

                    if (Role.NormalizedName.Equals("ADMIN"))
                        _unitOfWork.EmployeeRepository.SetDelegateeAsAdminByEmpID(item);

                    if (Role.NormalizedName.Equals("SUPERADMIN"))
                        _unitOfWork.EmployeeRepository.SetDelegateeAsSuperAdminByEmpID(item);

                    _unitOfWork.EntityInvitationRepository.Add(EntityInvitation);
                }

                _unitOfWork.DelegationsRepository.Add(modal);
                if (_unitOfWork.Commit())
                { 
                    //send email
                }

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

                var Role = _unitOfWork.RoleRepository.Find(planViewModel.role_id);

                foreach (var item in planViewModel.delegatee_emp_id)
                {
                    var Delegatee = new DelegationsDelegatee()
                    {
                        delegator_id = modal.id,
                        delegatee_id = item,
                        is_type_temporary = planViewModel.is_type_temporary,
                        expires_on = planViewModel.expires_on,
                        is_type_permanent = planViewModel.is_type_permanent,
                        is_notify_delegator_and_delegatee = planViewModel.is_notify_delegator_and_delegatee,
                        is_notify_delegatee = planViewModel.is_notify_delegatee,
                        role_id = planViewModel.role_id,
                        modifiedby = modal.createdby,
                        modified_date = _dateTime.ToString(),
                        is_deleted = false
                    };

                    if (Role.NormalizedName.Equals("ADMIN"))
                        _unitOfWork.EmployeeRepository.SetDelegateeAsAdminByEmpID(item);

                    if (Role.NormalizedName.Equals("SUPERADMIN"))
                        _unitOfWork.EmployeeRepository.SetDelegateeAsSuperAdminByEmpID(item);

                    _unitOfWork.DelegationsDelegateeRepository.Update(Delegatee);
                }

                foreach (var item in planViewModel.invitees)
                {
                    string entityid, emp_id = string.Empty;
                    entityid = Guid.NewGuid().ToString();
                    emp_id = Guid.NewGuid().ToString();

                    var Delegatee = new DelegationsDelegatee()
                    {
                        id = entityid,
                        delegator_id = modal.id,
                        delegatee_id = emp_id,
                        is_type_temporary = planViewModel.is_type_temporary,
                        expires_on = planViewModel.expires_on,
                        is_type_permanent = planViewModel.is_type_permanent,
                        is_notify_delegator_and_delegatee = planViewModel.is_notify_delegator_and_delegatee,
                        is_notify_delegatee = planViewModel.is_notify_delegatee,
                        role_id = planViewModel.role_id,
                        createdby = modal.createdby,
                        created_date = _dateTime.ToString(),
                        is_deleted = false
                    };

                    var EntityInvitation = new EntityInvitation()
                    {
                        id = Guid.NewGuid().ToString(),
                        entity_id = entityid,
                        org_id = modal.org_id,
                        emp_id = emp_id,
                        email = item,
                        role_id = planViewModel.role_id,
                        createdby = modal.createdby,
                        created_date = _dateTime.ToString(),
                        is_deleted = false

                    };

                    if (Role.NormalizedName.Equals("ADMIN"))
                        _unitOfWork.EmployeeRepository.SetDelegateeAsAdminByEmpID(item);

                    if (Role.NormalizedName.Equals("SUPERADMIN"))
                        _unitOfWork.EmployeeRepository.SetDelegateeAsSuperAdminByEmpID(item);



                    _unitOfWork.EntityInvitationRepository.Add(EntityInvitation);


                    //send invitation mail
                }

                _unitOfWork.DelegationsRepository.Update(modal);
                if (_unitOfWork.Commit())
                {
                    //send email
                }

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
                _unitOfWork.EntityInvitationRepository.RemoveByEntityID(Utils.ID);
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

        [HttpPost]
        [Route("FindByDelegateesID")]
        public async Task<object> FindByDelegateesID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils));

                var result = _unitOfWork.DelegationsRepository.FindByDelegateesID(Utils.ID);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemoveByDelegateeID")]
        public async Task<object> RemoveByDelegateeID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                _unitOfWork.DelegationsDelegateeRepository.RemoveByDelegateeID(Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Employee removed succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        #endregion Delegations
    }
}