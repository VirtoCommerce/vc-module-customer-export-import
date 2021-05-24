using CsvHelper.Configuration;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerExportImportModule.Core.Services;
using VirtoCommerce.CustomerExportImportModule.Data.Services;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.Platform.Core.Assets;

namespace VirtoCommerce.PriceExportImportModule.Data.Services
{
    public sealed class CustomerImportPagedDataSourceFactory : ICustomerImportPagedDataSourceFactory
    {
        private readonly IBlobStorageProvider _blobStorageProvider;

        private readonly IMemberService _memberService;

        public CustomerImportPagedDataSourceFactory(IBlobStorageProvider blobStorageProvider, IMemberService memberService)
        {
            _blobStorageProvider = blobStorageProvider;
            _memberService = memberService;
        }

        public ICustomerImportPagedDataSource Create(string filePath, int pageSize, Configuration configuration = null)
        {
            return new CustomerImportPagedDataSource(filePath, _blobStorageProvider, _memberService, pageSize, configuration ?? new ImportConfiguration());
        }
    }
}
