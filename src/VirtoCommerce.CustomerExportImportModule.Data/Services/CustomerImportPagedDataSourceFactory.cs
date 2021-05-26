using CsvHelper.Configuration;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerExportImportModule.Core.Services;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.Platform.Core.Assets;

namespace VirtoCommerce.CustomerExportImportModule.Data.Services
{
    public sealed class CustomerImportPagedDataSourceFactory : ICustomerImportPagedDataSourceFactory
    {
        private readonly IBlobStorageProvider _blobStorageProvider;

        public CustomerImportPagedDataSourceFactory(IBlobStorageProvider blobStorageProvider)
        {
            _blobStorageProvider = blobStorageProvider;
        }

        public ICustomerImportPagedDataSource Create(string filePath, int pageSize, Configuration configuration = null)
        {
            return new CustomerImportPagedDataSource(filePath, _blobStorageProvider, pageSize, configuration ?? new ImportConfiguration());
        }
    }
}
