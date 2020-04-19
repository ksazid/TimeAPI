using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;
using TimeAPI.API.Filters;
using TimeAPI.API.Models;
using TimeAPI.API.Models.BillingViewModels;
using TimeAPI.API.Models.OrganizationViewModel;
using TimeAPI.API.Models.PlanFeatureViewModels;
using TimeAPI.API.Models.PlanPriceViewModels;
using TimeAPI.API.Models.PlanViewModels;
using TimeAPI.API.Services;
using TimeAPI.Domain;
using TimeAPI.Domain.Entities;

namespace TimeAPI.API.Controllers
{
    [Authorize]
    [ApiKeyAuth]
    //[EnableCors("CorsPolicy")]
    [Route("[controller]")]
    public class AdminController : Controller
    {
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly ApplicationSettings _appSettings;
        private readonly IUnitOfWork _unitOfWork;
        private static DateTime _dateTime;

        public AdminController(IUnitOfWork unitOfWork, ILogger<AdminController> logger,
            IEmailSender emailSender, IOptions<ApplicationSettings> AppSettings)
        {
            _emailSender = emailSender;
            _logger = logger;
            _appSettings = AppSettings.Value;
            _unitOfWork = unitOfWork;
            _dateTime = InternetTime.GetCurrentTimeFromTimeZone().Value.DateTime;
        }

        #region Plan

