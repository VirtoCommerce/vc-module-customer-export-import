using System;
using System.Threading.Tasks;
using VirtoCommerce.CustomerModule.Core.Model.Search;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.CustomerModule.Core.Services.Indexed;
using VirtoCommerce.CustomerModule.Data.Repositories;
using VirtoCommerce.CustomerModule.Data.Services;
using VirtoCommerce.Platform.Core.Caching;

namespace VirtoCommerce.CustomerExportImportModule.Data.Services
{
    public sealed class ExportImportMemberSearchService : MemberSearchService
    {
        public ExportImportMemberSearchService(Func<IMemberRepository> repositoryFactory, IMemberService memberService, IIndexedMemberSearchService indexedSearchService, IPlatformMemoryCache platformMemoryCache)
            : base(repositoryFactory, memberService, indexedSearchService, platformMemoryCache)
        {
        }

        public override Task<MemberSearchResult> SearchMembersAsync(MembersSearchCriteria criteria)
        {
            return base.SearchMembersAsync(criteria);
        }
    }
}
