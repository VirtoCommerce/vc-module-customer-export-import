using FluentValidation;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerExportImportModule.Data.Helpers;

namespace VirtoCommerce.CustomerExportImportModule.Data.Validation
{
    public class ImportOrganizationValidator: AbstractValidator<ImportRecord<CsvOrganization>>
    {
        public ImportOrganizationValidator()
        {
            AttachValidators();
        }

        private void AttachValidators()
        {
            RuleFor(x => x.Record.OuterId)
                .MaximumLength(128)
                .WithExceededMaxLengthCodeAndMessage("Organization Outer Id", 128)
                .WithImportState();

            RuleFor(x => x.Record.OrganizationName)
                .NotEmpty()
                .WithMissingRequiredValueCodeAndMessage("Organization Name")
                .WithImportState()
                .DependentRules(() =>
                {
                    RuleFor(x => x.Record.OrganizationName)
                        .MaximumLength(128)
                        .WithExceededMaxLengthCodeAndMessage("Organization Name", 128)
                        .WithImportState();
                });
            
            RuleFor(x => x.Record.BusinessCategory)
                .MaximumLength(128)
                .WithExceededMaxLengthCodeAndMessage("Business Category", 128)
                .WithImportState();
        }
    }
}
