using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerExportImportModule.Core.Services;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.StoreModule.Core.Services;

namespace VirtoCommerce.CustomerExportImportModule.Data.Services
{
    public sealed class CustomerExportPagedDataSourceFactory: ICustomerExportPagedDataSourceFactory
    {
        private readonly IMemberService _memberService;
        private readonly IMemberSearchService _memberSearchService;
        private readonly IStoreService _storeService;

        public CustomerExportPagedDataSourceFactory(IMemberService memberService, IMemberSearchService memberSearchService, IStoreService storeService)
        {
            _memberService = memberService;
            _memberSearchService = memberSearchService;
            _storeService = storeService;
        }
        
        public ICustomerExportPagedDataSource Create(int pageSize, ExportDataRequest request)
        {
            return new CustomerExportPagedDataSource(_memberService, _memberSearchService, _storeService, pageSize, request);
        }
    }
}
