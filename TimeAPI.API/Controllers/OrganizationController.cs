using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TimeAPI.API.Models;
using TimeAPI.API.Models.EntityLocationViewModels;
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
        private readonly DateTime _dateTime;

        public OrganizationController(IUnitOfWork unitOfWork, ILogger<EmployeeController> logger,
                                        IEmailSender emailSender, IOptions<ApplicationSettings> AppSettings)
        {
            _emailSender = emailSender;
            _logger = logger;
            _appSettings = AppSettings.Value;
            _unitOfWork = unitOfWork;
            _dateTime = InternetTime.GetCurrentTimeFromTimeZone().Value.DateTime;
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
                modal.createdby = organizationViewModel.createdby;
                modal.created_date = _dateTime.ToString();
                modal.is_deleted = false;

                //SuperAdminXOrg
                var result = _unitOfWork.EmployeeRepository.FindByEmpUserID(modal.user_id);

                if (result.is_superadmin)
                {
                    var SuperAdminXOrg = new SuperadminOrganization()
                    {
                        id = Guid.NewGuid().ToString(),
                        superadmin_empid = result.id,
                        org_id = modal.org_id,
                        created_date = _dateTime.ToString(),
                        is_deleted = false,
                        createdby = organizationViewModel.createdby
                    };

                    _unitOfWork.SuperadminOrganizationRepository.Add(SuperAdminXOrg);
                }

                if (organizationViewModel.EntityLocationViewModel != null)
                {
                    var OrgLocation = SetLocationForOrg(organizationViewModel.EntityLocationViewModel, modal.org_id.ToString());
                    OrgLocation.id = Guid.NewGuid().ToString();
                    OrgLocation.created_date = _dateTime.ToString();
                    OrgLocation.is_deleted = false;
                    OrgLocation.createdby = organizationViewModel.createdby;
                    _unitOfWork.EntityLocationRepository.Add(OrgLocation);
                }

                if (organizationViewModel.OrganizationSetup != null)
                {
                    var configs = new AutoMapper.MapperConfiguration(m => m.CreateMap<OrganizationSetup, OrganizationSetup>());
                    var mapper1 = config.CreateMapper();
                    var modal1 = mapper.Map<OrganizationSetup>(organizationViewModel.OrganizationSetup);

                    modal1.id = Guid.NewGuid().ToString();
                    modal1.org_id = modal.org_id;
                    modal1.createdby = organizationViewModel.createdby;
                    modal1.created_date = _dateTime.ToString();
                    modal1.is_deleted = false;
                    modal1.fiscal_year = organizationViewModel.OrganizationSetup.fiscal_year;
                    modal1.start_of_week = organizationViewModel.OrganizationSetup.start_of_week;

                    _unitOfWork.OrganizationSetupRepository.Add(modal1);
                }

                //Adding Branch
                AddBranchOrg(organizationViewModel, modal.org_id);

                _unitOfWork.OrganizationRepository.Add(modal);
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
                modal.modified_date = _dateTime.ToString();

                var result = _unitOfWork.EntityLocationRepository.FindByEnitiyID(modal.org_id);
                var result1 = _unitOfWork.OrganizationSetupRepository.FindByEnitiyID(modal.org_id);
                AddOrUpdateSetupAndLocation(organizationViewModel, config, mapper, modal, result, result1);

                //Update Branch
                UpdateBranchOrg(organizationViewModel, modal.org_id);
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
        public async Task<object> RemoveOrganization([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                _unitOfWork.EntityLocationRepository.Remove(Utils.ID);
                _unitOfWork.SuperadminOrganizationRepository.RemoveByOrgID(Utils.ID);
                _unitOfWork.OrganizationSetupRepository.RemoveByOrgID(Utils.ID);
                _unitOfWork.OrganizationRepository.Remove(Utils.ID);

                var ListBranch = _unitOfWork.OrganizationBranchRepository.FindByParentOrgID(Utils.ID).ToList();
                for (int i = 0; i < ListBranch.Count; i++)
                {
                    _unitOfWork.OrganizationRepository.Remove(ListBranch[i].org_id);
                    _unitOfWork.OrganizationBranchRepository.Remove(Utils.ID);
                    //_unitOfWork.SuperadminOrganizationRepository.RemoveByOrgID(ListBranch[i].org_id);
                    _unitOfWork.EntityLocationRepository.Remove(ListBranch[i].org_id);
                    _unitOfWork.OrganizationSetupRepository.RemoveByOrgID(ListBranch[i].org_id);
                }
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Organization removed succefully." }).ConfigureAwait(false);
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
                OrganizationViewModel organizationViewModel = new OrganizationViewModel();

                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = _unitOfWork.OrganizationRepository.Find(Utils.ID);
                var resultLocation = _unitOfWork.EntityLocationRepository.FindByEnitiyID(result.org_id);
                var resultSetup = _unitOfWork.OrganizationSetupRepository.FindByEnitiyID(result.org_id);
                var resultBranch = _unitOfWork.OrganizationRepository.FindByAllBranchByParengOrgID(result.org_id).ToList();

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<OrganizationViewModel, Organization>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Organization>(result);

                modal.EntityLocation = resultLocation;
                modal.OrganizationSetup = resultSetup;

                modal.OrganizationBranchViewModel = resultBranch;

                return await Task.FromResult<object>(modal).ConfigureAwait(false);
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

        #region helper
        
        //not in use
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
                modal.created_date = _dateTime.ToString();
                modal.is_deleted = false;

                if (organizationBranchViewModel.EntityLocationViewModel != null)
                {
                    var OrgLocation = SetLocationForOrg(organizationBranchViewModel.EntityLocationViewModel, modal.org_id.ToString());
                    OrgLocation.createdby = organizationBranchViewModel.createdby;
                    _unitOfWork.EntityLocationRepository.Add(OrgLocation);
                }

                var OrgBranch = new OrganizationBranch()
                {
                    id = Guid.NewGuid().ToString(),
                    parent_org_id = organizationBranchViewModel.parent_org_id,
                    org_id = modal.org_id,
                    createdby = organizationBranchViewModel.createdby,
                    created_date = _dateTime.ToString(),
                    is_deleted = false
                };

                _unitOfWork.OrganizationRepository.Add(modal);
                _unitOfWork.OrganizationBranchRepository.Add(OrgBranch);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Organization Added succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        private EntityLocation SetLocationForOrg(EntityLocation OrgLocation, string OrgID)
        {
            var EntityLocation = new EntityLocation()
            {
                entity_id = OrgID,
                geo_address = OrgLocation.geo_address,
                formatted_address = OrgLocation.formatted_address,
                lat = OrgLocation.lat,
                lang = OrgLocation.lang,
                street_number = OrgLocation.street_number,
                route = OrgLocation.route,
                locality = OrgLocation.locality,
                administrative_area_level_2 = OrgLocation.administrative_area_level_2,
                administrative_area_level_1 = OrgLocation.administrative_area_level_1,
                postal_code = OrgLocation.postal_code,
                country = OrgLocation.country
            };
            return EntityLocation;
        }

        private void AddBranchOrg(OrganizationViewModel organizationViewModel, string org_id)
        {
            if (organizationViewModel.OrganizationBranchViewModel != null)
            {
                var ListOfBranch = organizationViewModel.OrganizationBranchViewModel;
                for (int i = 0; i < ListOfBranch.Count; i++)
                {
                    OrganizationBranchViewModel organization = ListOfBranch[i];
                    var configBranch = new AutoMapper.MapperConfiguration(m => m.CreateMap<OrganizationBranchViewModel, Organization>());
                    var mapperBranch = configBranch.CreateMapper();
                    var modalBranch = mapperBranch.Map<Organization>(organization);

                    if (modalBranch.org_id == null)
                    {
                        modalBranch.org_id = Guid.NewGuid().ToString();
                        modalBranch.createdby = organizationViewModel.createdby;
                        modalBranch.created_date = _dateTime.ToString();
                        modalBranch.is_deleted = false;

                        //saving in x table
                        var OrgBranch = new OrganizationBranch()
                        {
                            id = Guid.NewGuid().ToString(),
                            parent_org_id = org_id,
                            org_id = modalBranch.org_id,
                            createdby = organizationViewModel.createdby,
                            created_date = _dateTime.ToString(),
                            is_deleted = false
                        };
                        _unitOfWork.OrganizationBranchRepository.Add(OrgBranch);

                        //setup for branch
                        if (organizationViewModel.OrganizationSetup != null)
                        {
                            var configsBranchSetup = new AutoMapper.MapperConfiguration(m => m.CreateMap<OrganizationSetup, OrganizationSetup>());
                            var mapperBranchSetup = configsBranchSetup.CreateMapper();
                            var modalBranchSetup = mapperBranchSetup.Map<OrganizationSetup>(organizationViewModel.OrganizationSetup);

                            modalBranchSetup.id = Guid.NewGuid().ToString();
                            modalBranchSetup.org_id = modalBranch.org_id;
                            modalBranchSetup.createdby = organizationViewModel.createdby;
                            modalBranchSetup.created_date = _dateTime.ToString();
                            modalBranchSetup.is_deleted = false;
                            modalBranchSetup.fiscal_year = modalBranchSetup.fiscal_year;
                            modalBranchSetup.start_of_week = modalBranchSetup.start_of_week;

                            _unitOfWork.OrganizationSetupRepository.Add(modalBranchSetup);
                        }

                        //Location for branch
                        if (ListOfBranch[i].EntityLocationViewModel != null)
                        {
                            var OrgLocation = SetLocationForOrg(ListOfBranch[i].EntityLocationViewModel, modalBranch.org_id.ToString());
                            OrgLocation.id = Guid.NewGuid().ToString();
                            OrgLocation.createdby = organizationViewModel.createdby;
                            OrgLocation.created_date = _dateTime.ToString();
                            OrgLocation.is_deleted = false;
                            OrgLocation.createdby = organizationViewModel.createdby;
                            _unitOfWork.EntityLocationRepository.Add(OrgLocation);
                        }

                        _unitOfWork.OrganizationRepository.Add(modalBranch);
                    }
                }
            }
        }

        private void UpdateBranchOrg(OrganizationViewModel organizationViewModel, string org_id)
        {
            if (organizationViewModel.OrganizationBranchViewModel != null)
            {
                var ListOfBranch = organizationViewModel.OrganizationBranchViewModel;
                string orgid = string.Empty;
                for (int i = 0; i < ListOfBranch.Count; i++)
                {
                    OrganizationBranchViewModel organization = ListOfBranch[i];
                    var configBranch = new AutoMapper.MapperConfiguration(m => m.CreateMap<OrganizationBranchViewModel, Organization>());
                    var mapperBranch = configBranch.CreateMapper();
                    var modalBranch = mapperBranch.Map<Organization>(organization);
                    modalBranch.modifiedby = organizationViewModel.createdby;
                    modalBranch.modified_date = _dateTime.ToString();
                    modalBranch.is_deleted = false;

                    // need to paas only the id for create new not whole list......
                    if (modalBranch.org_id == null)
                    {
                        AddBranchOrg(organizationViewModel, org_id);
                    }
                    else
                    {
                        //saving in x table
                        var OrgBranch = new OrganizationBranch()
                        {
                            parent_org_id = org_id,
                            org_id = modalBranch.org_id,
                            modifiedby = organizationViewModel.createdby,
                            modified_date = _dateTime.ToString(),
                            is_deleted = false
                        };
                        _unitOfWork.OrganizationBranchRepository.Update(OrgBranch);

                        //setup for branch
                        if (organizationViewModel.OrganizationSetup != null)
                        {
                            var configsBranchSetup = new AutoMapper.MapperConfiguration(m => m.CreateMap<OrganizationSetup, OrganizationSetup>());
                            var mapperBranchSetup = configsBranchSetup.CreateMapper();
                            var modalBranchSetup = mapperBranchSetup.Map<OrganizationSetup>(organizationViewModel.OrganizationSetup);

                            modalBranchSetup.fiscal_year = modalBranchSetup.fiscal_year;
                            modalBranchSetup.org_id = modalBranch.org_id;
                            modalBranchSetup.modifiedby = organizationViewModel.createdby;
                            modalBranchSetup.modified_date = _dateTime.ToString();
                            modalBranchSetup.is_deleted = false;

                            _unitOfWork.OrganizationSetupRepository.Update(modalBranchSetup);
                        }

                        //Location for branch
                        if (ListOfBranch[i].EntityLocationViewModel != null)
                        {
                            var OrgLocation = SetLocationForOrg(ListOfBranch[i].EntityLocationViewModel, orgid);
                            OrgLocation.modifiedby = organizationViewModel.createdby;
                            OrgLocation.modified_date = _dateTime.ToString();
                            OrgLocation.is_deleted = false;
                            _unitOfWork.EntityLocationRepository.Update(OrgLocation);
                        }

                        _unitOfWork.OrganizationRepository.Update(modalBranch);
                    }
                }
            }
        }

        private void AddOrUpdateSetupAndLocation(OrganizationViewModel organizationViewModel, MapperConfiguration config,
            IMapper mapper, Organization modal, EntityLocation result, OrganizationSetup result1)
        {
            if (result != null)
            {
                if (organizationViewModel.EntityLocationViewModel != null)
                {
                    //var EntityLocation = SetUpdateOrgAddress(organizationViewModel, modal);
                    var EntityLocation = SetLocationForOrg(organizationViewModel.EntityLocationViewModel, modal.org_id.ToString());
                    EntityLocation.modified_date = _dateTime.ToString();
                    EntityLocation.modifiedby = organizationViewModel.createdby;

                    _unitOfWork.EntityLocationRepository.Update(EntityLocation);
                }
            }
            else
            {
                if (organizationViewModel.EntityLocationViewModel != null)
                {
                    var OrgLocation = SetLocationForOrg(organizationViewModel.EntityLocationViewModel, modal.org_id.ToString());
                    OrgLocation.id = Guid.NewGuid().ToString();
                    OrgLocation.created_date = _dateTime.ToString();
                    OrgLocation.is_deleted = false;
                    OrgLocation.createdby = organizationViewModel.createdby;
                    _unitOfWork.EntityLocationRepository.Add(OrgLocation);
                }
            }

            if (result1 != null)
            {
                if (organizationViewModel.OrganizationSetup != null)
                {
                    var configs = new AutoMapper.MapperConfiguration(m => m.CreateMap<OrganizationSetup, OrganizationSetup>());
                    var mapper1 = config.CreateMapper();
                    var modal1 = mapper.Map<OrganizationSetup>(organizationViewModel.OrganizationSetup);
                    modal1.org_id = organizationViewModel.org_id;
                    modal1.modified_date = _dateTime.ToString();
                    modal1.is_deleted = false;


                    _unitOfWork.OrganizationSetupRepository.Update(modal1);
                }
            }
            else
            {
                if (organizationViewModel.OrganizationSetup != null)
                {
                    var configs = new AutoMapper.MapperConfiguration(m => m.CreateMap<OrganizationSetup, OrganizationSetup>());
                    var mapper1 = config.CreateMapper();
                    var modal1 = mapper.Map<OrganizationSetup>(organizationViewModel.OrganizationSetup);

                    modal1.id = Guid.NewGuid().ToString();
                    modal1.org_id = modal.org_id;
                    modal1.createdby = organizationViewModel.createdby;
                    modal1.created_date = _dateTime.ToString();
                    modal1.is_deleted = false;

                    _unitOfWork.OrganizationSetupRepository.Add(modal1);
                }
            }
        }

        #endregion helper
    }
}