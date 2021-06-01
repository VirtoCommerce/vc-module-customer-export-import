using System.Threading.Tasks;

namespace VirtoCommerce.CustomerExportImportModule.Core.Services
{
    public interface ICsvCustomerImportReporterFactory
    {
        Task<ICsvCustomerImportReporter> CreateAsync(string reportFilePath, string delimiter);
    }
}
