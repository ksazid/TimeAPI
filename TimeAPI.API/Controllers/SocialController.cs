using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using TimeAPI.API.Models;
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
    public class SocialController : Controller
    {
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly ApplicationSettings _appSettings;
        private readonly IUnitOfWork _unitOfWork;
        private readonly DateTime _dateTime;

        public SocialController(IUnitOfWork unitOfWork, ILogger<SocialController> logger,
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
        [Route("AddSocial")]
        public async Task<object> AddSocial([FromBody] SocialViewModel socialViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (socialViewModel == null)
                    throw new ArgumentNullException(nameof(socialViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<SocialViewModel, Social>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Social>(socialViewModel);

                modal.id = Guid.NewGuid().ToString();
                modal.created_date = _dateTime.ToString();
                modal.is_deleted = false;

                _unitOfWork.SocialRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Social IDs saved successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdateSocial")]
        public async Task<object> UpdateSocial([FromBody] SocialViewModel socialViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (socialViewModel == null)
                    throw new ArgumentNullException(nameof(socialViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<SocialViewModel, Social>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Social>(socialViewModel);
                modal.modified_date = _dateTime.ToString();

                _unitOfWork.SocialRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Social ID updated succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("RemoveSocial")]
        public async Task<object> RemoveSocial([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                _unitOfWork.SocialRepository.Remove(Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Social ID removed succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetAllSocial")]
        public async Task<object> GetAllSocial(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.SocialRepository.All();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindBySocailID")]
        public async Task<object> FindBySocailID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = _unitOfWork.SocialRepository.Find(Utils.ID);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FindSocialIdsByEmpID")]
        public async Task<object> FindSocialIdsByEmpID([FromBody] Utils Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (Utils == null)
                    throw new ArgumentNullException(nameof(Utils.ID));

                var result = _unitOfWork.SocialRepository.FindSocialIdsByEmpID(Utils.ID);

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        //private void SolvedComplainMailtoCustomer()
        //{
        //    try
        //    {
        //        //Mail Message
        //        MailMessage mM = new MailMessage();
        //        //Mail Address
        //        mM.From = new MailAddress("noreply.Interfuture@gmail.com");
        //        //receiver email id

        //        mM.Bcc.Add("sazid@interfuture.ae");

        //        ///////Customer mail Format///////////
        //        mM.Subject = "Your Complaint has been Solved. ";

        //        string mailformat = "Hello world";
        //        mM.IsBodyHtml = true;
        //        mM.Body = mailformat.ToString();
        //        //////////////////////////////////////
        //        //SMTP client
        //        SmtpClient sC = new SmtpClient("smtp.gmail.com");
        //        //port number for Hot mail
        //        sC.Port = 465;
        //        //credentials to login in to hotmail account
        //        sC.Credentials = new NetworkCredential("noreply.Interfuture@gmail.com", "interfuture123");
        //        sC.DeliveryMethod = SmtpDeliveryMethod.Network;
        //        sC.UseDefaultCredentials = false;

        //        //enabled SSL
        //        sC.EnableSsl = true;
        //        //Send an email
        //        sC.Send(mM);
        //    }
        //    catch (Exception ex)
        //    {
        //    }
        //}
    }
}