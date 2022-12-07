using System.Globalization;
using CsvHelper.Configuration;

namespace VirtoCommerce.CustomerExportImportModule.Core.Models
{
    public sealed record ImportConfiguration : CsvConfiguration
    {
        public ImportConfiguration() : base(CultureInfo.InvariantCulture)
        {
            Delimiter = ";";
            ReadingExceptionOccurred = _ => false;
            BadDataFound = null;
            MissingFieldFound = null;
        }
    }
}
