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
    }
}
