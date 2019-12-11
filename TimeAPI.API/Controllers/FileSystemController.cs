using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using ailogica.Azure.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using TimeAPI.API.Models;
using TimeAPI.API.Models.EmployeeProfileViewModels;
using TimeAPI.API.Models.EmployeeViewModels;
using TimeAPI.API.Services;
using TimeAPI.Domain;
using TimeAPI.Domain.Entities;

namespace TimeAPI.API.Controllers
{
    [Route("[controller]")]
    public class FileSystemController : Controller
    {
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly ApplicationSettings _appSettings;
        private readonly IUnitOfWork _unitOfWork;
        private IConfiguration _configuration;
        private StorageSettings _storageSettings;
        public FileSystemController(IUnitOfWork unitOfWork, ILogger<FileSystemController> logger, UserManager<ApplicationUser> userManager,
            IEmailSender emailSender, IOptions<ApplicationSettings> AppSettings, IConfiguration configuration, IOptions<StorageSettings> StorageSettings)
        {
            _emailSender = emailSender;
            _logger = logger;
            _appSettings = AppSettings.Value;
            _unitOfWork = unitOfWork;
            _storageSettings = StorageSettings.Value;
        }


        [HttpPost]
        [Route("AddUploadProfile")]
        public async Task<object> AddUploadProfile([FromForm] IFormFile FormFile, [FromForm] string UserID, [FromForm] string CreatedBy, CancellationToken cancellationToken)
        {
            try
            {
                EmployeeProfileViewModel employeeProfileViewModel = new EmployeeProfileViewModel();

                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                if (FormFile == null)
                    throw new ArgumentNullException(nameof(FormFile));

                try
                {
                    if (CloudStorageAccount.TryParse(_storageSettings.StorageDefaultConnection, out CloudStorageAccount storageAccount))
                    {
                        CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                        CloudBlobContainer container = blobClient.GetContainerReference(_storageSettings.Container);

                        CloudBlockBlob blockBlob = container.GetBlockBlobReference(FormFile.FileName);

                        if (await container.ExistsAsync())
                        {
                            CloudBlob file = container.GetBlobReference(FormFile.FileName);

                            if (await file.ExistsAsync())
                            {
                                await file.DeleteAsync();
                            }
                        }

                        await blockBlob.UploadFromStreamAsync(FormFile.OpenReadStream()).ConfigureAwait(false);

                        employeeProfileViewModel.img_url = blockBlob.Uri.AbsoluteUri;
                    }
                }
                catch (Exception et)
                {
                    return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = et.Message, Desc = et.Message });
                }


                employeeProfileViewModel.user_id = UserID;
                employeeProfileViewModel.img_name = FormFile.FileName;
                employeeProfileViewModel.createdby = CreatedBy;


                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<EmployeeProfileViewModel, Image>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<Image>(employeeProfileViewModel);

                modal.id = Guid.NewGuid().ToString();
                modal.created_date = DateTime.Now.ToString(CultureInfo.CurrentCulture);
                modal.is_deleted = false;

                _unitOfWork.ProfileImageRepository.Add(modal);
                _unitOfWork.Commit();

                //return new OkResult();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Profile image uploaded succefully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {

                //return new OkResult();
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }
    }
}