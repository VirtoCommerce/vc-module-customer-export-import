using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerExportImportModule.Core.Services;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Model.Search;
using VirtoCommerce.CustomerModule.Core.Services;

namespace VirtoCommerce.CustomerExportImportModule.Data.Services
{
    public sealed class CustomerExportPagedDataSource : ICustomerExportPagedDataSource
    {
        private readonly IMemberService _memberService;
        private readonly IMemberSearchService _memberSearchService;
        private readonly ExportDataRequest _request;

        public CustomerExportPagedDataSource(IMemberService memberService, IMemberSearchService memberSearchService, int pageSize, ExportDataRequest request)
        {
            _memberService = memberService;
            _memberSearchService = memberSearchService;
            _request = request;

            PageSize = pageSize;
        }

        public int CurrentPageNumber { get; private set; }

        public int PageSize { get; }

        public IExportable[] Items { get; private set; }

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
                Items = Array.Empty<IExportable>();
                return false;
            }

            var searchCriteria = _request.ToSearchCriteria();
            searchCriteria.Skip = (CurrentPageNumber - 1) * PageSize;
            searchCriteria.Take = PageSize;

            var searchResult = await _memberSearchService.SearchMembersAsync(searchCriteria);

            var contacts = searchResult.Results.Where(member => member.MemberType == nameof(Contact)).Cast<Contact>().ToArray();
            var organizations = searchResult.Results.Where(member => member.MemberType == nameof(Organization)).Cast<Organization>().ToArray();

            var contactOrganizationIds = contacts.SelectMany(contact => contact.Organizations).Distinct().ToArray();

            var loadedOrganizations = organizations.Where(organization => contactOrganizationIds.Contains(organization.Id)).ToArray();
            var loadedOrganizationIds = loadedOrganizations.Select(organization => organization.Id);

            var notExistOrganizationIds = contactOrganizationIds.Where(contactOrganizationId => loadedOrganizationIds.Contains(contactOrganizationId)).ToArray();
            var additionalOrganizations = (await _memberService.GetByIdsAsync(notExistOrganizationIds)).Cast<Organization>();

            var allOrganizations = loadedOrganizations.Concat(additionalOrganizations).ToDictionary(x => x.Id, x => x);

            Items = searchResult.Results.Select<Member, IExportable>(member =>
            {
                switch (member.MemberType)
                {
                    case nameof(Contact):
                        var contact = (Contact)member;
                        var organizationId = contact.Organizations.FirstOrDefault();
                        return new ExportableContact().FromModel(contact, organizationId != null ? allOrganizations[organizationId] : null);
                    case nameof(Organization):
                        return new ExportableOrganization().FromModel((Organization)member);
                    default:
                        throw new InvalidDataException();
                }
            }).ToArray();

            CurrentPageNumber++;

            return true;
        }
    }
}
