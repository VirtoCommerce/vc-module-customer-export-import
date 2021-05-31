using System;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace VirtoCommerce.CustomerExportImportModule.Core.Models
{
    public sealed class ImportConfiguration : Configuration
    {
        public ImportConfiguration()
            : base(CultureInfo.InvariantCulture)
        {
        }

        public override string Delimiter { get; set; } = ";";

        public override Func<CsvHelperException, bool> ReadingExceptionOccurred { get; set; } = ex => false;

        public override Action<ReadingContext> BadDataFound { get; set; } = null;

        public override Action<string[], int, ReadingContext> MissingFieldFound { get; set; } = null;
    }
}
