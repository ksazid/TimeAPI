using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using TimeAPI.API.Models;
using TimeAPI.API.Models.CustomerViewModels;
using TimeAPI.API.Services;
using TimeAPI.Domain;
using TimeAPI.Domain.Entities;

namespace TimeAPI.API.Controllroers
{
    //[ApiKeyAuth]
    //[EnableCors("CorsPolicy")]
    [Route("[controller]")]
    //[Authorize(Roles = "superadmin")]
    public class CustomerController : Controller
    {
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly ApplicationSettings _appSettings;
        private readonly IUnitOfWork _unitOfWork;
        private readonly DateTime _dateTime;

        public CustomerController(IUnitOfWork unitOfWork, ILogger<CustomerController> logger,
            IEmailSender emailSender,
            IOptions<ApplicationSettings> AppSettings)
        {
            _emailSender = emailSender;
            _logger = logger;
            _appSettings = AppSettings.Value;
            _unitOfWork = unitOfWork;
            _dateTime = InternetTime.GetCurrentTimeFromTimeZone().Value.DateTime;
        }

        [HttpPost]
        [Route("AddCustomer")]
        public async Task<object> AddCustomer([FromBody] CustomerViewModel CustomerViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (CustomerViewModel == null)
                    throw new ArgumentNullException(nameof(CustomerViewModel));

                string cst_id = string.Empty;
                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<CustomerViewModel, Customer>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Customer>(CustomerViewModel);

                modal.id = Guid.NewGuid().ToString();
                cst_id = modal.id;
                modal.created_date = _dateTime.ToString();
                modal.is_deleted = false;

                var config1 = new AutoMapper.MapperConfiguration(m => m.CreateMap<EntityContact, EntityContact>());
                var mapper1 = config.CreateMapper();

                if (CustomerViewModel.EntityContact != null)
                {
                    var modal1 = mapper.Map<EntityContact>(CustomerViewModel.EntityContact);
                    modal1.id = Guid.NewGuid().ToString();
                    modal1.entity_id = modal.id;

                    _unitOfWork.EntityContactRepository.Add(modal1);
                }
                _unitOfWork.CustomerRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = cst_id, Desc = "Customer Saved Successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdateCustomer")]
        public async Task<object> UpdateCustomer([FromBody] CustomerViewModel CustomerViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (CustomerViewModel == null)
                    throw new ArgumentNullException(nameof(CustomerViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<CustomerViewModel, Customer>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Customer>(CustomerViewModel);
                modal.modified_date = _dateTime.ToString();

                var config1 = new AutoMapper.MapperConfiguration(m => m.CreateMap<EntityContact, EntityContact>());
                var mapper1 = config.CreateMapper();
                var modal1 = mapper.Map<EntityContact>(CustomerViewModel.EntityContact);
                modal1.modified_date = _dateTime.ToString();
                modal1.entity_id = modal.id;

                await _unitOfWork.EntityContactRepository.UpdateByEntityID(modal1).ConfigureAwait(false);
                _unitOfWork.CustomerRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Customer updated successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemoveCustomer")]
        public async Task<object> RemoveCustomer([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                _unitOfWork.CustomerRepository.Remove(Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Customer ID removed successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetAllCustomer")]
        public async Task<object> GetAllCustomer(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = await _unitOfWork.CustomerRepository.All().ConfigureAwait(false);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindByCustomerId")]
        public async Task<object> FindByCustomerId([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                CustomerViewModel customerViewModel = new CustomerViewModel();

                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = await _unitOfWork.CustomerRepository.Find(Utils.ID).ConfigureAwait(false);

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<CustomerViewModel, Customer>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Customer>(result);

                if (result != null)
                {
                    var resultContact = await _unitOfWork.EntityContactRepository.FindByEntityID(result.id).ConfigureAwait(false);
                    modal.EntityContact = resultContact;
                }

                return await Task.FromResult<object>(modal).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("GetAllCustomerByOrgID")]
        public async Task<object> GetAllCustomerByOrgID([FromBody] UtilsOrgID Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = await _unitOfWork.CustomerRepository.FindCustomerByOrgID(Utils.OrgID).ConfigureAwait(false);
                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindByCustomerByNameAndEmail")]
        public async Task<object> FindByCustomerByNameAndEmail([FromBody] UtilsCustomerNameAndEmail Utils, CancellationToken cancellationToken)
        {
            try
            {
                CustomerViewModel customerViewModel = new CustomerViewModel();

                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.CustomerName));

                var result = await _unitOfWork.CustomerRepository.FindByCustomerByNameAndEmail(Utils.CustomerName, Utils.Email).ConfigureAwait(false);
                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<CustomerViewModel, Customer>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Customer>(result);

                if (result != null)
                {
                    var resultContact = await _unitOfWork.EntityContactRepository.FindByEntityID(result.id).ConfigureAwait(false);
                    modal.EntityContact = resultContact;
                }
                if (modal != null)
                {
                    return await Task.FromResult<object>(modal).ConfigureAwait(false);
                }
                else
                {
                    return Ok(new Customer { id = "" });
                }

            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }
    }
}