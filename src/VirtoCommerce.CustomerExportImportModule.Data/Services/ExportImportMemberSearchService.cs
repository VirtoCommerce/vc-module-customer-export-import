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
            var orgSkip = criteria.Skip;
            var orgTake = criteria.Take;

            criteria.Skip = 0;
            criteria.Take = int.MaxValue;

            var result = await base.SearchMembersAsync(criteria);

            if (criteria.DeepSearch && (criteria.MemberId != null || !criteria.ObjectIds.IsNullOrEmpty() || criteria.Keyword != null))
            {
                var organizations = result.Results.OfType<Organization>().ToArray();
                if (!organizations.IsNullOrEmpty())
                {
                    await LoadChildren(criteria, organizations, result);
                }
            }

            //skip take as firstly
            result.Results = result.Results.Skip(orgSkip).Take(orgTake).ToArray();

            return result;
        }

        private async Task LoadChildren(MembersSearchCriteria criteria, IEnumerable<Organization> organizations, MemberSearchResult result)
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
                    MemberType = criteria.MemberType,
                    ResponseGroup = criteria.ResponseGroup
                };

                var searchChildrenResult = await RegularSearchMembersAsync(searchChildrenCriteria);

                result.Results.AddRange(searchChildrenResult.Results);
                result.TotalCount += searchChildrenResult.TotalCount;

                var childOrganizations = searchChildrenResult.Results.OfType<Organization>().ToArray();

                if (!childOrganizations.IsNullOrEmpty())
                {
                    await LoadChildren(criteria, childOrganizations, result);
                }
            }
        }
    }
}
