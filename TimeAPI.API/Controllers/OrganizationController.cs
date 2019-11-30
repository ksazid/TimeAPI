using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TimeAPI.API.Filters;
using TimeAPI.API.Models;
using TimeAPI.API.Models.OrganizationViewModels;
using TimeAPI.API.Services;
using TimeAPI.Domain;
using TimeAPI.Domain.Entities;

namespace TimeAPI.API.Controllers
{
    //[ApiKeyAuth]
    [EnableCors("CorsPolicy")]
    [Route("[controller]")]
    //[Authorize(Roles = "superadmin")]

    public class OrganizationController : Controller
    {
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly ApplicationSettings _appSettings;
        private readonly IUnitOfWork _unitOfWork;
        public OrganizationController(IUnitOfWork unitOfWork, ILogger<EmployeeController> logger,
            IEmailSender emailSender,
            IOptions<ApplicationSettings> AppSettings)
        {
            _emailSender = emailSender;
            _logger = logger;
            _appSettings = AppSettings.Value;
            _unitOfWork = unitOfWork;
        }


        [HttpPost]
        [Route("AddOrganization")]
        public async Task<object> AddOrganization([FromBody] OrganizationViewModel organizationViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (organizationViewModel == null)
                    throw new ArgumentNullException(nameof(organizationViewModel));

                //organizationViewModel.org_id = Guid.NewGuid().ToString();
                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<OrganizationViewModel, Organization>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Organization>(organizationViewModel);

                modal.org_id = Guid.NewGuid().ToString();
                modal.created_date = DateTime.Now.ToString();
                modal.is_deleted = false;

                _unitOfWork.OrganizationRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Organization Added succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }


        [HttpPost]
        [Route("UpdateOrganization")]
        public async Task<object> UpdateOrganization([FromBody] OrganizationViewModel organizationViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (organizationViewModel == null)
                    throw new ArgumentNullException(nameof(organizationViewModel));

                organizationViewModel.modified_date = DateTime.Now.ToString();
                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<OrganizationViewModel, Organization>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Organization>(organizationViewModel);

                _unitOfWork.OrganizationRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Organization Updated succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }


        [HttpPost]
        [Route("RemoveOrganization")]
        public async Task<object> RemoveOrganization([FromBody] Utils _Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (_Utils == null)
                    throw new ArgumentNullException(nameof(_Utils.ID));

                _unitOfWork.OrganizationRepository.Remove(_Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Organization remvoed succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindByOrgId")]
        public async Task<object> FindByOrgId([FromBody] Utils _Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (_Utils == null)
                    throw new ArgumentNullException(nameof(_Utils.ID));

                var result = _unitOfWork.OrganizationRepository.Find(_Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindByUsersId")]
        public async Task<object> FindByUsersId([FromBody] Utils _Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (_Utils == null)
                    throw new ArgumentNullException(nameof(_Utils.ID));

                var result = _unitOfWork.OrganizationRepository.FindByUsersID(_Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindByOrgName")]
        public async Task<object> FindOrganizationByName([FromBody] UtilsName _UtilsName, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (_UtilsName == null)
                    throw new ArgumentNullException(nameof(_UtilsName.FullName));

                var result = _unitOfWork.OrganizationRepository.FindByOrgName(_UtilsName.FullName);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }


        [Route("GetAllOrg")]
        [HttpPost]
        public async Task<object> GetAllOrg(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.OrganizationRepository.All();
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

