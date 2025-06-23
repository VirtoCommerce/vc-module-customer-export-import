using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.AssetsModule.Core.Assets;
using VirtoCommerce.CustomerExportImportModule.Core;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerExportImportModule.Core.Services;
using VirtoCommerce.CustomerExportImportModule.Web.BackgroundJobs;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.PushNotifications;
using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.CustomerExportImportModule.Web.Controllers.Api
{
    [Route("api/customer/import")]
    [Authorize(ModuleConstants.Security.Permissions.ImportAccess)]
    [ApiController]
    public class ImportController : ControllerBase
    {
        private readonly ICsvCustomerDataValidator _csvCustomerDataValidator;
        private readonly IBlobStorageProvider _blobStorageProvider;
        private readonly ICustomerImportPagedDataSourceFactory _customerImportPagedDataSourceFactory;
        private readonly IUserNameResolver _userNameResolver;
        private readonly IPushNotificationManager _pushNotificationManager;

        public ImportController(IBlobStorageProvider blobStorageProvider,
            ICustomerImportPagedDataSourceFactory customerImportPagedDataSourceFactory, ICsvCustomerDataValidator csvCustomerDataValidator,
            IUserNameResolver userNameResolver, IPushNotificationManager pushNotificationManager)
        {
            _csvCustomerDataValidator = csvCustomerDataValidator;
            _blobStorageProvider = blobStorageProvider;
            _customerImportPagedDataSourceFactory = customerImportPagedDataSourceFactory;
            _userNameResolver = userNameResolver;
            _pushNotificationManager = pushNotificationManager;
        }

        [HttpPost]
        [Route("validate")]
        public async Task<ActionResult<ImportDataValidationResult>> Validate([FromBody] ImportDataValidationRequest request)
        {
            if (request.FilePath.IsNullOrEmpty())
            {
                return BadRequest($"{nameof(request.FilePath)} can not be null or empty.");
            }

            var result = await _csvCustomerDataValidator.ValidateAsync(request.DataType, request.FilePath);

            return Ok(result);
        }

        [HttpPost]
        [Route("preview")]
        public async Task<ActionResult<ImportDataPreview>> GetImportPreview([FromBody] ImportDataPreviewRequest request)
        {
            if (request.FilePath.IsNullOrEmpty())
            {
                return BadRequest($"{nameof(request.FilePath)} can not be null");
            }

            var blobInfo = await _blobStorageProvider.GetBlobInfoAsync(request.FilePath);

            if (blobInfo == null)
            {
                return BadRequest("Blob with the such url does not exist.");
            }

            var result = new ImportDataPreview();

            switch (request.DataType)
            {
                case nameof(Contact):
                    using (var csvDataSource = await _customerImportPagedDataSourceFactory.CreateAsync<ImportableContact, Contact>(request.FilePath, 10))
                    {
                        result.TotalCount = csvDataSource.GetTotalCount();
                        await csvDataSource.FetchAsync();
                        result.Results = csvDataSource.Items.Select(item => item.Record).ToArray();
                    }
                    break;
                case nameof(Organization):
                    using (var csvDataSource = await _customerImportPagedDataSourceFactory.CreateAsync<ImportableOrganization, Organization>(request.FilePath, 10))
                    {
                        result.TotalCount = csvDataSource.GetTotalCount();
                        await csvDataSource.FetchAsync();
                        result.Results = csvDataSource.Items.Select(item => item.Record).ToArray();
                    }
                    break;
            }

            return Ok(result);
        }

        [HttpPost]
        [Route("run")]
        public async Task<ActionResult<ExportPushNotification>> RunImport([FromBody] ImportDataRequest request)
        {
            var notification = new ImportPushNotification(_userNameResolver.GetCurrentUserName())
            {
                Title = "Customers import",
                Description = "Starting import task..."
            };

            await _pushNotificationManager.SendAsync(notification);

            notification.JobId = BackgroundJob.Enqueue<ImportJob>(job => job.ImportBackgroundAsync(request, notification, JobCancellationToken.Null, null));

            return Ok(notification);
        }

        [HttpPost]
        [Route("cancel")]
        public ActionResult CancelImport([FromBody] ImportCancellationRequest cancellationRequest)
        {
            BackgroundJob.Delete(cancellationRequest.JobId);
            return Ok();
        }
    }
}
