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
using TimeAPI.API.Models.OrganizationViewModels;
using TimeAPI.API.Models.OrganizationViewModel;
using System.Threading;
using TimeAPI.Domain.Entities;
using System.Globalization;
using TimeAPI.API.Models.PlanViewModels;
using TimeAPI.API.Models.PlanFeatureViewModels;
using TimeAPI.API.Models.PlanPriceViewModels;

namespace TimeAPI.API.Controllers
{
    //[ApiKeyAuth]
    //[EnableCors("CorsPolicy")]
    [Route("[controller]")]
    //[Authorize(Roles = "superadmin")]
    public class AdminController : Controller
    {
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly ApplicationSettings _appSettings;
        private readonly IUnitOfWork _unitOfWork;
        public AdminController(IUnitOfWork unitOfWork, ILogger<AdminController> logger,
            IEmailSender emailSender, IOptions<ApplicationSettings> AppSettings)
        {
            _emailSender = emailSender;
            _logger = logger;
            _appSettings = AppSettings.Value;
            _unitOfWork = unitOfWork;
        }


        #region Plan

        [HttpPost]
        [Route("AddPlan")]
        public async Task<object> AddPlan([FromBody] PlanViewModel subscriptionViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (subscriptionViewModel == null)
                    throw new ArgumentNullException(nameof(subscriptionViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<PlanViewModel, Plan>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Plan>(subscriptionViewModel);

                modal.id = Guid.NewGuid().ToString();
                modal.created_date = DateTime.Now.ToString(CultureInfo.CurrentCulture);
                modal.is_deleted = false;

                _unitOfWork.PlanRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Plan registered succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdatePlan")]
        public async Task<object> UpdatePlan([FromBody] PlanViewModel subscriptionViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (subscriptionViewModel == null)
                    throw new ArgumentNullException(nameof(subscriptionViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<PlanViewModel, Plan>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Plan>(subscriptionViewModel);
                modal.modified_date = DateTime.Now.ToString(CultureInfo.CurrentCulture);

                _unitOfWork.PlanRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Plan updated succefully." }).ConfigureAwait(false);
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

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Employee removed succefully." }).ConfigureAwait(false);
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


        #endregion Plan

        #region PlanFeature

        [HttpPost]
        [Route("AddPlanFeature")]
        public async Task<object> AddPlanFeature([FromBody] PlanFeatureViewModel subscriptionViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (subscriptionViewModel == null)
                    throw new ArgumentNullException(nameof(subscriptionViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<PlanFeatureViewModel, PlanFeature>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<PlanFeature>(subscriptionViewModel);

                modal.id = Guid.NewGuid().ToString();
                modal.created_date = DateTime.Now.ToString(CultureInfo.CurrentCulture);
                modal.is_deleted = false;

                _unitOfWork.PlanFeatureRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Plan Features registered succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdatePlanFeature")]
        public async Task<object> UpdatePlanFeature([FromBody] PlanFeatureViewModel subscriptionViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (subscriptionViewModel == null)
                    throw new ArgumentNullException(nameof(subscriptionViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<PlanFeatureViewModel, PlanFeature>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<PlanFeature>(subscriptionViewModel);
                modal.modified_date = DateTime.Now.ToString(CultureInfo.CurrentCulture);

                _unitOfWork.PlanFeatureRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Plan Feature updated succefully." }).ConfigureAwait(false);
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

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Plan Feature removed succefully." }).ConfigureAwait(false);
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


        #endregion PlanFeature

        #region PlanPrice

        [HttpPost]
        [Route("AddPlanPrice")]
        public async Task<object> AddPlanPrice([FromBody] PlanPriceViewModel subscriptionViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (subscriptionViewModel == null)
                    throw new ArgumentNullException(nameof(subscriptionViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<PlanPriceViewModel, PlanPrice>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<PlanPrice>(subscriptionViewModel);

                modal.id = Guid.NewGuid().ToString();
                modal.created_date = DateTime.Now.ToString(CultureInfo.CurrentCulture);
                modal.is_deleted = false;

                _unitOfWork.PlanPriceRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Plan Price Added succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdatePlanPrice")]
        public async Task<object> UpdatePlanPrice([FromBody] PlanPriceViewModel subscriptionViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (subscriptionViewModel == null)
                    throw new ArgumentNullException(nameof(subscriptionViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<PlanPriceViewModel, PlanPrice>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<PlanPrice>(subscriptionViewModel);
                modal.modified_date = DateTime.Now.ToString(CultureInfo.CurrentCulture);

                _unitOfWork.PlanPriceRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Plan Price updated succefully." }).ConfigureAwait(false);
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

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Plan Price removed succefully." }).ConfigureAwait(false);
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


        #endregion PlanPrice

        #region subscription


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
                modal.created_date = DateTime.Now.ToString(CultureInfo.CurrentCulture);
                modal.is_deleted = false;

                _unitOfWork.SubscriptionRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Subscription registered succefully." }).ConfigureAwait(false);
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
                modal.modified_date = DateTime.Now.ToString(CultureInfo.CurrentCulture);

                _unitOfWork.SubscriptionRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Subscription updated succefully." }).ConfigureAwait(false);
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

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Employee removed succefully." }).ConfigureAwait(false);
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


        //[HttpPost]
        //[Route("FindByApiKeyOrgID")]
        //public async Task<object> FindByApiKeyOrgID([FromBody] Utils Utils, CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        if (cancellationToken != null)
        //            cancellationToken.ThrowIfCancellationRequested();

        //        if (Utils == null)
        //            throw new ArgumentNullException(nameof(Utils.ID));

        //        var result = _unitOfWork.SubscriptionRepository.FindByApiKeyOrgID(Utils.ID);

        //        return await Task.FromResult<object>(result).ConfigureAwait(false);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
        //    }
        //}

        #endregion subscription
    }
}
