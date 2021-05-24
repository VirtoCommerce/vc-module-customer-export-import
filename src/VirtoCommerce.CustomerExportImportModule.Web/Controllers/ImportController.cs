using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.CustomerExportImportModule.Core;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerExportImportModule.Core.Services;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CustomerExportImportModule.Web.Controllers
{
    [Route("api/pricing/import")]
    [Authorize(ModuleConstants.Security.Permissions.ImportAccess)]
    [ApiController]
    public class ImportController : ControllerBase
    {
        private readonly IBlobStorageProvider _blobStorageProvider;
        private readonly ICustomerImportPagedDataSourceFactory _csvPagedPriceDataSourceFactory;

        public ImportController(IBlobStorageProvider blobStorageProvider,
            ICustomerImportPagedDataSourceFactory csvPagedPriceDataSourceFactory)
        {
            _blobStorageProvider = blobStorageProvider;
            _csvPagedPriceDataSourceFactory = csvPagedPriceDataSourceFactory;
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

            using var csvDataSource = _csvPagedPriceDataSourceFactory.Create(request.FilePath, 10);

            var result = new ImportDataPreview
            {
                TotalCount = csvDataSource.GetTotalCount()
            };

            await csvDataSource.FetchAsync();

            result.Results = csvDataSource.Contacts;

            return Ok(result);
        }
    }
}
