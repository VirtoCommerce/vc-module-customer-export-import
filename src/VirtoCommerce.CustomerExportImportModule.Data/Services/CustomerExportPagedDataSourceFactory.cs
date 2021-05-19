using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerExportImportModule.Core.Services;
using VirtoCommerce.CustomerModule.Core.Services;

namespace VirtoCommerce.CustomerExportImportModule.Data.Services
{
    public sealed class CustomerExportPagedDataSourceFactory: ICustomerExportPagedDataSourceFactory
    {
        private readonly IMemberService _memberService;
        private readonly IMemberSearchService _memberSearchService;


        public CustomerExportPagedDataSourceFactory(IMemberService memberService, IMemberSearchService memberSearchService)
        {
            _memberService = memberService;
            _memberSearchService = memberSearchService;
        }
        
        public ICustomerExportPagedDataSource Create(int pageSize, ExportDataRequest request)
        {
            return new CustomerExportPagedDataSource(_memberService, _memberSearchService, pageSize, request);
        }
    }
}
