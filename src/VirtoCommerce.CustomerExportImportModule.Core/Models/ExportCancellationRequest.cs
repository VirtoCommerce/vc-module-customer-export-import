using VirtoCommerce.Platform.Core.Swagger;

namespace VirtoCommerce.CustomerExportImportModule.Core.Models
{
    [SwaggerSchemaId("CustomerExportCancellationRequest")]
    public sealed class ExportCancellationRequest
    {
        public string JobId { get; set; }
    }
}
