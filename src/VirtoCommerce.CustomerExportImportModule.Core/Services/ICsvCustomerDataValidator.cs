using System.Threading.Tasks;
using VirtoCommerce.CustomerExportImportModule.Core.Models;

namespace VirtoCommerce.CustomerExportImportModule.Core.Services
{
    public interface ICsvCustomerDataValidator
    {
        Task<ImportDataValidationResult> ValidateAsync(string filePath);
    }
}
