using System.Threading.Tasks;
using VirtoCommerce.CustomerExportImportModule.Core.Models;

namespace VirtoCommerce.CustomerExportImportModule.Core.Services
{
    public interface ICsvCustomerDataValidator
    {
        Task<ImportDataValidationResult> ValidateAsync(string dataType, string filePath);
        Task<ImportDataValidationResult> ValidateAsync<T>(string filePath);
    }
}
