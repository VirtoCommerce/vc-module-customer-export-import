using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.CustomerExportImportModule.Core;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerExportImportModule.Core.Services;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CustomerExportImportModule.Web.Controllers.Api
{
    [Route("api/customers/import")]
    [Authorize(ModuleConstants.Security.Permissions.ImportAccess)]
    [ApiController]
    public class ImportController : ControllerBase
    {
        private readonly ICsvCustomerDataValidator _csvCustomerDataValidator;
        private readonly IBlobStorageProvider _blobStorageProvider;
        private readonly ICustomerImportPagedDataSourceFactory _customerImportPagedDataSourceFactory;

        public ImportController(IBlobStorageProvider blobStorageProvider,
            ICustomerImportPagedDataSourceFactory customerImportPagedDataSourceFactory, ICsvCustomerDataValidator csvCustomerDataValidator)
        {
            _csvCustomerDataValidator = csvCustomerDataValidator;
            _blobStorageProvider = blobStorageProvider;
            _customerImportPagedDataSourceFactory = customerImportPagedDataSourceFactory;
        }

        [HttpPost]
        [Route("validate")]
        public async Task<ActionResult<ImportDataValidationResult>> Validate([FromBody] ImportDataValidationRequest request)
        {
            if (request.FilePath.IsNullOrEmpty())
            {
                return BadRequest($"{nameof(request.FilePath)} can not be null or empty.");
            }

            var result = await _csvCustomerDataValidator.ValidateAsync(request.FilePath);

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

            using var csvDataSource = await _customerImportPagedDataSourceFactory.CreateAsync(request.FilePath, 10);

            var result = new ImportDataPreview
            {
                TotalCount = csvDataSource.GetTotalCount()
            };

            await csvDataSource.FetchAsync();

            result.Results = csvDataSource.Items.Select(item => item.Record).ToArray();

            return Ok(result);
        }
    }
}
