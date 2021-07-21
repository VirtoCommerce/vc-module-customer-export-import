using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Model.Search;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.CustomerModule.Core.Services.Indexed;
using VirtoCommerce.CustomerModule.Data.Model;
using VirtoCommerce.CustomerModule.Data.Repositories;
using VirtoCommerce.CustomerModule.Data.Services;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CustomerExportImportModule.Data.Services
{
    public sealed class ExportImportMemberSearchService : MemberSearchService
    {
        private const int ElasticMaxTake = 10000;
        const string OrganizationMemberType = nameof(Organization);

        public ExportImportMemberSearchService(Func<IMemberRepository> repositoryFactory, IMemberService memberService, IIndexedMemberSearchService indexedSearchService, IPlatformMemoryCache platformMemoryCache)
            : base(repositoryFactory, memberService, indexedSearchService, platformMemoryCache)
        {
        }

        public override async Task<MemberSearchResult> SearchMembersAsync(MembersSearchCriteria criteria)
        {
            var result = new MemberSearchResult();

            if (criteria == null || (!criteria.DeepSearch || (criteria.MemberId == null && criteria.ObjectIds.IsNullOrEmpty() && criteria.Keyword.IsNullOrEmpty())))
            {
                result = await base.SearchMembersAsync(criteria);
            }
            else
            {
                var orgSkip = criteria.Skip;
                var orgTake = criteria.Take;
                var orgMemberTypes = criteria.MemberTypes?.Select(x => x).ToArray();
                //do not use index searching if concrete members are selected
                if (!criteria.ObjectIds.IsNullOrEmpty())
                {
                    criteria.Keyword = null;
                }

                var withoutOrganizations = criteria.MemberTypes != null && !criteria.MemberTypes.Contains(OrganizationMemberType);

                criteria.Skip = 0;
                criteria.Take = criteria.Keyword.IsNullOrEmpty() ? int.MaxValue : ElasticMaxTake;

                if (withoutOrganizations)
                {
                    criteria.MemberTypes = criteria.MemberTypes.Union(new[]{
                        OrganizationMemberType
                    }).ToArray();
                }

                var firstResult = await base.SearchMembersAsync(criteria);

                var organizations = firstResult.Results.OfType<Organization>().ToArray();

                result.Results = withoutOrganizations ? firstResult.Results.Where(x => orgMemberTypes.Contains(x.MemberType)).ToList() : firstResult.Results.ToList();
                result.TotalCount = result.Results.Count;

                if (!organizations.IsNullOrEmpty())
                {
                    await LoadChildren(criteria, organizations, withoutOrganizations, orgMemberTypes, result);
                }

                //skip take as firstly
                result.Results = result.Results.Skip(orgSkip).Take(orgTake).ToList();
            }

            return result;
        }

        protected override IQueryable<MemberEntity> BuildQuery(IMemberRepository repository, MembersSearchCriteria criteria)
        {
            var query = base.BuildQuery(repository, criteria);

            if (criteria is ExtendedMembersSearchCriteria extendedCriteria && !extendedCriteria.OuterIds.IsNullOrEmpty())
            {
                query = query.Where(m => extendedCriteria.OuterIds.Contains(m.OuterId));
            }

            return query;
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

                var resultIds = result.Results.Select(x => x.Id).ToArray();
                childResults = childResults.Where(c => !resultIds.Contains(c.Id)).ToList();
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
