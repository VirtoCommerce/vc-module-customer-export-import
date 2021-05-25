using System;

namespace VirtoCommerce.CustomerExportImportModule.Core.Models
{
    public sealed class ImportDataValidationResult
    {
        public ImportDataValidationResult()
        {
            Errors = Array.Empty<ImportDataValidationError>();
        }

        public ImportDataValidationError[] Errors { get; set; }
    }
}
