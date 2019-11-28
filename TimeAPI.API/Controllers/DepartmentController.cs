using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TimeAPI.API.Models;
using TimeAPI.API.Models.DepartmentViewModels;
using TimeAPI.API.Models.EmployeeViewModels;
using TimeAPI.API.Services;
using TimeAPI.Domain;
using TimeAPI.Domain.Entities;

namespace TimeAPI.API.Controllers
{
    [EnableCors("CorsPolicy")]
    [Route("[controller]")]
    //[Authorize]
    public class DepartmentController : Controller
    {
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly ApplicationSettings _appSettings;
        private readonly IUnitOfWork _unitOfWork;
        public DepartmentController(IUnitOfWork unitOfWork, ILogger<EmployeeController> logger,
            IEmailSender emailSender,
           IOptions<ApplicationSettings> AppSettings)
        {
            _emailSender = emailSender;
            _logger = logger;
            _appSettings = AppSettings.Value;
            _unitOfWork = unitOfWork;
        }

        
        [HttpPost]
        [Route("AddDepartment")]
        public async Task<object> AddDepartment([FromBody] DepartmentViewModel departmentViewModels, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (departmentViewModels == null)
                    throw new ArgumentNullException(nameof(departmentViewModels));

                departmentViewModels.id = Guid.NewGuid().ToString();
                departmentViewModels.created_date = DateTime.Now.ToString();

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<DepartmentViewModel, Department>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Department>(departmentViewModels);

                _unitOfWork.DepartmentRepository.Add(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Department registered succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        
        [HttpPut]
        [Route("UpdateDepartment")]
        public async Task<object> UpdateDepartment([FromBody] DepartmentViewModel departmentViewModels, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (departmentViewModels == null)
                    throw new ArgumentNullException(nameof(departmentViewModels));

                departmentViewModels.modified_date = DateTime.Now.ToString();
                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<DepartmentViewModel, Department>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Department>(departmentViewModels);


                _unitOfWork.DepartmentRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Department updated succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        
        [HttpPost]
        [Route("RemoveDepartment")]
        public async Task<object> RemoveDepartment([FromBody] Utils _Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (_Utils == null)
                    throw new ArgumentNullException(nameof(_Utils.ID));

                _unitOfWork.DepartmentRepository.Remove(_Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Department removed succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        
        [HttpGet]
        [Route("GetAllDepartments")]
        public async Task<object> GetAllDepartments(CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.DepartmentRepository.All();
                _unitOfWork.Commit();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        
        [HttpPost]
        [Route("FindByDepartmentName")]
        public async Task<object> FindByDepartmentName([FromBody] UtilsName _UtilsName, CancellationToken cancellationToken)
        {

            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (_UtilsName == null)
                    throw new ArgumentNullException(nameof(_UtilsName.FullName));

                var result = _unitOfWork.DepartmentRepository.FindByDepartmentName(_UtilsName.FullName);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        
        [HttpPost]
        [Route("FindByDepartmentAlias")]
        public async Task<object> FindByDepartmentAlias([FromBody] UtilsAlias _UtilsAlias, CancellationToken cancellationToken)
        {

            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (_UtilsAlias == null)
                    throw new ArgumentNullException(nameof(_UtilsAlias.Alias));

                var result = _unitOfWork.DepartmentRepository.FindByDepartmentAlias(_UtilsAlias.Alias);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        
        [HttpPost]
        [Route("FindDepartmentByOrgID")]
        public async Task<object> FindDepartmentByOrgID([FromBody] Utils _Utils, CancellationToken cancellationToken)
        {

            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (_Utils == null)
                    throw new ArgumentNullException(nameof(_Utils.ID));

                var result = _unitOfWork.DepartmentRepository.FindDepartmentByOrgID(_Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        
        [HttpPost]
        [Route("FindByDepartmentID")]
        public async Task<object> FindByDepartmentID([FromBody] Utils _Utils, CancellationToken cancellationToken)
        {

            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (_Utils == null)
                    throw new ArgumentNullException(nameof(_Utils.ID));

                var result = _unitOfWork.DepartmentRepository.Find(_Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        
        [HttpPost]
        [Route("FindDepLeadByDepID")]
        public async Task<object> FindDepLeadByDepID([FromBody] Utils _Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (_Utils == null)
                    throw new ArgumentNullException(nameof(_Utils.ID));

                var result = _unitOfWork.DepartmentRepository.FindDepLeadByDepID(_Utils.ID);
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
