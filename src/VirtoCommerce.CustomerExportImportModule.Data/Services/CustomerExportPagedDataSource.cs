using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerExportImportModule.Core.Services;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;

namespace VirtoCommerce.CustomerExportImportModule.Data.Services
{
    public sealed class CustomerExportPagedDataSource : ICustomerExportPagedDataSource
    {
        private readonly IMemberService _memberService;
        private readonly IMemberSearchService _memberSearchService;
        private readonly IStoreService _storeService;
        private readonly ExportDataRequest _request;

        public CustomerExportPagedDataSource(IMemberService memberService, IMemberSearchService memberSearchService, IStoreService storeService, int pageSize, ExportDataRequest request)
        {
            _memberService = memberService;
            _memberSearchService = memberSearchService;
            _storeService = storeService;
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

            // Build search criteria
            var searchCriteria = _request.ToSearchCriteria();
            searchCriteria.Skip = CurrentPageNumber * PageSize;
            searchCriteria.Take = PageSize;
            searchCriteria.ResponseGroup = (MemberResponseGroup.Default | MemberResponseGroup.WithAddresses | MemberResponseGroup.WithDynamicProperties |
                                            MemberResponseGroup.WithEmails | MemberResponseGroup.WithPhones | MemberResponseGroup.WithSecurityAccounts |
                                            MemberResponseGroup.WithGroups).ToString();

            // Get all exporting members from current chunk
            var searchResult = await _memberSearchService.SearchMembersAsync(searchCriteria);

            // Split members into contacts and organizations
            var contacts = searchResult.Results.OfType<Contact>().ToArray();
            var organizations = searchResult.Results.OfType<Organization>().ToArray();

            // Get IDs of contact & organization parent organization
            var contactOrganizationIds = contacts.Select(contact => contact.Organizations?.MinBy(organizationId => organizationId)).Where(x => x != null).Distinct().ToArray();
            var parentOrganizationIds = organizations.Select(organization => organization.ParentId).Where(x => x != null).Distinct().ToArray();

            // Get already loaded organizations and their IDs
            var loadedOrganizations = organizations.Where(organization => contactOrganizationIds.Contains(organization.Id) || parentOrganizationIds.Contains(organization.Id)).ToArray();
            var loadedOrganizationIds = loadedOrganizations.Select(organization => organization.Id);

            // Get not loaded organizations and their IDs
            var notLoadedOrganizationIds = contactOrganizationIds
                .Where(contactOrganizationId => !loadedOrganizationIds.Contains(contactOrganizationId))
                .Concat(parentOrganizationIds.Where(parentOrganizationId => !loadedOrganizationIds.Contains(parentOrganizationId)))
                .ToArray();

            var additionalOrganizations = (await _memberService.GetByIdsAsync(notLoadedOrganizationIds, MemberResponseGroup.Default.ToString())).Cast<Organization>();

            // Get all required organizations
            var allOrganizations = loadedOrganizations.Concat(additionalOrganizations).ToDictionary(x => x.Id, x => x);

            // Get stores from accounts
            var accounts = contacts.Select(contact => contact.SecurityAccounts?.MinBy(account => account.Id)).Where(account => account != null).Distinct().ToArray();
            var storeIds = accounts.Select(account => account.StoreId).Where(storeId => !string.IsNullOrEmpty(storeId)).Distinct().ToList();
            var stores = (await _storeService.GetAsync(storeIds, StoreResponseGroup.None.ToString(), false)).ToDictionary(store => store.Id, store => store).WithDefaultValue(null);

            Items = searchResult.Results.Select(member => GetExportable(member, allOrganizations, stores)).ToArray();

            CurrentPageNumber++;

            return true;
        }

        private static IExportable GetExportable(Member member, IDictionary<string, Organization> organizations, IDictionary<string, Store> stores)
        {
            switch (member.MemberType)
            {
                case nameof(Contact):
                    var contact = (Contact)member;
                    var organizationId = contact.Organizations?.MinBy(organizationId => organizationId);
                    var account = contact.SecurityAccounts?.MinBy(securityAccount => securityAccount.Id);
                    var storeId = account?.StoreId;
                    return new ExportableContact().FromModels(contact,
                        organizationId != null && organizations.TryGetValue(organizationId, out var foundOrganization) ? foundOrganization : null,
                        storeId != null ? stores[storeId] : null);
                case nameof(Organization):
                    var organization = (Organization)member;
                    var parentOrganizationId = organization.ParentId;
                    return new ExportableOrganization().FromModels(organization, parentOrganizationId != null ? organizations[parentOrganizationId] : null);
                default:
                    throw new InvalidDataException();
            }
        }
    }
}
