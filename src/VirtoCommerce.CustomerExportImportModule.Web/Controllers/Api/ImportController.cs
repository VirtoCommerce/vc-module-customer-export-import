using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.CustomerExportImportModule.Core;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerExportImportModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CustomerExportImportModule.Web.Controllers.Api
{
    [Route("api/customers/import")]
    [Authorize(ModuleConstants.Security.Permissions.ImportAccess)]
    [ApiController]
    public class ImportController : ControllerBase
    {
        private readonly ICsvCustomerDataValidator _csvCustomerDataValidator;

        public ImportController(ICsvCustomerDataValidator csvCustomerDataValidator)
        {
            _csvCustomerDataValidator = csvCustomerDataValidator;
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
    }
}
