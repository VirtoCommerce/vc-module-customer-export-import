using System.Linq;
using FluentValidation;
using VirtoCommerce.CustomerExportImportModule.Core;
using VirtoCommerce.CustomerExportImportModule.Core.Models;

namespace VirtoCommerce.CustomerExportImportModule.Data.Validation
{
    public class ImportContactValidator: AbstractValidator<ImportRecord<CsvContact>>
    {
        public ImportContactValidator()
        {
            AttachValidators();
        }

        private void AttachValidators()
        {
            RuleFor(x => x.Record.ContactFirstName).NotEmpty().WithErrorCode(ModuleConstants.ValidationErrors.MissingRequiredColumns);
            RuleFor(x => x.Record.ContactFirstName).MaximumLength(128).WithErrorCode(ModuleConstants.ValidationErrors.ExceedingMaxLength);

            RuleFor(x => x.Record.ContactLastName).NotEmpty().WithErrorCode(ModuleConstants.ValidationErrors.MissingRequiredColumns);
            RuleFor(x => x.Record.ContactLastName).MaximumLength(128).WithErrorCode(ModuleConstants.ValidationErrors.ExceedingMaxLength);

            RuleFor(x => x.Record.ContactFullName).NotEmpty().WithErrorCode(ModuleConstants.ValidationErrors.MissingRequiredColumns);
            RuleFor(x => x.Record.ContactFullName).MaximumLength(254).WithErrorCode(ModuleConstants.ValidationErrors.ExceedingMaxLength);

            RuleFor(x => x.Record.OrganizationOuterId).MaximumLength(128).WithErrorCode(ModuleConstants.ValidationErrors.ExceedingMaxLength);
            RuleFor(x => x.Record.OrganizationName).MaximumLength(128).WithErrorCode(ModuleConstants.ValidationErrors.ExceedingMaxLength);

            RuleFor(x => x.Record.ContactStatus).MaximumLength(64).WithErrorCode(ModuleConstants.ValidationErrors.ExceedingMaxLength);
            RuleFor(x => x.Record.TimeZone).MaximumLength(32).WithErrorCode(ModuleConstants.ValidationErrors.ExceedingMaxLength);
            RuleFor(x => x.Record.Salutation).MaximumLength(256).WithErrorCode(ModuleConstants.ValidationErrors.ExceedingMaxLength);
            RuleFor(x => x.Record.DefaultLanguage).MaximumLength(32).WithErrorCode(ModuleConstants.ValidationErrors.ExceedingMaxLength);
            RuleFor(x => x.Record.TaxPayerId).MaximumLength(64).WithErrorCode(ModuleConstants.ValidationErrors.ExceedingMaxLength);
            RuleFor(x => x.Record.PreferredDelivery).MaximumLength(64).WithErrorCode(ModuleConstants.ValidationErrors.ExceedingMaxLength);
            RuleFor(x => x.Record.PreferredCommunication).MaximumLength(64).WithErrorCode(ModuleConstants.ValidationErrors.ExceedingMaxLength);

            When(x => new[]
                {
                    x.Record.AccountId, x.Record.AccountType, x.Record.AccountStatus, x.Record.AccountLogin, x.Record.AccountEmail, x.Record.StoreId, x.Record.StoreName,
                    x.Record.EmailVerified.ToString()
                }.Any(field => !string.IsNullOrEmpty(field))
                , () =>
                {

                    RuleFor(x => x.Record.AccountLogin).NotEmpty().WithErrorCode(ModuleConstants.ValidationErrors.MissingRequiredColumns); ;
                    RuleFor(x => x.Record.AccountEmail).NotEmpty().WithErrorCode(ModuleConstants.ValidationErrors.MissingRequiredColumns); ;
                    RuleFor(x => x.Record.StoreId).NotEmpty().WithErrorCode(ModuleConstants.ValidationErrors.MissingRequiredColumns); ;
                });
        }
    }
}
