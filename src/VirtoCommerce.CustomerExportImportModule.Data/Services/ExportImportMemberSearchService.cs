using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Model.Search;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.CustomerModule.Core.Services.Indexed;
using VirtoCommerce.CustomerModule.Data.Repositories;
using VirtoCommerce.CustomerModule.Data.Services;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CustomerExportImportModule.Data.Services
{
    public sealed class ExportImportMemberSearchService : MemberSearchService
    {
        public ExportImportMemberSearchService(Func<IMemberRepository> repositoryFactory, IMemberService memberService, IIndexedMemberSearchService indexedSearchService, IPlatformMemoryCache platformMemoryCache)
            : base(repositoryFactory, memberService, indexedSearchService, platformMemoryCache)
        {
        }

        public override async Task<MemberSearchResult> SearchMembersAsync(MembersSearchCriteria criteria)
        {
            MemberSearchResult result = null;

            if (criteria.DeepSearch && (criteria.MemberId != null || !criteria.ObjectIds.IsNullOrEmpty() || criteria.Keyword != null))
            {
                var orgSkip = criteria.Skip;
                var orgTake = criteria.Take;
                var orgMemberTypes = criteria.MemberTypes.Select(x => x).ToArray();

                const string organizationMemberType = nameof(Organization);
                var withoutOrganizations = !criteria.MemberTypes.Contains(organizationMemberType);

                criteria.Skip = 0;
                criteria.Take = int.MaxValue;

                if (withoutOrganizations)
                {
                    criteria.MemberTypes = criteria.MemberTypes.Union(new[]{
                        organizationMemberType
                    }).ToArray();
                }

                result = await base.SearchMembersAsync(criteria);

                var organizations = result.Results.OfType<Organization>().ToArray();

                if (withoutOrganizations)
                {
                    result.Results = result.Results.Where(x => orgMemberTypes.Contains(x.MemberType)).ToList();
                    result.TotalCount = result.Results.Count;
                }

                if (!organizations.IsNullOrEmpty())
                {
                    await LoadChildren(criteria, organizations, withoutOrganizations, orgMemberTypes, result);
                }

                //skip take as firstly
                result.Results = result.Results.Skip(orgSkip).Take(orgTake).ToList();
            }
            else
            {
                result = await base.SearchMembersAsync(criteria);
            }

            return result;
        }

        private async Task LoadChildren(MembersSearchCriteria criteria, IEnumerable<Organization> organizations, bool withoutOrganizations, string[] orgMemberTypes, MemberSearchResult result)
        {
            foreach (var organization in organizations)
            {
                var searchChildrenCriteria = new MembersSearchCriteria()
                {
                    MemberId = organization.Id,
                    DeepSearch = true,
                    Skip = 0,
                    Take = int.MaxValue,
                    Sort = criteria.Sort,
                    MemberTypes = criteria.MemberTypes,
                    ResponseGroup = criteria.ResponseGroup
                };

                var searchChildrenResult = await RegularSearchMembersAsync(searchChildrenCriteria);

                var childOrganizations = searchChildrenResult.Results.OfType<Organization>().ToArray();

                if (withoutOrganizations)
                {
                    searchChildrenResult.Results = searchChildrenResult.Results.Where(x => orgMemberTypes.Contains(x.MemberType)).ToList();
                    searchChildrenResult.TotalCount = searchChildrenResult.Results.Count;
                }

                result.Results.AddRange(searchChildrenResult.Results);
                result.TotalCount += searchChildrenResult.TotalCount;

                if (!childOrganizations.IsNullOrEmpty())
                {
                    await LoadChildren(criteria, childOrganizations, withoutOrganizations, orgMemberTypes, result);
                }
            }
        }
    }
}
