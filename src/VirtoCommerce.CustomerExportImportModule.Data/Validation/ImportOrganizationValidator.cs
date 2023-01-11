using FluentValidation;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerExportImportModule.Data.Helpers;

namespace VirtoCommerce.CustomerExportImportModule.Data.Validation
{
    public class ImportOrganizationValidator: AbstractValidator<ImportRecord<ImportableOrganization>>
    {
        public ImportOrganizationValidator()
        {
            AttachValidators();
        }

        private void AttachValidators()
        {
            RuleFor(x => x.Record.Id)
                .MaximumLength(128)
                .WithExceededMaxLengthCodeAndMessage("Organization Id", 128)
                .WithImportState();

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

            RuleFor(x => x.Record.ParentOrganizationId)
                .MaximumLength(128)
                .WithExceededMaxLengthCodeAndMessage("Parent Organization Id", 128)
                .WithImportState();

            RuleFor(x => x.Record.ParentOrganizationOuterId)
                .MaximumLength(128)
                .WithExceededMaxLengthCodeAndMessage("Parent Organization Outer Id", 128)
                .WithImportState();

            RuleFor(x => x.Record.ParentOrganizationName)
                .MaximumLength(128)
                .WithExceededMaxLengthCodeAndMessage("Parent Organization Name", 128)
                .WithImportState();

            RuleFor(x => x.Record.BusinessCategory)
                .MaximumLength(64)
                .WithExceededMaxLengthCodeAndMessage("Business Category", 64)
                .WithImportState();

            RuleFor(x => x.Record.OrganizationGroups)
                .MaximumLength(64)
                .WithExceededMaxLengthCodeAndMessage("Organization Groups", 64)
                .WithImportState();
        }
    }
}
