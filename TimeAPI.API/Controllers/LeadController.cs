using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TimeAPI.API.Models;
using TimeAPI.API.Models.LeadCompanyViewModels;
using TimeAPI.API.Models.LeadProjectViewModels;
using TimeAPI.API.Models.LeadRatingViewModels;
using TimeAPI.API.Models.LeadSourceViewModels;
using TimeAPI.API.Models.LeadStatusViewModels;
using TimeAPI.API.Models.LeadViewModels;
using TimeAPI.API.Services;
using TimeAPI.Domain;
using TimeAPI.Domain.Entities;

namespace TimeAPI.API.Controllroers
{
    //[ApiKeyAuth]
    //[EnableCors("CorsPolicy")]
    [Route("[controller]")]
    //[Authorize(Roles = "superadmin")]
    public class LeadController : Controller
    {
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly ApplicationSettings _appSettings;
        private readonly IUnitOfWork _unitOfWork;
        private readonly DateTime _dateTime;

        public LeadController(IUnitOfWork unitOfWork, ILogger<LeadController> logger,
            IEmailSender emailSender,
            IOptions<ApplicationSettings> AppSettings)
        {
            _emailSender = emailSender;
            _logger = logger;
            _appSettings = AppSettings.Value;
            _unitOfWork = unitOfWork;
            _dateTime = InternetTime.GetCurrentTimeFromTimeZone().Value.DateTime;
        }

        #region Lead

