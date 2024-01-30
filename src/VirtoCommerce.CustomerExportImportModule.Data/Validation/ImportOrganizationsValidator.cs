using FluentValidation;
using VirtoCommerce.CustomerExportImportModule.Core.Models;

namespace VirtoCommerce.CustomerExportImportModule.Data.Validation
{
    public sealed class ImportOrganizationsValidator : AbstractValidator<ImportRecord<ImportableOrganization>[]>
    {
        private readonly IImportMemberValidator<ImportableOrganization> _memberValidator;

        public ImportOrganizationsValidator(IImportMemberValidator<ImportableOrganization> memberValidator)
        {
            _memberValidator = memberValidator;
            AttachValidators();
        }

        private void AttachValidators()
        {
            RuleFor(importRecords => importRecords).SetValidator(_ => new ImportEntitiesAreNotDuplicatesValidator<ImportableOrganization>());
            RuleFor(importRecords => importRecords).SetValidator(_ => new ImportEntitiesAdditionalLinesValidator<ImportableOrganization>());
            RuleForEach(importRecords => importRecords).SetValidator(_memberValidator);
            RuleForEach(importRecords => importRecords).SetValidator(new ImportOrganizationValidator());
        }
    }
}
