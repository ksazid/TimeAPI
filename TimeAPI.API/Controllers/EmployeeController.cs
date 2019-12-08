using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TimeAPI.API.Models;
using TimeAPI.API.Models.EmployeeProfileViewModels;
using TimeAPI.API.Models.EmployeeViewModels;
using TimeAPI.API.Services;
using TimeAPI.Domain;
using TimeAPI.Domain.Entities;

namespace TimeAPI.API.Controllers
{
    [EnableCors("CorsPolicy")]
    [Route("[controller]")]
    //[Authorize]
    public class EmployeeController : Controller
    {
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly ApplicationSettings _appSettings;
        private readonly IUnitOfWork _unitOfWork;
        public IConfiguration _configuration;
        public IHostingEnvironment _hostingEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;
        public EmployeeController(IUnitOfWork unitOfWork, ILogger<EmployeeController> logger, UserManager<ApplicationUser> userManager,
            IEmailSender emailSender,
           IOptions<ApplicationSettings> AppSettings, IConfiguration configuration,
           IHostingEnvironment hostingEnvironment)
        {
            _emailSender = emailSender;
            _logger = logger;
            _appSettings = AppSettings.Value;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _hostingEnvironment = hostingEnvironment;
            _userManager = userManager;
        }


        [HttpPost]
        [Route("AddEmployee")]
        public async Task<object> AddEmployee([FromBody] EmployeeViewModel employeeViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (employeeViewModel == null)
                    throw new ArgumentNullException(nameof(employeeViewModel));

                #region User
                string _userName = "";
                Role role = null;
                if (employeeViewModel.email != null)
                {
                    _userName = employeeViewModel.email;
                }
                if (employeeViewModel.phone != null)
                {
                    _userName = employeeViewModel.phone;
                }
                if (employeeViewModel.role_id != null)
                {
                    role = _unitOfWork.RoleRepository.Find(employeeViewModel.role_id.ToString());
                    if (role.NormalizedName == "ADMIN")
                        employeeViewModel.is_admin = true;

                }

                var user = new ApplicationUser()
                {
                    UserName = _userName,
                    Email = employeeViewModel.email,
                    FirstName = employeeViewModel.first_name,
                    LastName = employeeViewModel.last_name,
                    FullName = employeeViewModel.full_name,
                    Role = role.Name,
                    Phone = employeeViewModel.phone
                };

                var result = await _userManager.CreateAsync(user, user.UserName).ConfigureAwait(true);
                var xRest = await _userManager.AddToRoleAsync(user, role.Name).ConfigureAwait(true);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    if (user.Email != null)
                    {
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user).ConfigureAwait(true);
                        var callbackUrl = Url.EmailConfirmationLink(user.Id, code, Request.Scheme);
                        await _emailSender.SendEmailConfirmationAsync(user.Email, callbackUrl).ConfigureAwait(true);
                    }
                    else
                    {
                        // check if its a phone 
                    }
                }

                #endregion User

                #region Employee

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<EmployeeViewModel, Employee>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Employee>(employeeViewModel);

                modal.id = Guid.NewGuid().ToString();
                modal.created_date = DateTime.Now.ToString();

                _unitOfWork.EmployeeRepository.Add(modal);

                #endregion

                #region Read File Content

                var uploads = Path.Combine(_hostingEnvironment.WebRootPath, "Uploads");
                bool exists = Directory.Exists(uploads);
                if (!exists)
                    Directory.CreateDirectory(uploads);

                var fileName = Path.GetFileName(employeeViewModel.imgurl_name.FileName);
                var fileStream = new FileStream(Path.Combine(uploads, employeeViewModel.imgurl_name.FileName), FileMode.Create);
                string mimeType = employeeViewModel.imgurl_name.ContentType;
                byte[] fileData = new byte[employeeViewModel.imgurl_name.Length];

                BlobStorageService objBlobService = new BlobStorageService(_configuration.GetConnectionString("StorageDefaultConnection"));

