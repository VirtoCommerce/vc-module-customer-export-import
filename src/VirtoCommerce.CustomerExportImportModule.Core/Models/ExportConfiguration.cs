using System.Globalization;
using CsvHelper.Configuration;

namespace VirtoCommerce.CustomerExportImportModule.Core.Models
{
    public sealed record ExportConfiguration : CsvConfiguration
    {
        public ExportConfiguration() : base(CultureInfo.InvariantCulture)
        {
            Delimiter = ";";
        }
    }
}
