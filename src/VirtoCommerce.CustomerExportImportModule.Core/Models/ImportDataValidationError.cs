using System.Collections.Generic;

namespace VirtoCommerce.CustomerExportImportModule.Core.Models
{
    public sealed class ImportDataValidationError
    {
        public ImportDataValidationError()
        {
            Properties = new Dictionary<string, string>();
        }

        public string ErrorCode { get; set; }

        public Dictionary<string, string> Properties { get; set; }
    }
}
