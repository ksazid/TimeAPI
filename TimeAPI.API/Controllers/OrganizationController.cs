using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TimeAPI.API.Models;
using TimeAPI.API.Models.EmployeeViewModels;
using TimeAPI.API.Services;
using TimeAPI.Domain;
using TimeAPI.Domain.Entities;

namespace TimeAPI.API.Controllers
{
    [EnableCors("CorsPolicy")]
    [Route("[controller]")]
    public class OrganizationController : Controller
    {
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly ApplicationSettings _appSettings;
        private readonly IUnitOfWork _unitOfWork;
        public OrganizationController(IUnitOfWork unitOfWork, ILogger<EmployeeController> logger,
            IEmailSender emailSender,
            ApplicationSettings appSettings)
        {
            _emailSender = emailSender;
            _logger = logger;
            _appSettings = appSettings;
            _unitOfWork = unitOfWork;
        }


        [EnableCors("CorsPolicy")]
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

                organizationViewModel.org_id = Guid.NewGuid().ToString();
                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<OrganizationViewModel, Organization>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Organization>(organizationViewModel);


                _unitOfWork.OrganizationRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Organization Added succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }
    }
}
