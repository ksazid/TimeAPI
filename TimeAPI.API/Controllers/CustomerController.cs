using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using TimeAPI.API.Models;
using TimeAPI.API.Models.CustomerViewModels;
using TimeAPI.API.Models.SocialViewModels;
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

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<CustomerViewModel, Customer>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Customer>(CustomerViewModel);

                modal.id = Guid.NewGuid().ToString();
                modal.created_date = _dateTime.ToString();
                modal.is_deleted = false;


                var config1 = new AutoMapper.MapperConfiguration(m => m.CreateMap<EntityContact, EntityContact>());
                var mapper1 = config.CreateMapper();
                var modal1 = mapper.Map<EntityContact>(CustomerViewModel.EntityContact);

                modal1.entity_id = modal.id;

                _unitOfWork.EntityContactRepository.Add(modal1);
                _unitOfWork.CustomerRepository.Add(modal);

                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Customer saved successfully." }).ConfigureAwait(false);
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


                _unitOfWork.CustomerRepository.Update(modal);


                _unitOfWork.CustomerRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Customer updated succefully." }).ConfigureAwait(false);
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

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Customer ID removed succefully." }).ConfigureAwait(false);
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

                var result = _unitOfWork.CustomerRepository.All();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        public async Task<object> FindByCustomerId([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                CustomerViewModel customerViewModel = new CustomerViewModel();

                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = _unitOfWork.CustomerRepository.Find(Utils.ID);
                var resultContact = _unitOfWork.EntityContactRepository.FindByEntityID(result.id);

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<CustomerViewModel, Customer>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Customer>(result);

                modal.EntityContact = resultContact;

                return await Task.FromResult<object>(modal).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }
    }
}