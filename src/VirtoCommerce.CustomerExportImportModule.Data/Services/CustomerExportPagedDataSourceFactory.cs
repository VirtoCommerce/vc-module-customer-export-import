using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerExportImportModule.Core.Services;
using VirtoCommerce.CustomerModule.Core.Services;

namespace VirtoCommerce.CustomerExportImportModule.Data.Services
{
    public sealed class CustomerExportPagedDataSourceFactory: ICustomerExportPagedDataSourceFactory
    {
        private readonly IMemberSearchService _memberSearchService;


        public CustomerExportPagedDataSourceFactory(IMemberSearchService memberSearchService)
        {
            _memberSearchService = memberSearchService;
        }
        
        public ICustomerExportPagedDataSource Create(int pageSize, ExportDataRequest request)
        {
            return new CustomerExportPagedDataSource(_memberSearchService, pageSize, request);
        }
    }
}
