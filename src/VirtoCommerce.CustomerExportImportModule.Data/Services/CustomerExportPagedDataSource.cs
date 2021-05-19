using System;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerExportImportModule.Core.Services;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;

namespace VirtoCommerce.CustomerExportImportModule.Data.Services
{
    public sealed class CustomerExportPagedDataSource : ICustomerExportPagedDataSource
    {
        private readonly IMemberSearchService _memberSearchService;
        private readonly ExportDataRequest _request;

        public CustomerExportPagedDataSource(IMemberSearchService memberSearchService, int pageSize, ExportDataRequest request)
        {
            _memberSearchService = memberSearchService;
            _request = request;

            PageSize = pageSize;
        }

        public int CurrentPageNumber { get; private set; }

        public int PageSize { get; }

        public Member[] Items { get; private set; }

        public async Task<int> GetTotalCountAsync()
        {
            var searchCriteria = _request.ToSearchCriteria();
            searchCriteria.Take = 0;
            var searchResult = await _memberSearchService.SearchMembersAsync(searchCriteria);
            return searchResult.TotalCount;
        }

        public async Task<bool> FetchAsync()
        {
            if (CurrentPageNumber * PageSize >= await GetTotalCountAsync())
            {
                Items = Array.Empty<Member>();
                return false;
            }

            var searchCriteria = _request.ToSearchCriteria();
            searchCriteria.Skip = (CurrentPageNumber - 1) * PageSize;
            searchCriteria.Take = PageSize;

            var searchResult = await _memberSearchService.SearchMembersAsync(searchCriteria);

            Items = searchResult.Results.ToArray();

            CurrentPageNumber++;

            return true;
        }
    }
}
