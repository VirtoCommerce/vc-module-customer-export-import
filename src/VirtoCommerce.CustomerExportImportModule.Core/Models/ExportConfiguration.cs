using System.Globalization;
using CsvHelper.Configuration;
using VirtoCommerce.Platform.Core.Swagger;

namespace VirtoCommerce.CustomerExportImportModule.Core.Models
{
    [SwaggerSchemaId("CustomerExportConfiguration")]
    public sealed record ExportConfiguration : CsvConfiguration
    {
        public ExportConfiguration() : base(CultureInfo.InvariantCulture)
        {
            Delimiter = ";";
        }
    }
}