        [HttpPost]
        [Route("AddPlan")]
        public async Task<object> AddPlan([FromBody] PlanViewModel planViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (planViewModel == null)
                    throw new ArgumentNullException(nameof(planViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<PlanViewModel, Plan>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Plan>(planViewModel);

                modal.id = Guid.NewGuid().ToString();
                modal.created_date = _dateTime.ToString();
                modal.is_deleted = false;

                _unitOfWork.PlanRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Plan registered successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdatePlan")]
        public async Task<object> UpdatePlan([FromBody] PlanViewModel planViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (planViewModel == null)
                    throw new ArgumentNullException(nameof(planViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<PlanViewModel, Plan>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Plan>(planViewModel);
                modal.modified_date = _dateTime.ToString();

                _unitOfWork.PlanRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Plan updated successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemovePlan")]
        public async Task<object> RemovePlan([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                _unitOfWork.PlanRepository.Remove(Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Employee removed successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetAllPlan")]
        public async Task<object> GetAllPlan(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.PlanRepository.All();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindByPlanID")]
        public async Task<object> FindByPlanID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = _unitOfWork.PlanRepository.Find(Utils.ID);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindPlanPriceByPlanID")]
        public async Task<object> FindPlanPriceByPlanID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = _unitOfWork.PlanRepository.FindPlanPriceByPlanID(Utils.ID);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        #endregion Plan

        #region PlanFeature

        [HttpPost]
        [Route("AddPlanFeature")]
        public async Task<object> AddPlanFeature([FromBody] PlanFeatureViewModel featureViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (featureViewModel == null)
                    throw new ArgumentNullException(nameof(featureViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<PlanFeatureViewModel, PlanFeature>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<PlanFeature>(featureViewModel);

                modal.id = Guid.NewGuid().ToString();
                modal.created_date = _dateTime.ToString();
                modal.is_deleted = false;

                _unitOfWork.PlanFeatureRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Plan Features registered successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdatePlanFeature")]
        public async Task<object> UpdatePlanFeature([FromBody] PlanFeatureViewModel featureViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (featureViewModel == null)
                    throw new ArgumentNullException(nameof(featureViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<PlanFeatureViewModel, PlanFeature>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<PlanFeature>(featureViewModel);
                modal.modified_date = _dateTime.ToString();

                _unitOfWork.PlanFeatureRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Plan Feature updated successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemovePlanFeature")]
        public async Task<object> RemovePlanFeature([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                _unitOfWork.PlanFeatureRepository.Remove(Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Plan Feature removed successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetAllPlanFeature")]
        public async Task<object> GetAllPlanFeature(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.PlanFeatureRepository.All();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindByPlanFeatureID")]
        public async Task<object> FindByPlanFeatureID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = _unitOfWork.PlanFeatureRepository.Find(Utils.ID);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpGet]
        [Route("FetchAllPlanFeatures")]
        public async Task<object> FetchAllPlanFeatures(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                oDataTable _oDataTable = new oDataTable();
                var results = _unitOfWork.PlanFeatureRepository.GetAllPlanFeatures();
                var xResult = _oDataTable.ToDataTable(results);

                return await System.Threading.Tasks.Task.FromResult<object>(JsonConvert.SerializeObject(xResult, Formatting.Indented)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return System.Threading.Tasks.Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        #endregion PlanFeature

        #region PlanPrice

        [HttpPost]
        [Route("AddPlanPrice")]
        public async Task<object> AddPlanPrice([FromBody] PlanPriceViewModel priceViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (priceViewModel == null)
                    throw new ArgumentNullException(nameof(priceViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<PlanPriceViewModel, PlanPrice>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<PlanPrice>(priceViewModel);

                modal.id = Guid.NewGuid().ToString();
                modal.created_date = _dateTime.ToString();
                modal.is_deleted = false;

                _unitOfWork.PlanPriceRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Plan Price Added successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdatePlanPrice")]
        public async Task<object> UpdatePlanPrice([FromBody] PlanPriceViewModel priceViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (priceViewModel == null)
                    throw new ArgumentNullException(nameof(priceViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<PlanPriceViewModel, PlanPrice>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<PlanPrice>(priceViewModel);
                modal.modified_date = _dateTime.ToString();

                _unitOfWork.PlanPriceRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Plan Price updated successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemovePlanPrice")]
        public async Task<object> RemovePlanPrice([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                _unitOfWork.PlanPriceRepository.Remove(Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Plan Price removed successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetAllPlanPrice")]
        public async Task<object> GetAllPlanPrice(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.PlanPriceRepository.All();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindByPlanPriceID")]
        public async Task<object> FindByPlanPriceID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = _unitOfWork.PlanPriceRepository.Find(Utils.ID);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpGet]
        [Route("FetchAllPlanPrice")]
        public async Task<object> FetchAllPlanPrice(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                oDataTable _oDataTable = new oDataTable();
                var results = _unitOfWork.PlanPriceRepository.GetAllPlanPrice();
                var xResult = _oDataTable.ToDataTable(results);

                return await System.Threading.Tasks.Task.FromResult<object>(JsonConvert.SerializeObject(xResult, Formatting.Indented)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return System.Threading.Tasks.Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        #endregion PlanPrice

        #region Subscription

        [HttpPost]
        [Route("AddSubscription")]
        public async Task<object> AddSubscription([FromBody] SubscriptionViewModel subscriptionViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (subscriptionViewModel == null)
                    throw new ArgumentNullException(nameof(subscriptionViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<SubscriptionViewModel, Subscription>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Subscription>(subscriptionViewModel);

                modal.id = Guid.NewGuid().ToString();
                modal.created_date = _dateTime.ToString();
                modal.is_deleted = false;

                _unitOfWork.SubscriptionRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Subscription registered successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdateSubscription")]
        public async Task<object> UpdateSubscription([FromBody] SubscriptionViewModel subscriptionViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (subscriptionViewModel == null)
                    throw new ArgumentNullException(nameof(subscriptionViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<SubscriptionViewModel, Subscription>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Subscription>(subscriptionViewModel);
                modal.modified_date = _dateTime.ToString();

                _unitOfWork.SubscriptionRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Subscription updated successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemoveSubscription")]
        public async Task<object> RemoveSubscription([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                _unitOfWork.SubscriptionRepository.Remove(Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Employee removed successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetAllSubscription")]
        public async Task<object> GetAllSubscription(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.SubscriptionRepository.All();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindBySubscriptionID")]
        public async Task<object> FindBySubscriptionID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = _unitOfWork.SubscriptionRepository.Find(Utils.ID);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindByApiKeyByUserID")]
        public async Task<object> FindByApiKeyByUserID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = _unitOfWork.SubscriptionRepository.FindByApiKeyByUserID(Utils.ID);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        #endregion Subscription

        #region Billing

        [HttpPost]
        [Route("AddBilling")]
        public async Task<object> AddBilling([FromBody] BillingViewModel billingViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (billingViewModel == null)
                    throw new ArgumentNullException(nameof(billingViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<BillingViewModel, Billing>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Billing>(billingViewModel);

                modal.id = Guid.NewGuid().ToString();
                modal.created_date = _dateTime.ToString();
                modal.is_deleted = false;

                _unitOfWork.BillingRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Billing registered successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdateBilling")]
        public async Task<object> UpdateBilling([FromBody] BillingViewModel billingViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (billingViewModel == null)
                    throw new ArgumentNullException(nameof(billingViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<BillingViewModel, Billing>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Billing>(billingViewModel);
                modal.modified_date = _dateTime.ToString();

                _unitOfWork.BillingRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Billing updated successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemoveBilling")]
        public async Task<object> RemoveBilling([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                _unitOfWork.BillingRepository.Remove(Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Employee removed successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetAllBilling")]
        public async Task<object> GetAllBilling(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.BillingRepository.All();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        #endregion Billing

        //[HttpPost]
        //[Route("FindByApiKeyOrgID")]
        //public async Task<object> FindByApiKeyOrgID([FromBody] Utils Utils, CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        if (cancellationToken != null)
        //            cancellationToken.ThrowIfCancellationRequested();

        // if (Utils == null) throw new ArgumentNullException(nameof(Utils.ID));

        // var result = _unitOfWork.SubscriptionRepository.FindByApiKeyOrgID(Utils.ID);

        //        return await Task.FromResult<object>(result).ConfigureAwait(false);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
        //    }
        //}
    }
}