using VirtoCommerce.Platform.Core.Swagger;

namespace VirtoCommerce.CustomerExportImportModule.Core.Models
{
    [SwaggerSchemaId("CustomerExportOptions")]
    public sealed class ExportOptions
    {
        public int? LimitOfLines { get; set; }
    }
}
