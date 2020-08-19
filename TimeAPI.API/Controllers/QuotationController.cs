using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;
using TimeAPI.API.Models;
using TimeAPI.API.Models.ExclusionViewModels;
using TimeAPI.API.Models.LeadStageViewModels;
using TimeAPI.API.Models.PaymentModeViewModels;
using TimeAPI.API.Models.PaymentViewModels;
using TimeAPI.API.Models.PrefixViewModels;
using TimeAPI.API.Models.QuotationViewModels;
using TimeAPI.API.Models.WarrantyViewModels;
using TimeAPI.API.Services;
using TimeAPI.Domain;
using TimeAPI.Domain.Entities;

namespace TimeAPI.API.Controllers
{
    //[ApiKeyAuth]
    //[EnableCors("CorsPolicy")]
    [Route("[controller]")]
    //[Authorize(Quotations = "superadmin")]
    public class QuotationController : Controller
    {
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly ApplicationSettings _appSettings;
        private readonly IUnitOfWork _unitOfWork;
        private readonly DateTime _dateTime;

        public QuotationController(IUnitOfWork unitOfWork, ILogger<QuotationController> logger,
            IEmailSender emailSender,
            IOptions<ApplicationSettings> AppSettings)
        {
            _emailSender = emailSender;
            _logger = logger;
            _appSettings = AppSettings.Value;
            _unitOfWork = unitOfWork;
            _dateTime = InternetTime.GetCurrentTimeFromTimeZone().Value.DateTime;
        }

        #region Quotation