        [HttpPost]
        [Route("AddLead")]
        public async Task<object> AddLead([FromBody] LeadViewModel LeadViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (LeadViewModel == null)
                    throw new ArgumentNullException(nameof(LeadViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<LeadViewModel, Lead>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Lead>(LeadViewModel);

                modal.id = Guid.NewGuid().ToString();
                modal.created_date = _dateTime.ToString();
                modal.is_deleted = false;

                if (LeadViewModel.EntityContact != null)
                    foreach (var item in LeadViewModel.EntityContact)
                    {
                        var config1 = new AutoMapper.MapperConfiguration(m => m.CreateMap<EntityContact, EntityContact>());
                        var mapper1 = config1.CreateMapper();
                        var modal1 = mapper1.Map<EntityContact>(item);
                        modal1.id = Guid.NewGuid().ToString();
                        modal1.entity_id = modal.id;
                        _unitOfWork.EntityContactRepository.Add(modal1);
                    }

                var config2 = new AutoMapper.MapperConfiguration(m => m.CreateMap<LeadProject, LeadProject>());
                var mapper2 = config2.CreateMapper();
                var modal2 = mapper2.Map<LeadProject>(LeadViewModel.LeadProject);
                modal2.id = Guid.NewGuid().ToString();
                modal2.lead_id = modal.id;

                _unitOfWork.LeadProjectRepository.Add(modal2);
                _unitOfWork.LeadRepository.Add(modal);

                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Lead saved successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdateLead")]
        public async Task<object> UpdateLead([FromBody] LeadViewModel LeadViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (LeadViewModel == null)
                    throw new ArgumentNullException(nameof(LeadViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<LeadViewModel, Lead>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Lead>(LeadViewModel);
                modal.modified_date = _dateTime.ToString();

                if (LeadViewModel.EntityContact != null)
                {
                    _unitOfWork.EntityContactRepository.RemoveByEntityID(modal.id);
                    foreach (var item in LeadViewModel.EntityContact)
                    {
                        var config1 = new AutoMapper.MapperConfiguration(m => m.CreateMap<EntityContact, EntityContact>());
                        var mapper1 = config1.CreateMapper();
                        var modal1 = mapper1.Map<EntityContact>(item);
                        modal1.id = Guid.NewGuid().ToString();
                        modal1.entity_id = modal.id;
                        _unitOfWork.EntityContactRepository.Add(modal1);
                    }
                }

                var config2 = new AutoMapper.MapperConfiguration(m => m.CreateMap<LeadProject, LeadProject>());
                var mapper2 = config2.CreateMapper();
                var modal2 = mapper2.Map<LeadProject>(LeadViewModel.LeadProject);
                modal2.modified_date = _dateTime.ToString();
                modal2.lead_id = modal.id;

                _unitOfWork.LeadRepository.Update(modal);
                _unitOfWork.LeadProjectRepository.Update(modal2);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Lead updated successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemoveLead")]
        public async Task<object> RemoveLead([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                _unitOfWork.LeadRepository.Remove(Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Lead ID removed successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetAllLead")]
        public async Task<object> GetAllLead(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.LeadRepository.All();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindByLeadId")]
        public async Task<object> FindByLeadId([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                LeadViewModel customerViewModel = new LeadViewModel();
                List<EntityContact> ListEntityContact = new List<EntityContact>();

                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = _unitOfWork.LeadRepository.Find(Utils.ID);
                ListEntityContact = _unitOfWork.EntityContactRepository.FindByEntityListID(result.id).ToList();
                var resultLeadProject = _unitOfWork.LeadProjectRepository.LeadProjectByLeadID(result.id);

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<LeadViewModel, Lead>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Lead>(result);

                modal.EntityContact = ListEntityContact;
                modal.LeadProject = resultLeadProject;

                return await Task.FromResult<object>(modal).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("GetAllLeadByOrgID")]
        public async Task<object> GetAllLeadByOrgID([FromBody] UtilsOrgID Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.LeadRepository.LeadByOrgID(Utils.OrgID);
                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        #endregion Lead

        #region LeadCompany

        [HttpPost]
        [Route("AddLeadCompany")]
        public async Task<object> AddLeadCompany([FromBody] LeadCompanyViewModel LeadCompanyViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (LeadCompanyViewModel == null)
                    throw new ArgumentNullException(nameof(LeadCompanyViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<LeadCompanyViewModel, LeadCompany>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<LeadCompany>(LeadCompanyViewModel);

                modal.id = Guid.NewGuid().ToString();
                modal.created_date = _dateTime.ToString();
                modal.is_deleted = false;

                _unitOfWork.LeadCompanyRepository.Add(modal);

                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Lead Company saved successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdateLeadCompany")]
        public async Task<object> UpdateLeadCompany([FromBody] LeadCompanyViewModel LeadCompanyViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (LeadCompanyViewModel == null)
                    throw new ArgumentNullException(nameof(LeadCompanyViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<LeadCompanyViewModel, LeadCompany>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<LeadCompany>(LeadCompanyViewModel);
                modal.modified_date = _dateTime.ToString();

                _unitOfWork.LeadCompanyRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Lead Company updated successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemoveLeadCompany")]
        public async Task<object> RemoveLeadCompany([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                _unitOfWork.LeadCompanyRepository.Remove(Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Lead Company ID removed successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetAllLeadCompany")]
        public async Task<object> GetAllLeadCompany(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.LeadCompanyRepository.All();
                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindByLeadCompanyByID")]
        public async Task<object> FindByLeadCompanyByID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                LeadCompanyViewModel customerViewModel = new LeadCompanyViewModel();

                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = _unitOfWork.LeadCompanyRepository.Find(Utils.ID);
                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("GetAllLeadCompanyByOrgID")]
        public async Task<object> GetAllLeadCompanyByOrgID([FromBody] UtilsOrgID Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.LeadCompanyRepository.LeadCompanyByOrgID(Utils.OrgID);
                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        #endregion LeadCompany

        #region LeadProject

        [HttpPost]
        [Route("AddLeadProject")]
        public async Task<object> AddLeadProject([FromBody] LeadProjectViewModel LeadProjectViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (LeadProjectViewModel == null)
                    throw new ArgumentNullException(nameof(LeadProjectViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<LeadProjectViewModel, LeadProject>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<LeadProject>(LeadProjectViewModel);

                modal.id = Guid.NewGuid().ToString();
                modal.created_date = _dateTime.ToString();
                modal.is_deleted = false;

                _unitOfWork.LeadProjectRepository.Add(modal);

                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Lead Project saved successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdateLeadProject")]
        public async Task<object> UpdateLeadProject([FromBody] LeadProjectViewModel LeadProjectViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (LeadProjectViewModel == null)
                    throw new ArgumentNullException(nameof(LeadProjectViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<LeadProjectViewModel, LeadProject>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<LeadProject>(LeadProjectViewModel);
                modal.modified_date = _dateTime.ToString();

                _unitOfWork.LeadProjectRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Lead Project updated successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemoveLeadProject")]
        public async Task<object> RemoveLeadProject([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                _unitOfWork.LeadProjectRepository.Remove(Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Lead Project ID removed successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetAllLeadProject")]
        public async Task<object> GetAllLeadProject(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.LeadProjectRepository.All();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindByLeadProjectId")]
        public async Task<object> FindByLeadProjectId([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                LeadProjectViewModel customerViewModel = new LeadProjectViewModel();

                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = _unitOfWork.LeadProjectRepository.Find(Utils.ID);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("GetAllLeadProjectByOrgID")]
        public async Task<object> GetAllLeadProjectByOrgID([FromBody] UtilsOrgID Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.LeadProjectRepository.LeadProjectByOrgID(Utils.OrgID);
                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        #endregion LeadProject

        #region LeadSource

        [HttpPost]
        [Route("AddLeadSource")]
        public async Task<object> AddLeadSource([FromBody] LeadSourceViewModel LeadSourceViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (LeadSourceViewModel == null)
                    throw new ArgumentNullException(nameof(LeadSourceViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<LeadSourceViewModel, LeadSource>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<LeadSource>(LeadSourceViewModel);

                modal.id = Guid.NewGuid().ToString();
                modal.created_date = _dateTime.ToString();
                modal.is_deleted = false;

                _unitOfWork.LeadSourceRepository.Add(modal);

                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Lead Source saved successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdateLeadSource")]
        public async Task<object> UpdateLeadSource([FromBody] LeadSourceViewModel LeadSourceViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (LeadSourceViewModel == null)
                    throw new ArgumentNullException(nameof(LeadSourceViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<LeadSourceViewModel, LeadSource>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<LeadSource>(LeadSourceViewModel);
                modal.modified_date = _dateTime.ToString();

                _unitOfWork.LeadSourceRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Lead Source updated successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemoveLeadSource")]
        public async Task<object> RemoveLeadSource([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                _unitOfWork.LeadSourceRepository.Remove(Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Lead Source ID removed successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetAllLeadSource")]
        public async Task<object> GetAllLeadSource(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.LeadSourceRepository.All();
                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindByLeadSourceByID")]
        public async Task<object> FindByLeadSourceByID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                LeadSourceViewModel customerViewModel = new LeadSourceViewModel();

                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = _unitOfWork.LeadSourceRepository.Find(Utils.ID);
                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("GetAllLeadSourceByOrgID")]
        public async Task<object> GetAllLeadSourceByOrgID([FromBody] UtilsOrgID Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.LeadSourceRepository.LeadSourceByOrgID(Utils.OrgID);
                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        #endregion LeadSource

        #region LeadStatus

        [HttpPost]
        [Route("AddLeadStatus")]
        public async Task<object> AddLeadStatus([FromBody] LeadStatusViewModel LeadStatusViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (LeadStatusViewModel == null)
                    throw new ArgumentNullException(nameof(LeadStatusViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<LeadStatusViewModel, LeadStatus>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<LeadStatus>(LeadStatusViewModel);

                modal.id = Guid.NewGuid().ToString();
                modal.created_date = _dateTime.ToString();
                modal.is_deleted = false;

                _unitOfWork.LeadStatusRepository.Add(modal);

                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Lead Status saved successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdateLeadStatus")]
        public async Task<object> UpdateLeadStatus([FromBody] LeadStatusViewModel LeadStatusViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (LeadStatusViewModel == null)
                    throw new ArgumentNullException(nameof(LeadStatusViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<LeadStatusViewModel, LeadStatus>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<LeadStatus>(LeadStatusViewModel);
                modal.modified_date = _dateTime.ToString();

                _unitOfWork.LeadStatusRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Lead Status updated successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemoveLeadStatus")]
        public async Task<object> RemoveLeadStatus([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                _unitOfWork.LeadStatusRepository.Remove(Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Lead Status ID removed successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetAllLeadStatus")]
        public async Task<object> GetAllLeadStatus(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.LeadStatusRepository.All();
                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindByLeadStatusByID")]
        public async Task<object> FindByLeadStatusByID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                LeadStatusViewModel customerViewModel = new LeadStatusViewModel();

                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = _unitOfWork.LeadStatusRepository.Find(Utils.ID);
                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("GetAllLeadStatusByOrgID")]
        public async Task<object> GetAllLeadStatusByOrgID([FromBody] UtilsOrgID Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.LeadStatusRepository.LeadStatusByOrgID(Utils.OrgID);
                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        #endregion LeadStatus

        #region LeadRating

        [HttpPost]
        [Route("AddLeadRating")]
        public async Task<object> AddLeadRating([FromBody] LeadRatingViewModel LeadRatingViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (LeadRatingViewModel == null)
                    throw new ArgumentNullException(nameof(LeadRatingViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<LeadRatingViewModel, LeadRating>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<LeadRating>(LeadRatingViewModel);

                modal.id = Guid.NewGuid().ToString();
                modal.created_date = _dateTime.ToString();
                modal.is_deleted = false;

                _unitOfWork.LeadRatingRepository.Add(modal);

                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Lead Rating saved successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdateLeadRating")]
        public async Task<object> UpdateLeadRating([FromBody] LeadRatingViewModel LeadRatingViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (LeadRatingViewModel == null)
                    throw new ArgumentNullException(nameof(LeadRatingViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<LeadRatingViewModel, LeadRating>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<LeadRating>(LeadRatingViewModel);
                modal.modified_date = _dateTime.ToString();

                _unitOfWork.LeadRatingRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Lead Rating updated successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemoveLeadRating")]
        public async Task<object> RemoveLeadRating([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                _unitOfWork.LeadRatingRepository.Remove(Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Lead Rating ID removed successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetAllLeadRating")]
        public async Task<object> GetAllLeadRating(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.LeadRatingRepository.All();
                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindByLeadRatingByID")]
        public async Task<object> FindByLeadRatingByID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                LeadRatingViewModel customerViewModel = new LeadRatingViewModel();

                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = _unitOfWork.LeadRatingRepository.Find(Utils.ID);
                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("GetAllLeadRatingByOrgID")]
        public async Task<object> GetAllLeadRatingByOrgID([FromBody] UtilsOrgID Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.LeadRatingRepository.LeadRatingByOrgID(Utils.OrgID);
                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        #endregion LeadRating
    }
}