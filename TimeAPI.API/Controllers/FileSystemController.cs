﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Threading;
using System.Threading.Tasks;
using TimeAPI.API.Models;
using TimeAPI.API.Models.EmployeeProfileViewModels;
using TimeAPI.API.Models.EmployeeScreenshotViewModels;
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
        private readonly DateTime _dateTime;

        public FileSystemController(IUnitOfWork unitOfWork, ILogger<FileSystemController> logger, UserManager<ApplicationUser> userManager,
            IEmailSender emailSender, IOptions<ApplicationSettings> AppSettings, IConfiguration configuration, IOptions<StorageSettings> StorageSettings)
        {
            _emailSender = emailSender;
            _logger = logger;
            _appSettings = AppSettings.Value;
            _unitOfWork = unitOfWork;
            _storageSettings = StorageSettings.Value;
            _dateTime = InternetTime.GetCurrentTimeFromTimeZone().Value.DateTime;
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

                        if (await container.ExistsAsync().ConfigureAwait(false))
                        {
                            CloudBlob file = container.GetBlobReference(FormFile.FileName);

                            if (await file.ExistsAsync().ConfigureAwait(false))
                            {
                                await file.DeleteAsync().ConfigureAwait(false);
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
                modal.created_date = _dateTime.ToString();
                modal.is_deleted = false;

                _unitOfWork.ProfileImageRepository.Add(modal);
                _unitOfWork.Commit();

                //return new OkResult();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Profile image uploaded successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                //return new OkResult();
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }

        [HttpPost]
        [Route("AddUploadScreenshot")]
        public async Task<object> AddUploadScreenshot([FromForm] IFormFile FormFile, [FromForm] string emp_id, [FromForm] string org_id, [FromForm] string CreatedBy, CancellationToken cancellationToken)
        {
            try
            {
                EmployeeScreenshotViewModel employeeProfileViewModel = new EmployeeScreenshotViewModel();

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

                employeeProfileViewModel.org_id = org_id;
                employeeProfileViewModel.emp_id = emp_id;
                employeeProfileViewModel.img_name = FormFile.FileName;
                employeeProfileViewModel.createdby = CreatedBy;

                var config = new AutoMapper.MapperConfiguration(m => m.CreateMap<EmployeeScreenshotViewModel, EmployeeScreenshot>());
                var mapper = config.CreateMapper();
                var modal = mapper.Map<EmployeeScreenshot>(employeeProfileViewModel);

                modal.id = Guid.NewGuid().ToString();
                modal.created_date = _dateTime.ToString();
                modal.ondate = _dateTime.ToString();
                modal.is_deleted = false;

                _unitOfWork.EmployeeScreenshotRepository.Add(modal);
                _unitOfWork.Commit();

                //return new OkResult();

                return await Task.FromResult<object>(new SuccessViewModel { Status = "200", Code = "Success", Desc = "Screenshot image uploaded successfully." }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                //return new OkResult();
                return Task.FromResult<object>(new SuccessViewModel { Status = "201", Code = ex.Message, Desc = ex.Message });
            }
        }
    }

    //public static async Task<bool> UploadFileToStorage(Stream fileStream, string fileName,
    //                                            AzureStorageConfig _storageConfig)
    //{
    //    // Create a URI to the blob
    //    Uri blobUri = new Uri("https://" +
    //                          _storageConfig.AccountName +
    //                          ".blob.core.windows.net/" +
    //                          _storageConfig.ImageContainer +
    //                          "/" + fileName);

    //    // Create StorageSharedKeyCredentials object by reading
    //    // the values from the configuration (appsettings.json)
    //    StorageSharedKeyCredential storageCredentials =
    //        new StorageSharedKeyCredential(_storageConfig.AccountName, _storageConfig.AccountKey);

    //    // Create the blob client.
    //    BlobClient blobClient = new BlobClient(blobUri, storageCredentials);

    //    // Upload the file
    //    await blobClient.UploadAsync(fileStream);

    //    return await Task.FromResult(true);
    //}
    //}
}