using FluentValidation;
using VirtoCommerce.CustomerExportImportModule.Core.Models;

namespace VirtoCommerce.CustomerExportImportModule.Data.Validation
{
    public sealed class ImportOrganizationsValidator : AbstractValidator<ImportRecord<CsvOrganization>[]>
    {
        public ImportOrganizationsValidator()
        {
            AttachValidators();
        }

        private void AttachValidators()
        {
            RuleFor(importRecords => importRecords).SetValidator(_ => new ImportEntitiesAreNotDuplicatesValidator<CsvOrganization>());
            RuleForEach(importRecords => importRecords).SetValidator(new ImportMemberValidator<CsvOrganization>());
            RuleForEach(importRecords => importRecords).SetValidator(new ImportOrganizationValidator());
        }
    }
}
