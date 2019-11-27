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
using System.Threading;
using TimeAPI.Domain.Entities;
using TimeAPI.API.Models.SocialViewModels;

namespace TimeAPI.API.Controllroers
{
    public class SocialContller : Controller
    {
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly ApplicationSettings _appSettings;
        private readonly IUnitOfWork _unitOfWork;
        public SocialContller(IUnitOfWork unitOfWork, ILogger<SocialContller> logger,
            IEmailSender emailSender,
            IOptions<ApplicationSettings> AppSettings)
        {
            _emailSender = emailSender;
            _logger = logger;
            _appSettings = AppSettings.Value;
            _unitOfWork = unitOfWork;
        }


        [EnableCors("CorsPolicy")]
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

                socialViewModel.id = Guid.NewGuid().ToString();
                socialViewModel.created_date = DateTime.Now.ToString();
                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<SocialViewModel, Social>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Social>(socialViewModel);

                _unitOfWork.SocialRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Social IDs saved successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [EnableCors("CorsPolicy")]
        [HttpPut]
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

                _unitOfWork.SocialRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Social ID updated succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [EnableCors("CorsPolicy")]
        [HttpDelete]
        [Route("RemoveSocial")]
        public async Task<object> RemoveSocial([FromBody] Utils _Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (_Utils == null)
                    throw new ArgumentNullException(nameof(_Utils.ID));

                _unitOfWork.SocialRepository.Remove(_Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Social ID removed succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [EnableCors("CorsPolicy")]
        [HttpGet]
        [Route("GetAllSocial")]
        public async Task<object> GetAllSocial(CancellationToken cancellationToken)
        {

            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.SocialRepository.All();
                _unitOfWork.Commit();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [EnableCors("CorsPolicy")]
        [HttpPost]
        [Route("FindBySocailID")]
        public async Task<object> FindBySocailID([FromBody] Utils _Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (_Utils == null)
                    throw new ArgumentNullException(nameof(_Utils.ID));

                var result = _unitOfWork.SocialRepository.Find(_Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [EnableCors("CorsPolicy")]
        [HttpPost]
        [Route("FindSocialIdsByEmpID")]
        public async Task<object> FindSocialIdsByEmpID([FromBody] Utils _Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (_Utils == null)
                    throw new ArgumentNullException(nameof(_Utils.ID));

                var result = _unitOfWork.SocialRepository.FindSocialIdsByEmpID(_Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }
    
    }
}