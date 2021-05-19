using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Model.Search;

namespace VirtoCommerce.CustomerExportImportModule.Core.Models
{
    public sealed class ExportDataRequest
    {
        public string Keyword { get; set; }

        public string[] ObjectIds { get; set; }

        public string OrganizationId { get; set; }

        public MembersSearchCriteria ToSearchCriteria()
        {
            return new MembersSearchCriteria
            {
                Keyword = Keyword,
                ObjectIds = ObjectIds,
                MemberId = OrganizationId,
                MemberTypes = new []{ nameof(Contact), nameof(Organization) },
                DeepSearch = true
            };
        }
    }
}
