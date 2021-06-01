using FluentValidation;
using VirtoCommerce.CustomerExportImportModule.Core.Models;

namespace VirtoCommerce.CustomerExportImportModule.Data.Validation
{
    public sealed class ImportContactValidator: AbstractValidator<ImportRecord<CsvContact>[]>
    {
        public ImportContactValidator()
        {
            AttachValidators();
        }

        private void AttachValidators()
        {
            RuleFor(importRecords => importRecords).SetValidator(_ => new ImportEntitiesAreNotDuplicatesValidator<CsvContact>());
        }
    }
}