                employeeViewModel.imgurl = objBlobService.UploadFileToBlob(employeeViewModel.imgurl_name.FileName, fileData, mimeType);

                EmployeeProfileViewModel employeeProfileViewModel = new EmployeeProfileViewModel
                {
                    user_id = "",
                    img_name = fileName,
                    img_url = employeeViewModel.imgurl,
                    createdby = employeeViewModel.createdby
                };


                var config1 = new AutoMapper.MapperConfiguration(m => m.CreateMap<EmployeeProfileViewModel, Image>());
                var mapper1 = config.CreateMapper();
                var modal1 = mapper.Map<Image>(employeeProfileViewModel);

                modal1.id = Guid.NewGuid().ToString();
                modal1.created_date = DateTime.Now.ToString();
                _unitOfWork.ProfileImageRepository.Add(modal1);

                #endregion

                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Employee registered succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }


        [HttpPatch]
        [Route("UpdateEmployee")]
        public async Task<object> UpdateEmployee([FromBody] EmployeeViewModel employeeViewModel, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (employeeViewModel == null)
                    throw new ArgumentNullException(nameof(employeeViewModel));

                employeeViewModel.modified_date = DateTime.Now.ToString();
                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<EmployeeViewModel, Employee>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Employee>(employeeViewModel);


                _unitOfWork.EmployeeRepository.Update(modal);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Employee updated succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }


        [HttpPost]
        [Route("RemoveEmployee")]
        public async Task<object> RemoveEmployee([FromBody] Utils _Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (_Utils == null)
                    throw new ArgumentNullException(nameof(_Utils.ID));

                _unitOfWork.EmployeeRepository.Remove(_Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Employee removed succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }


        [HttpGet]
        [Route("GetAllEmployees")]
        public async Task<object> GetAllEmployees(CancellationToken cancellationToken)
        {

            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var result = _unitOfWork.EmployeeRepository.All();
                _unitOfWork.Commit();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }


        [HttpPost]
        [Route("FindByEmpID")]
        public async Task<object> FindByEmpID([FromBody] Utils _Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (_Utils == null)
                    throw new ArgumentNullException(nameof(_Utils.ID));

                var result = _unitOfWork.EmployeeRepository.Find(_Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }


        [HttpPost]
        [Route("FindByEmpName")]
        public async Task<object> FindByEmpName([FromBody] UtilsName _UtilsName, CancellationToken cancellationToken)
        {

            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (_UtilsName == null)
                    throw new ArgumentNullException(nameof(_UtilsName));

                var result = _unitOfWork.EmployeeRepository.FindByEmpName(_UtilsName.FullName);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }


        [HttpPost]
        [Route("FindByOrgID")]
        public async Task<object> FindByOrgID([FromBody] Utils _Utils, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (_Utils == null)
                    throw new ArgumentNullException(nameof(_Utils.ID));

                var result = _unitOfWork.EmployeeRepository.FindByOrgIDCode(_Utils.ID);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }


        [HttpPost]
        [Route("FindByEmpCode")]
        public async Task<object> FindByEmpCode([FromBody] UtilsCode _EmpCode, CancellationToken cancellationToken)
        {

            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (_EmpCode == null)
                    throw new ArgumentNullException(nameof(_EmpCode.Code));

                var result = _unitOfWork.EmployeeRepository.FindByEmpCode(_EmpCode.Code);
                _unitOfWork.Commit();

                return await Task.FromResult<object>(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }


        [HttpPost]
        [Route("FindByRoleName")]
        public async Task<object> FindByRoleName([FromBody] UtilsRole _UtilsRole, CancellationToken cancellationToken)
        {

            try
            {
                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (_UtilsRole == null)
                    throw new ArgumentNullException(nameof(_UtilsRole.Role));

                var result = _unitOfWork.EmployeeRepository.FindByRoleName(_UtilsRole.Role);
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
