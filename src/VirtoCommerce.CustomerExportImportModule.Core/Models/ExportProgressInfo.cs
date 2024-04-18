using VirtoCommerce.Platform.Core.Swagger;

namespace VirtoCommerce.CustomerExportImportModule.Core.Models
{
    [SwaggerSchemaId("CustomerExportProgressInfo")]
    public sealed class ExportProgressInfo
    {
        public int ProcessedCount { get; set; }

        public int TotalCount { get; set; }

        public string Description { get; set; }

        public string ContactsFileUrl { get; set; }
        public string OrganizationsFileUrl { get; set; }
    }
}
