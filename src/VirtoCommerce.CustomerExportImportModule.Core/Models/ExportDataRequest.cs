using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Model.Search;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Swagger;

namespace VirtoCommerce.CustomerExportImportModule.Core.Models
{
    [SwaggerSchemaId("CustomerExportDataRequest")]
    public sealed class ExportDataRequest
    {
        public string Keyword { get; set; }

        public string[] ObjectIds { get; set; }

        public string OrganizationId { get; set; }

        public MembersSearchCriteria ToSearchCriteria()
        {
            return new ExtendedMembersSearchCriteria
            {
                Keyword = ObjectIds.IsNullOrEmpty() ? Keyword : null, // if concrete members selected there is no index searching
                ObjectIds = ObjectIds,
                MemberId = OrganizationId,
                MemberTypes = new[] { nameof(Contact), nameof(Organization) },
                DeepSearch = true,
                ExportData = true
            };
        }
    }
}
