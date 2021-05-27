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
            var result = new MemberSearchResult();

            if (criteria.DeepSearch && (criteria.MemberId != null || !criteria.ObjectIds.IsNullOrEmpty() || criteria.Keyword != null))
            {
                var orgSkip = criteria.Skip;
                var orgTake = criteria.Take;
                var orgMemberTypes = criteria.MemberTypes?.Select(x => x).ToArray();

                const string organizationMemberType = nameof(Organization);
                var withoutOrganizations = criteria.MemberTypes != null && !criteria.MemberTypes.Contains(organizationMemberType);

                criteria.Skip = 0;
                criteria.Take = int.MaxValue;

                if (withoutOrganizations)
                {
                    criteria.MemberTypes = criteria.MemberTypes.Union(new[]{
                        organizationMemberType
                    }).ToArray();
                }

                var firstResult = await base.SearchMembersAsync(criteria);

                var organizations = result.Results.OfType<Organization>().ToArray();

                result.Results = withoutOrganizations ? firstResult.Results.Where(x => orgMemberTypes.Contains(x.MemberType)).ToList() : firstResult.Results.ToList();
                result.TotalCount = firstResult.Results.Count;

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

                var childResults = withoutOrganizations ? searchChildrenResult.Results.Where(x => orgMemberTypes.Contains(x.MemberType)).ToList() : searchChildrenResult.Results.ToList();

                result.Results.AddRange(childResults);
                result.TotalCount += childResults.Count;

                if (!childOrganizations.IsNullOrEmpty())
                {
                    await LoadChildren(criteria, childOrganizations, withoutOrganizations, orgMemberTypes, result);
                }
            }
        }
    }
}
