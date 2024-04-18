using VirtoCommerce.CustomerModule.Core.Model.Search;
using VirtoCommerce.Platform.Core.Swagger;

namespace VirtoCommerce.CustomerExportImportModule.Core.Models
{
    [SwaggerSchemaId("CustomerExtendedMembersSearchCriteria")]
    public class ExtendedMembersSearchCriteria : MembersSearchCriteria
    {
        public bool? ExportData { get; set; }
    }
}
