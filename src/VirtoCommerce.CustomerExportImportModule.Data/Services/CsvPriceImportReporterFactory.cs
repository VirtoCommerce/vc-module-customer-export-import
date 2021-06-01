using System.Threading.Tasks;
using VirtoCommerce.CustomerExportImportModule.Core.Services;
using VirtoCommerce.Platform.Core.Assets;

namespace VirtoCommerce.CustomerExportImportModule.Data.Services
{
    public sealed class CsvPriceImportReporterFactory : ICsvCustomerImportReporterFactory
    {
        private readonly IBlobStorageProvider _blobStorageProvider;
        public CsvPriceImportReporterFactory(IBlobStorageProvider blobStorageProvider)
        {
            _blobStorageProvider = blobStorageProvider;
        }

        public async Task<ICsvCustomerImportReporter> CreateAsync(string reportFilePath, string delimiter)
        {
            var reportBlob = await _blobStorageProvider.GetBlobInfoAsync(reportFilePath);

            if (reportBlob != null)
            {
                await _blobStorageProvider.RemoveAsync(new[] { reportFilePath });
            }

            return new CsvCustomerImportReporter(reportFilePath, _blobStorageProvider, delimiter);
        }
    }
}