        [HttpPost]
        [Route("AddQuotation")]
        public async Task<object> AddQuotation([FromBody] QuotationViewModel QuotationViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (QuotationViewModel == null)
                    throw new ArgumentNullException(nameof(QuotationViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<QuotationViewModel, Quotation>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Quotation>(QuotationViewModel);

                string qout_id = modal.id = Guid.NewGuid().ToString();
                modal.created_date = _dateTime.ToString();
                modal.is_deleted = false;

                _unitOfWork.QuotationRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = qout_id, Desc = "Quotation registered successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdateQuotation")]
        public async Task<object> UpdateQuotation([FromBody]  QuotationViewModel QuotationViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (QuotationViewModel == null)
                    throw new ArgumentNullException(nameof(QuotationViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<QuotationViewModel, Quotation>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Quotation>(QuotationViewModel);

                modal.modified_date = _dateTime.ToString();

                _unitOfWork.QuotationRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Quotation updated successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemoveQuotation")]
        public async Task<object> RemoveQuotation([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                _unitOfWork.QuotationRepository.Remove(Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Quotation removed successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetAllQuotation")]
        public async Task<object> GetAllQuotation(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.QuotationRepository.All();
                _unitOfWork.Commit();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindByQuotationID")]
        public async Task<object> FindByQuotationID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = _unitOfWork.QuotationRepository.Find(Utils.ID);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }


        [HttpPost]
        [Route("GetAllQuotationByOrgID")]
        public async Task<object> GetAllQuotationByOrgID([FromBody] UtilsOrgID UtilsOrgID, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (UtilsOrgID == null)
                    throw new ArgumentNullException(nameof(UtilsOrgID.OrgID));

                var results = _unitOfWork.QuotationRepository.QuotationByOrgID(UtilsOrgID.OrgID);

                return await System.Threading.Tasks.Task.FromResult<object>(JsonConvert.SerializeObject(results, Formatting.Indented)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return System.Threading.Tasks.Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        #endregion Quotation

        #region PaymentMode

        [HttpPost]
        [Route("AddPaymentMode")]
        public async Task<object> AddPaymentMode([FromBody] PaymentModeViewModel PaymentModeViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (PaymentModeViewModel == null)
                    throw new ArgumentNullException(nameof(PaymentModeViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<PaymentModeViewModel, PaymentMode>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<PaymentMode>(PaymentModeViewModel);

                modal.id = Guid.NewGuid().ToString();
                modal.created_date = _dateTime.ToString();
                modal.is_deleted = false;

                _unitOfWork.PaymentModeRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "PaymentMode registered successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdatePaymentMode")]
        public async Task<object> UpdatePaymentMode([FromBody]  PaymentModeViewModel PaymentModeViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (PaymentModeViewModel == null)
                    throw new ArgumentNullException(nameof(PaymentModeViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<PaymentModeViewModel, PaymentMode>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<PaymentMode>(PaymentModeViewModel);

                modal.modified_date = _dateTime.ToString();

                _unitOfWork.PaymentModeRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "PaymentMode updated successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemovePaymentMode")]
        public async Task<object> RemovePaymentMode([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                _unitOfWork.PaymentModeRepository.Remove(Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "PaymentMode removed successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetAllPaymentMode")]
        public async Task<object> GetAllPaymentMode(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.PaymentModeRepository.All();
                _unitOfWork.Commit();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindByPaymentModeID")]
        public async Task<object> FindByPaymentModeID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = _unitOfWork.PaymentModeRepository.Find(Utils.ID);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }


        [HttpPost]
        [Route("GetAllPaymentModeByOrgID")]
        public async Task<object> GetAllPaymentModeByOrgID([FromBody] UtilsOrgID UtilsOrgID, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (UtilsOrgID == null)
                    throw new ArgumentNullException(nameof(UtilsOrgID.OrgID));

                var results = _unitOfWork.PaymentModeRepository.PaymentModeByOrgID(UtilsOrgID.OrgID);
                return await System.Threading.Tasks.Task.FromResult<object>(JsonConvert.SerializeObject(results, Formatting.Indented)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return System.Threading.Tasks.Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        #endregion PaymentMode

        #region Payment

        [HttpPost]
        [Route("AddPayment")]
        public async Task<object> AddPayment([FromBody] PaymentViewModel PaymentViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (PaymentViewModel == null)
                    throw new ArgumentNullException(nameof(PaymentViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<PaymentViewModel, Payment>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Payment>(PaymentViewModel);

                modal.id = Guid.NewGuid().ToString();
                modal.created_date = _dateTime.ToString();
                modal.is_deleted = false;

                _unitOfWork.PaymentRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Payment registered successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdatePayment")]
        public async Task<object> UpdatePayment([FromBody]  PaymentViewModel PaymentViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (PaymentViewModel == null)
                    throw new ArgumentNullException(nameof(PaymentViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<PaymentViewModel, Payment>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Payment>(PaymentViewModel);

                modal.modified_date = _dateTime.ToString();

                _unitOfWork.PaymentRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Payment updated successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemovePayment")]
        public async Task<object> RemovePayment([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                _unitOfWork.PaymentRepository.Remove(Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Payment removed successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetAllPayment")]
        public async Task<object> GetAllPayment(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.PaymentRepository.All();
                _unitOfWork.Commit();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindByPaymentID")]
        public async Task<object> FindByPaymentID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = _unitOfWork.PaymentRepository.Find(Utils.ID);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }


        [HttpPost]
        [Route("GetAllPaymentByOrgID")]
        public async Task<object> GetAllPaymentByOrgID([FromBody] UtilsOrgID UtilsOrgID, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (UtilsOrgID == null)
                    throw new ArgumentNullException(nameof(UtilsOrgID.OrgID));

                var results = _unitOfWork.PaymentRepository.PaymentByOrgID(UtilsOrgID.OrgID);
                return await System.Threading.Tasks.Task.FromResult<object>(JsonConvert.SerializeObject(results, Formatting.Indented)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return System.Threading.Tasks.Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        #endregion Payment

        #region Warranty

        [HttpPost]
        [Route("AddWarranty")]
        public async Task<object> AddWarranty([FromBody] WarrantyViewModel WarrantyViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (WarrantyViewModel == null)
                    throw new ArgumentNullException(nameof(WarrantyViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<WarrantyViewModel, Warranty>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Warranty>(WarrantyViewModel);

                modal.id = Guid.NewGuid().ToString();
                modal.created_date = _dateTime.ToString();
                modal.is_deleted = false;

                _unitOfWork.WarrantyRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Warranty registered successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdateWarranty")]
        public async Task<object> UpdateWarranty([FromBody]  WarrantyViewModel WarrantyViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (WarrantyViewModel == null)
                    throw new ArgumentNullException(nameof(WarrantyViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<WarrantyViewModel, Warranty>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Warranty>(WarrantyViewModel);

                modal.modified_date = _dateTime.ToString();

                _unitOfWork.WarrantyRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Warranty updated successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemoveWarranty")]
        public async Task<object> RemoveWarranty([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                _unitOfWork.WarrantyRepository.Remove(Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Warranty removed successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetAllWarranty")]
        public async Task<object> GetAllWarranty(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.WarrantyRepository.All();
                _unitOfWork.Commit();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindByWarrantyID")]
        public async Task<object> FindByWarrantyID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = _unitOfWork.WarrantyRepository.Find(Utils.ID);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }


        [HttpPost]
        [Route("GetAllWarrantyByOrgID")]
        public async Task<object> GetAllWarrantyByOrgID([FromBody] UtilsOrgID UtilsOrgID, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (UtilsOrgID == null)
                    throw new ArgumentNullException(nameof(UtilsOrgID.OrgID));

                var results = _unitOfWork.WarrantyRepository.WarrantyByOrgID(UtilsOrgID.OrgID);
                return await System.Threading.Tasks.Task.FromResult<object>(JsonConvert.SerializeObject(results, Formatting.Indented)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return System.Threading.Tasks.Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        #endregion Warranty

        #region Exclusion

        [HttpPost]
        [Route("AddExclusion")]
        public async Task<object> AddExclusion([FromBody] ExclusionViewModel ExclusionViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (ExclusionViewModel == null)
                    throw new ArgumentNullException(nameof(ExclusionViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<ExclusionViewModel, Exclusion>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Exclusion>(ExclusionViewModel);

                modal.id = Guid.NewGuid().ToString();
                modal.created_date = _dateTime.ToString();
                modal.is_deleted = false;

                _unitOfWork.ExclusionRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Exclusion registered successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdateExclusion")]
        public async Task<object> UpdateExclusion([FromBody]  ExclusionViewModel ExclusionViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (ExclusionViewModel == null)
                    throw new ArgumentNullException(nameof(ExclusionViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<ExclusionViewModel, Exclusion>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Exclusion>(ExclusionViewModel);

                modal.modified_date = _dateTime.ToString();

                _unitOfWork.ExclusionRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Exclusion updated successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemoveExclusion")]
        public async Task<object> RemoveExclusion([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                _unitOfWork.ExclusionRepository.Remove(Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Exclusion removed successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetAllExclusion")]
        public async Task<object> GetAllExclusion(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.ExclusionRepository.All();
                _unitOfWork.Commit();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindByExclusionID")]
        public async Task<object> FindByExclusionID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = _unitOfWork.ExclusionRepository.Find(Utils.ID);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }


        [HttpPost]
        [Route("GetAllExclusionByOrgID")]
        public async Task<object> GetAllExclusionByOrgID([FromBody] UtilsOrgID UtilsOrgID, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (UtilsOrgID == null)
                    throw new ArgumentNullException(nameof(UtilsOrgID.OrgID));

                var results = _unitOfWork.ExclusionRepository.ExclusionByOrgID(UtilsOrgID.OrgID);
                return await System.Threading.Tasks.Task.FromResult<object>(JsonConvert.SerializeObject(results, Formatting.Indented)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return System.Threading.Tasks.Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        #endregion Exclusion

        #region Stage

        [HttpPost]
        [Route("AddStage")]
        public async Task<object> AddStage([FromBody] LeadStageViewModel LeadStageViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (LeadStageViewModel == null)
                    throw new ArgumentNullException(nameof(LeadStageViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<LeadStageViewModel, LeadStage>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<LeadStage>(LeadStageViewModel);

                modal.id = Guid.NewGuid().ToString();
                modal.created_date = _dateTime.ToString();
                modal.is_deleted = false;

                _unitOfWork.LeadStageRepository.Add(modal);

                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Lead Status saved successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdateStage")]
        public async Task<object> UpdateStage([FromBody] LeadStageViewModel LeadStageViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (LeadStageViewModel == null)
                    throw new ArgumentNullException(nameof(LeadStageViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<LeadStageViewModel, LeadStage>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<LeadStage>(LeadStageViewModel);
                modal.modified_date = _dateTime.ToString();

                _unitOfWork.LeadStageRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Lead Status updated successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemoveStage")]
        public async Task<object> RemoveStage([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                _unitOfWork.LeadStageRepository.Remove(Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Lead Status ID removed successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetAllStage")]
        public async Task<object> GetAllStage(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.LeadStageRepository.All();
                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindByStageByID")]
        public async Task<object> FindByStageByID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                LeadStageViewModel customerViewModel = new LeadStageViewModel();

                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = _unitOfWork.LeadStageRepository.Find(Utils.ID);
                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("GetAllStageByOrgID")]
        public async Task<object> GetAllStageByOrgID([FromBody] UtilsOrgID Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.LeadStageRepository.GetLeadStageByOrgID(Utils.OrgID);
                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        #endregion Stage
    }
}