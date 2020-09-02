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
using TimeAPI.API.Models.LeadDealTypeViewModels;
using TimeAPI.API.Models.LeadDealViewModels;
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

                if (LeadViewModel.LeadDeal != null)
                {
                    var config2 = new AutoMapper.MapperConfiguration(m => m.CreateMap<LeadDeal, LeadDeal>());
                    var mapper2 = config2.CreateMapper();
                    var modal2 = mapper2.Map<LeadDeal>(LeadViewModel.LeadDeal);
                    modal2.id = Guid.NewGuid().ToString();
                    modal2.lead_id = modal.id;
                    modal2.created_date = _dateTime.ToString();

                    _unitOfWork.LeadDealRepository.Add(modal2);
                }

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

                var config2 = new AutoMapper.MapperConfiguration(m => m.CreateMap<LeadDeal, LeadDeal>());
                var mapper2 = config2.CreateMapper();
                var modal2 = mapper2.Map<LeadDeal>(LeadViewModel.LeadDeal);
                modal2.modified_date = _dateTime.ToString();
                modal2.lead_id = modal.id;

                _unitOfWork.LeadRepository.Update(modal);
                _unitOfWork.LeadDealRepository.Update(modal2);
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

                var result = _unitOfWork.LeadRepository.FindByLeadID(Utils.ID);
                ListEntityContact = _unitOfWork.EntityContactRepository.FindByEntityListID(result.id);
                var resultLeadDeal = _unitOfWork.LeadDealRepository.LeadDealByLeadID(result.id);

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<LeadViewResponseModel, LeadViewResponseModel>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<LeadViewResponseModel>(result);

                modal.EntityContact = ListEntityContact;
                modal.LeadDeal = resultLeadDeal;

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

        [HttpPatch]
        [Route("UpdateLeadStatusByLeadID")]
        public async Task<object> UpdateLeadStatusByLeadID([FromBody] LeadStatusUpdateViewModel LeadViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (LeadViewModel == null)
                    throw new ArgumentNullException(nameof(LeadViewModel));

                if (LeadViewModel != null)
                {
                    Lead modal = new Lead()
                    {
                        id = LeadViewModel.id,
                        lead_status_id = LeadViewModel.lead_status_id,
                        modifiedby = LeadViewModel.modifiedby,
                        modified_date = _dateTime.ToString()
                    };

                    if (LeadViewModel.LeadDeal != null)
                    {
                        LeadDeal modal2 = new LeadDeal()
                        {
                            lead_id = LeadViewModel.LeadDeal.lead_id,
                            est_amount = LeadViewModel.LeadDeal.est_amount,
                            is_manual = LeadViewModel.LeadDeal.is_manual,
                            basic_cost = LeadViewModel.LeadDeal.basic_cost,
                            remarks = LeadViewModel.LeadDeal.remarks,
                            modifiedby = LeadViewModel.modifiedby,
                            modified_date = _dateTime.ToString()
                        };

                        _unitOfWork.LeadDealRepository.UpdateEstDealValueByLeadID(modal2);
                    };

                    _unitOfWork.LeadRepository.UpdateLeadStatusByLeadID(modal);

                }

                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Lead Status Updated Successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }


        [HttpPost]
        [Route("GetLastAddedLeadPrefixByOrgID")]
        public async Task<object> GetLastAddedLeadPrefixByOrgID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.LeadDealRepository.GetLastAddedLeadPrefixByOrgID(Utils.ID);
                return await Task.FromResult<object>(new SuccessViewModel
                {
                    Status = "200",
                    Code = (result ?? string.Empty).ToString(),
                    Desc = ""
                }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }


        #endregion Lead

        #region LeadDeal

        [HttpPost]
        [Route("AddLeadDeal")]
        public async Task<object> AddLeadDeal([FromBody] LeadDealViewModel LeadDealViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (LeadDealViewModel == null)
                    throw new ArgumentNullException(nameof(LeadDealViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<LeadDealViewModel, LeadDeal>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<LeadDeal>(LeadDealViewModel);

                modal.id = Guid.NewGuid().ToString();
                modal.created_date = _dateTime.ToString();
                modal.is_deleted = false;

                _unitOfWork.LeadDealRepository.Add(modal);

                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Lead Project saved successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdateLeadDeal")]
        public async Task<object> UpdateLeadDeal([FromBody] LeadDealViewModel LeadDealViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (LeadDealViewModel == null)
                    throw new ArgumentNullException(nameof(LeadDealViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<LeadDealViewModel, LeadDeal>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<LeadDeal>(LeadDealViewModel);
                modal.modified_date = _dateTime.ToString();

                _unitOfWork.LeadDealRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Lead Project updated successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemoveLeadDeal")]
        public async Task<object> RemoveLeadDeal([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                _unitOfWork.LeadDealRepository.Remove(Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Lead Project ID removed successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetAllLeadDeal")]
        public async Task<object> GetAllLeadDeal(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.LeadDealRepository.All();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindByLeadDealId")]
        public async Task<object> FindByLeadDealId([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                LeadDealViewModel customerViewModel = new LeadDealViewModel();

                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = _unitOfWork.LeadDealRepository.Find(Utils.ID);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("GetAllLeadDealByOrgID")]
        public async Task<object> GetAllLeadDealByOrgID([FromBody] UtilsOrgID Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.LeadDealRepository.LeadDealByOrgID(Utils.OrgID);
                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        #endregion LeadDeal

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

        #region LeadDealType

        [HttpPost]
        [Route("AddLeadDealType")]
        public async Task<object> AddLeadDealType([FromBody] LeadDealTypeViewModel LeadDealTypeViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (LeadDealTypeViewModel == null)
                    throw new ArgumentNullException(nameof(LeadDealTypeViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<LeadDealTypeViewModel, LeadDealType>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<LeadDealType>(LeadDealTypeViewModel);

                modal.id = Guid.NewGuid().ToString();
                modal.created_date = _dateTime.ToString();
                modal.is_deleted = false;

                _unitOfWork.LeadDealTypeRepository.Add(modal);

                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Lead Status saved successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdateLeadDealType")]
        public async Task<object> UpdateLeadDealType([FromBody] LeadDealTypeViewModel LeadDealTypeViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (LeadDealTypeViewModel == null)
                    throw new ArgumentNullException(nameof(LeadDealTypeViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<LeadDealTypeViewModel, LeadDealType>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<LeadDealType>(LeadDealTypeViewModel);
                modal.modified_date = _dateTime.ToString();

                _unitOfWork.LeadDealTypeRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Lead Status updated successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemoveLeadDealType")]
        public async Task<object> RemoveLeadDealType([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                _unitOfWork.LeadDealTypeRepository.Remove(Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Lead Status ID removed successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetAllLeadDealType")]
        public async Task<object> GetAllLeadDealType(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.LeadDealTypeRepository.All();
                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindByLeadDealTypeByID")]
        public async Task<object> FindByLeadDealTypeByID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                LeadDealTypeViewModel customerViewModel = new LeadDealTypeViewModel();

                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = _unitOfWork.LeadDealTypeRepository.Find(Utils.ID);
                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("GetAllLeadDealTypeByOrgID")]
        public async Task<object> GetAllLeadDealTypeByOrgID([FromBody] UtilsOrgID Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.LeadDealTypeRepository.GetLeadDealTypeByOrgID(Utils.OrgID);
                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        #endregion LeadDealType

        #region  Rating_ContractNotInUse

        //#region LeadRating

        //[HttpPost]
        //[Route("AddLeadRating")]
        //public async Task<object> AddLeadRating([FromBody] LeadRatingViewModel LeadRatingViewModel, CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        if (cancellationToken != null)
        //            cancellationToken.ThrowIfCancellationRequested();

        //        if (LeadRatingViewModel == null)
        //            throw new ArgumentNullException(nameof(LeadRatingViewModel));

        //        var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<LeadRatingViewModel, LeadRating>());
        //        var mapper = config.CreateMapper();
        //        var modal = mapper.Map<LeadRating>(LeadRatingViewModel);

        //        modal.id = Guid.NewGuid().ToString();
        //        modal.created_date = _dateTime.ToString();
        //        modal.is_deleted = false;

        //        _unitOfWork.LeadRatingRepository.Add(modal);

        //        _unitOfWork.Commit();

        //        return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Lead Rating saved successfully." }).ConfigureAwait(false);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
        //    }
        //}

        //[HttpPatch]
        //[Route("UpdateLeadRating")]
        //public async Task<object> UpdateLeadRating([FromBody] LeadRatingViewModel LeadRatingViewModel, CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        if (cancellationToken != null)
        //            cancellationToken.ThrowIfCancellationRequested();

        //        if (LeadRatingViewModel == null)
        //            throw new ArgumentNullException(nameof(LeadRatingViewModel));

        //        var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<LeadRatingViewModel, LeadRating>());
        //        var mapper = config.CreateMapper();
        //        var modal = mapper.Map<LeadRating>(LeadRatingViewModel);
        //        modal.modified_date = _dateTime.ToString();

        //        _unitOfWork.LeadRatingRepository.Update(modal);
        //        _unitOfWork.Commit();

        //        return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Lead Rating updated successfully." }).ConfigureAwait(false);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
        //    }
        //}

        //[HttpPost]
        //[Route("RemoveLeadRating")]
        //public async Task<object> RemoveLeadRating([FromBody] Utils Utils, CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        if (cancellationToken != null)
        //            cancellationToken.ThrowIfCancellationRequested();

        //        if (Utils == null)
        //            throw new ArgumentNullException(nameof(Utils.ID));

        //        _unitOfWork.LeadRatingRepository.Remove(Utils.ID);
        //        _unitOfWork.Commit();

        //        return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Lead Rating ID removed successfully." }).ConfigureAwait(false);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
        //    }
        //}

        //[HttpGet]
        //[Route("GetAllLeadRating")]
        //public async Task<object> GetAllLeadRating(CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        if (cancellationToken != null)
        //            cancellationToken.ThrowIfCancellationRequested();

        //        var result = _unitOfWork.LeadRatingRepository.All();
        //        return await Task.FromResult<object>(result).ConfigureAwait(false);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
        //    }
        //}

        //[HttpPost]
        //[Route("FindByLeadRatingByID")]
        //public async Task<object> FindByLeadRatingByID([FromBody] Utils Utils, CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        LeadRatingViewModel customerViewModel = new LeadRatingViewModel();

        //        if (cancellationToken != null)
        //            cancellationToken.ThrowIfCancellationRequested();

        //        if (Utils == null)
        //            throw new ArgumentNullException(nameof(Utils.ID));

        //        var result = _unitOfWork.LeadRatingRepository.Find(Utils.ID);
        //        return await Task.FromResult<object>(result).ConfigureAwait(false);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
        //    }
        //}

        //[HttpPost]
        //[Route("GetAllLeadRatingByOrgID")]
        //public async Task<object> GetAllLeadRatingByOrgID([FromBody] UtilsOrgID Utils, CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        if (cancellationToken != null)
        //            cancellationToken.ThrowIfCancellationRequested();

        //        var result = _unitOfWork.LeadRatingRepository.LeadRatingByOrgID(Utils.OrgID);
        //        return await Task.FromResult<object>(result).ConfigureAwait(false);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
        //    }
        //}

        //#endregion LeadRating

        //#region LeadContractRole

        //[HttpPost]
        //[Route("AddLeadContractRole")]
        //public async Task<object> AddLeadContractRole([FromBody] LeadContractRoleViewModel LeadContractRoleViewModel, CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        if (cancellationToken != null)
        //            cancellationToken.ThrowIfCancellationRequested();

        //        if (LeadContractRoleViewModel == null)
        //            throw new ArgumentNullException(nameof(LeadContractRoleViewModel));

        //        var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<LeadContractRoleViewModel, LeadContractRole>());
        //        var mapper = config.CreateMapper();
        //        var modal = mapper.Map<LeadContractRole>(LeadContractRoleViewModel);

        //        modal.id = Guid.NewGuid().ToString();
        //        modal.created_date = _dateTime.ToString();
        //        modal.is_deleted = false;

        //        _unitOfWork.LeadContractRoleRepository.Add(modal);

        //        _unitOfWork.Commit();

        //        return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Lead Status saved successfully." }).ConfigureAwait(false);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
        //    }
        //}

        //[HttpPatch]
        //[Route("UpdateLeadContractRole")]
        //public async Task<object> UpdateLeadContractRole([FromBody] LeadContractRoleViewModel LeadContractRoleViewModel, CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        if (cancellationToken != null)
        //            cancellationToken.ThrowIfCancellationRequested();

        //        if (LeadContractRoleViewModel == null)
        //            throw new ArgumentNullException(nameof(LeadContractRoleViewModel));

        //        var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<LeadContractRoleViewModel, LeadContractRole>());
        //        var mapper = config.CreateMapper();
        //        var modal = mapper.Map<LeadContractRole>(LeadContractRoleViewModel);
        //        modal.modified_date = _dateTime.ToString();

        //        _unitOfWork.LeadContractRoleRepository.Update(modal);
        //        _unitOfWork.Commit();

        //        return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Lead Status updated successfully." }).ConfigureAwait(false);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
        //    }
        //}

        //[HttpPost]
        //[Route("RemoveLeadContractRole")]
        //public async Task<object> RemoveLeadContractRole([FromBody] Utils Utils, CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        if (cancellationToken != null)
        //            cancellationToken.ThrowIfCancellationRequested();

        //        if (Utils == null)
        //            throw new ArgumentNullException(nameof(Utils.ID));

        //        _unitOfWork.LeadContractRoleRepository.Remove(Utils.ID);
        //        _unitOfWork.Commit();

        //        return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Lead Status ID removed successfully." }).ConfigureAwait(false);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
        //    }
        //}

        //[HttpGet]
        //[Route("GetAllLeadContractRole")]
        //public async Task<object> GetAllLeadContractRole(CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        if (cancellationToken != null)
        //            cancellationToken.ThrowIfCancellationRequested();

        //        var result = _unitOfWork.LeadContractRoleRepository.All();
        //        return await Task.FromResult<object>(result).ConfigureAwait(false);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
        //    }
        //}

        //[HttpPost]
        //[Route("FindByLeadContractRoleByID")]
        //public async Task<object> FindByLeadContractRoleByID([FromBody] Utils Utils, CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        LeadContractRoleViewModel customerViewModel = new LeadContractRoleViewModel();

        //        if (cancellationToken != null)
        //            cancellationToken.ThrowIfCancellationRequested();

        //        if (Utils == null)
        //            throw new ArgumentNullException(nameof(Utils.ID));

        //        var result = _unitOfWork.LeadContractRoleRepository.Find(Utils.ID);
        //        return await Task.FromResult<object>(result).ConfigureAwait(false);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
        //    }
        //}

        //[HttpPost]
        //[Route("GetAllLeadContractRoleByOrgID")]
        //public async Task<object> GetAllLeadContractRoleByOrgID([FromBody] UtilsOrgID Utils, CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        if (cancellationToken != null)
        //            cancellationToken.ThrowIfCancellationRequested();

        //        var result = _unitOfWork.LeadContractRoleRepository.GetLeadContractRoleByOrgID(Utils.OrgID);
        //        return await Task.FromResult<object>(result).ConfigureAwait(false);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
        //    }
        //}

        //#endregion LeadContractRole

        //#region LeadCompany

        //[HttpPost]
        //[Route("AddLeadCompany")]
        //public async Task<object> AddLeadCompany([FromBody] LeadCompanyViewModel LeadCompanyViewModel, CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        if (cancellationToken != null)
        //            cancellationToken.ThrowIfCancellationRequested();

        //        if (LeadCompanyViewModel == null)
        //            throw new ArgumentNullException(nameof(LeadCompanyViewModel));

        //        var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<LeadCompanyViewModel, LeadCompany>());
        //        var mapper = config.CreateMapper();
        //        var modal = mapper.Map<LeadCompany>(LeadCompanyViewModel);

        //        modal.id = Guid.NewGuid().ToString();
        //        modal.created_date = _dateTime.ToString();
        //        modal.is_deleted = false;

        //        _unitOfWork.LeadCompanyRepository.Add(modal);

        //        _unitOfWork.Commit();

        //        return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Lead Company saved successfully." }).ConfigureAwait(false);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
        //    }
        //}

        //[HttpPatch]
        //[Route("UpdateLeadCompany")]
        //public async Task<object> UpdateLeadCompany([FromBody] LeadCompanyViewModel LeadCompanyViewModel, CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        if (cancellationToken != null)
        //            cancellationToken.ThrowIfCancellationRequested();

        //        if (LeadCompanyViewModel == null)
        //            throw new ArgumentNullException(nameof(LeadCompanyViewModel));

        //        var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<LeadCompanyViewModel, LeadCompany>());
        //        var mapper = config.CreateMapper();
        //        var modal = mapper.Map<LeadCompany>(LeadCompanyViewModel);
        //        modal.modified_date = _dateTime.ToString();

        //        _unitOfWork.LeadCompanyRepository.Update(modal);
        //        _unitOfWork.Commit();

        //        return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Lead Company updated successfully." }).ConfigureAwait(false);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
        //    }
        //}

        //[HttpPost]
        //[Route("RemoveLeadCompany")]
        //public async Task<object> RemoveLeadCompany([FromBody] Utils Utils, CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        if (cancellationToken != null)
        //            cancellationToken.ThrowIfCancellationRequested();

        //        if (Utils == null)
        //            throw new ArgumentNullException(nameof(Utils.ID));

        //        _unitOfWork.LeadCompanyRepository.Remove(Utils.ID);
        //        _unitOfWork.Commit();

        //        return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Lead Company ID removed successfully." }).ConfigureAwait(false);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
        //    }
        //}

        //[HttpGet]
        //[Route("GetAllLeadCompany")]
        //public async Task<object> GetAllLeadCompany(CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        if (cancellationToken != null)
        //            cancellationToken.ThrowIfCancellationRequested();

        //        var result = _unitOfWork.LeadCompanyRepository.All();
        //        return await Task.FromResult<object>(result).ConfigureAwait(false);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
        //    }
        //}

        //[HttpPost]
        //[Route("FindByLeadCompanyByID")]
        //public async Task<object> FindByLeadCompanyByID([FromBody] Utils Utils, CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        LeadCompanyViewModel customerViewModel = new LeadCompanyViewModel();

        //        if (cancellationToken != null)
        //            cancellationToken.ThrowIfCancellationRequested();

        //        if (Utils == null)
        //            throw new ArgumentNullException(nameof(Utils.ID));

        //        var result = _unitOfWork.LeadCompanyRepository.Find(Utils.ID);
        //        return await Task.FromResult<object>(result).ConfigureAwait(false);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
        //    }
        //}

        //[HttpPost]
        //[Route("GetAllLeadCompanyByOrgID")]
        //public async Task<object> GetAllLeadCompanyByOrgID([FromBody] UtilsOrgID Utils, CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        if (cancellationToken != null)
        //            cancellationToken.ThrowIfCancellationRequested();

        //        var result = _unitOfWork.LeadCompanyRepository.LeadCompanyByOrgID(Utils.OrgID);
        //        return await Task.FromResult<object>(result).ConfigureAwait(false);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
        //    }
        //}

        //#endregion LeadCompany

        #endregion Rating_ContractNotInUse
    }
}