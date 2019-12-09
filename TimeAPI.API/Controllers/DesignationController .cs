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
using TimeAPI.API.Models.DesignationViewModels;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace TimeAPI.API.Controllers
{
    //[ApiKeyAuth]
    //[EnableCors("CorsPolicy")]
    [Route("[controller]")]
    //[Authorize(Roles = "superadmin")]
    public class DesignationController : Controller
    {
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly ApplicationSettings _appSettings;
        private readonly IUnitOfWork _unitOfWork;
        public DesignationController(IUnitOfWork unitOfWork, ILogger<DesignationController> logger,
            IEmailSender emailSender,
            IOptions<ApplicationSettings> AppSettings)
        {
            _emailSender = emailSender;
            _logger = logger;
            _appSettings = AppSettings.Value;
            _unitOfWork = unitOfWork;
        }

        [HttpPost]
        [Route("AddDesignation")]
        public async Task<object> AddDesignation([FromBody] DesignationViewModel designationViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (designationViewModel == null)
                    throw new ArgumentNullException(nameof(designationViewModel));

                designationViewModel.id = Guid.NewGuid().ToString();
                designationViewModel.created_date = DateTime.Now.ToString();
                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<DesignationViewModel, Designation>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Designation>(designationViewModel);

                _unitOfWork.DesignationRepositiory.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Designation registered succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPatch]
        [Route("UpdateDesignation")]
        public async Task<object> UpdateDesignation([FromBody]  DesignationViewModel designationViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (designationViewModel == null)
                    throw new ArgumentNullException(nameof(designationViewModel));

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<DesignationViewModel, Designation>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Designation>(designationViewModel);

                designationViewModel.modified_date = DateTime.Now.ToString();
                _unitOfWork.DesignationRepositiory.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Designation updated succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        
        [HttpPost]
        [Route("RemoveDesignation")]
        public async Task<object> RemoveDesignation([FromBody] Utils _Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (_Utils == null)
                    throw new ArgumentNullException(nameof(_Utils.ID));

                _unitOfWork.DesignationRepositiory.Remove(_Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Designation removed succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        
        [HttpGet]
        [Route("GetAllDesignation")]
        public async Task<object> GetAllDesignation(CancellationToken cancellationToken)
        {

            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.DesignationRepositiory.All();
                _unitOfWork.Commit();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }



        [HttpPost]
        [Route("FindByDesignationID")]
        public async Task<object> FindByDesignationID([FromBody] Utils _Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (_Utils == null)
                    throw new ArgumentNullException(nameof(_Utils.ID));

                var result = _unitOfWork.DesignationRepositiory.Find(_Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }


        [HttpPost]
        [Route("FindByDesignationName")]
        public async Task<object> FindByDesignationName([FromBody] UtilsName _Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (_Utils == null)
                    throw new ArgumentNullException(nameof(_Utils.FullName));

                var result = _unitOfWork.DesignationRepositiory.FindByDesignationName(_Utils.FullName);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        
        [HttpPost]
        [Route("FindByDesignationAlias")]
        public async Task<object> FindByDesignationAlias([FromBody] UtilsAlias _UtilsAlias, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (_UtilsAlias == null)
                    throw new ArgumentNullException(nameof(_UtilsAlias.Alias));

                var result = _unitOfWork.DesignationRepositiory.FindByDesignationAlias(_UtilsAlias.Alias);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        
        [HttpPost]
        [Route("FindDesignationByDeptID")]
        public async Task<object> FindDesignationByDeptID([FromBody] Utils _Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (_Utils == null)
                    throw new ArgumentNullException(nameof(_Utils.ID));

                var result = _unitOfWork.DesignationRepositiory.FindDesignationByDeptID(_Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("FetchGridDataByDesignationDeptOrgID")]
        public async Task<object> FetchGridDataByDesignationDeptOrgID([FromBody] UtilsOrgID _UtilsOrgID, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (_UtilsOrgID == null)
                    throw new ArgumentNullException(nameof(_UtilsOrgID.OrgID));

                oDataTable _oDataTable = new oDataTable();
                IEnumerable<dynamic> results = _unitOfWork.DesignationRepositiory.FetchGridDataByDesignationByDeptOrgID(_UtilsOrgID.OrgID);
                var xResult = _oDataTable.ToDataTable(results);
                _unitOfWork.Commit();

                return await System.Threading.Tasks.Task.FromResult<object>(JsonConvert.SerializeObject(xResult, Formatting.Indented)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return System.Threading.Tasks.Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

    }
}
