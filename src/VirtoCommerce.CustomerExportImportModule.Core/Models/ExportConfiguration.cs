using System.Globalization;
using CsvHelper.Configuration;

namespace VirtoCommerce.CustomerExportImportModule.Core.Models
{
    public sealed class ExportConfiguration : Configuration
    {
        public ExportConfiguration()
            : base(CultureInfo.InvariantCulture)
        {
        }

        public override string Delimiter { get; set; } = ";";

    }
}
