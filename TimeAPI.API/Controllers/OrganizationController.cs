using System;
using System.Globalization;
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
    //[EnableCors("CorsPolicy")]
    [Route("[controller]")]
    //[Authorize(Roles = "superadmin")]

    public class OrganizationController : Controller
    {
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly ApplicationSettings _appSettings;
        private readonly IUnitOfWork _unitOfWork;
        public OrganizationController(IUnitOfWork unitOfWork, ILogger<EmployeeController> logger,
            IEmailSender emailSender, IOptions<ApplicationSettings> AppSettings)
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

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<OrganizationViewModel, Organization>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Organization>(organizationViewModel);

                modal.org_id = Guid.NewGuid().ToString();
                modal.created_date = DateTime.Now.ToString(CultureInfo.CurrentCulture);
                modal.is_deleted = false;

                _unitOfWork.OrganizationRepository.Add(modal);

                if (organizationViewModel.EntityLocationViewModel != null)
                {
                    var OrgLocation = SetLocationForOrg(organizationViewModel.EntityLocationViewModel, modal.org_id);
                    OrgLocation.createdby = organizationViewModel.createdby;
                    _unitOfWork.EntityLocationRepository.Add(OrgLocation);
                }

                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Organization Added succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdateOrganization")]
        public async Task<object> UpdateOrganization([FromBody] OrganizationViewModel organizationViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (organizationViewModel == null)
                    throw new ArgumentNullException(nameof(organizationViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<OrganizationViewModel, Organization>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Organization>(organizationViewModel);
                modal.modified_date = DateTime.Now.ToString(CultureInfo.CurrentCulture);

                _unitOfWork.OrganizationRepository.Update(modal);

                if (organizationViewModel.EntityLocationViewModel != null)
                {
                    var EntityLocation = SetUpdateOrgAddress(organizationViewModel, modal);
                    _unitOfWork.EntityLocationRepository.Update(EntityLocation);
                }

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
        public async Task<object> RemoveOrganization([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                _unitOfWork.OrganizationRepository.Remove(Utils.ID);
                _unitOfWork.EntityLocationRepository.Remove(Utils.ID);
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
        public async Task<object> FindByOrgId([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = _unitOfWork.OrganizationRepository.Find(Utils.ID);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindByUsersId")]
        public async Task<object> FindByUsersId([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = _unitOfWork.OrganizationRepository.FindByUsersID(Utils.ID);
                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindByOrgName")]
        public async Task<object> FindOrganizationByName([FromBody] UtilsName UtilsName, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (UtilsName == null)
                    throw new ArgumentNullException(nameof(UtilsName.FullName));

                var result = _unitOfWork.OrganizationRepository.FindByOrgName(UtilsName.FullName);
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
                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("AddOrganizationBranch")]
        public async Task<object> AddOrganizationBranch([FromBody] OrganizationBranchViewModel organizationBranchViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (organizationBranchViewModel == null)
                    throw new ArgumentNullException(nameof(organizationBranchViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<OrganizationBranchViewModel, Organization>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Organization>(organizationBranchViewModel);

                modal.org_id = Guid.NewGuid().ToString();
                modal.created_date = DateTime.Now.ToString(CultureInfo.CurrentCulture);
                modal.is_deleted = false;

                _unitOfWork.OrganizationRepository.Add(modal);

                if (organizationBranchViewModel.EntityLocationViewModel != null)
                {
                    var OrgLocation = SetLocationForOrg(organizationBranchViewModel.EntityLocationViewModel, modal.org_id);
                    OrgLocation.createdby = organizationBranchViewModel.createdby;
                    _unitOfWork.EntityLocationRepository.Add(OrgLocation);
                }

                var OrgBranch = new OrganizationBranch()
                {
                    id = Guid.NewGuid().ToString(),
                    parent_org_id = organizationBranchViewModel.parent_org_id,
                    org_id = modal.org_id,
                    createdby = organizationBranchViewModel.createdby,
                    created_date = DateTime.Now.ToString(CultureInfo.CurrentCulture),
                    is_deleted = false
                };
                _unitOfWork.OrganizationBranchRepository.Add(OrgBranch);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Organization Added succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        private EntityLocation SetLocationForOrg(EntityLocationViewModel OrgLocation, string OrgID)
        {
            var EntityLocation = new EntityLocation()
            {
                id = Guid.NewGuid().ToString(),
                entity_id = OrgID,
                formatted_address = OrgLocation.formatted_address,
                lat = OrgLocation.lat,
                lang = OrgLocation.lang,
                street_number = OrgLocation.street_number,
                route = OrgLocation.route,
                locality = OrgLocation.locality,
                administrative_area_level_2 = OrgLocation.administrative_area_level_2,
                administrative_area_level_1 = OrgLocation.administrative_area_level_1,
                postal_code = OrgLocation.postal_code,
                country = OrgLocation.country,
                created_date = DateTime.Now.ToString(CultureInfo.CurrentCulture),
                is_deleted = false
            };
            return EntityLocation;
        }

        private static EntityLocation SetUpdateOrgAddress(OrganizationViewModel organizationViewModel, Organization modal)
        {
            var OrgLocation = organizationViewModel.EntityLocationViewModel;
            var EntityLocation = new EntityLocation()
            {
                entity_id = modal.org_id,
                formatted_address = OrgLocation.formatted_address,
                lat = OrgLocation.lat,
                lang = OrgLocation.lang,
                street_number = OrgLocation.street_number,
                route = OrgLocation.route,
                locality = OrgLocation.locality,
                administrative_area_level_2 = OrgLocation.administrative_area_level_2,
                administrative_area_level_1 = OrgLocation.administrative_area_level_1,
                postal_code = OrgLocation.postal_code,
                country = OrgLocation.country,
                modified_date = DateTime.Now.ToString(CultureInfo.CurrentCulture),
                modifiedby = organizationViewModel.createdby
            };
            return EntityLocation;
        }

    }
}

