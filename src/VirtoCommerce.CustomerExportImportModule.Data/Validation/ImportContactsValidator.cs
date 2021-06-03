using FluentValidation;
using VirtoCommerce.CustomerExportImportModule.Core.Models;

namespace VirtoCommerce.CustomerExportImportModule.Data.Validation
{
    public sealed class ImportContactsValidator: AbstractValidator<ImportRecord<CsvContact>[]>
    {
        public ImportContactsValidator()
        {
            AttachValidators();
        }

        private void AttachValidators()
        {
            RuleFor(importRecords => importRecords).SetValidator(_ => new ImportEntitiesAreNotDuplicatesValidator<CsvContact>());
            RuleForEach(importRecords => importRecords).SetValidator(new ImportMemberValidator<CsvContact>());
            RuleForEach(importRecords => importRecords).SetValidator(new ImportContactValidator());
        }
    }
}
