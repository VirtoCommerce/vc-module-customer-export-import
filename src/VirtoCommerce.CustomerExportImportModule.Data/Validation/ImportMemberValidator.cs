using System.Linq;
using FluentValidation;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.CustomerExportImportModule.Core;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerExportImportModule.Data.Helpers;

namespace VirtoCommerce.CustomerExportImportModule.Data.Validation
{
    public class ImportMemberValidator<T>: AbstractValidator<ImportRecord<T>>
        where T: CsvMember
    {
        public ImportMemberValidator()
        {
            AttachValidators();
        }

        private void AttachValidators()
        {
            RuleFor(x => x.Record.OuterId).MaximumLength(128).WithErrorCode(ModuleConstants.ValidationErrors.ExceedingMaxLength).WithImportState();

            When(x => new[]
                {
                    x.Record.AddressType, x.Record.AddressLine1, x.Record.AddressLine2, x.Record.AddressCity, x.Record.AddressRegion, x.Record.AddressCountryCode, x.Record.AddressCountry,
                    x.Record.AddressFirstName, x.Record.AddressLastName, x.Record.AddressPhone, x.Record.AddressEmail, x.Record.AddressZipCode
                }.Any(field => !string.IsNullOrEmpty(field))
                , () =>
                {
                    RuleFor(x => x.Record.AddressType).IsEnumName(typeof(AddressType)).WithErrorCode(ModuleConstants.ValidationErrors.InvalidValue).WithImportState();

                    RuleFor(x => x.Record.AddressFirstName).MaximumLength(128).WithErrorCode(ModuleConstants.ValidationErrors.ExceedingMaxLength).WithImportState();
                    RuleFor(x => x.Record.AddressLastName).MaximumLength(128).WithErrorCode(ModuleConstants.ValidationErrors.ExceedingMaxLength).WithImportState();

                    RuleFor(x => x.Record.AddressEmail).MaximumLength(64).WithErrorCode(ModuleConstants.ValidationErrors.ExceedingMaxLength).WithImportState();
                    RuleFor(x => x.Record.AddressPhone).MaximumLength(256).WithErrorCode(ModuleConstants.ValidationErrors.ExceedingMaxLength).WithImportState();

                    RuleFor(x => x.Record.AddressLine1).NotEmpty().WithErrorCode(ModuleConstants.ValidationErrors.MissingRequiredColumns).WithImportState();
                    RuleFor(x => x.Record.AddressLine1).MaximumLength(128).WithErrorCode(ModuleConstants.ValidationErrors.ExceedingMaxLength).WithImportState();
                    RuleFor(x => x.Record.AddressLine2).MaximumLength(128).WithErrorCode(ModuleConstants.ValidationErrors.ExceedingMaxLength).WithImportState();

                    RuleFor(x => x.Record.AddressCity).NotEmpty().WithErrorCode(ModuleConstants.ValidationErrors.MissingRequiredColumns).WithImportState();
                    RuleFor(x => x.Record.AddressCity).MaximumLength(128).WithErrorCode(ModuleConstants.ValidationErrors.ExceedingMaxLength).WithImportState();

                    RuleFor(x => x.Record.AddressRegion).MaximumLength(128).WithErrorCode(ModuleConstants.ValidationErrors.ExceedingMaxLength).WithImportState();

                    RuleFor(x => x.Record.AddressCountryCode).NotEmpty().WithErrorCode(ModuleConstants.ValidationErrors.MissingRequiredColumns).WithImportState();
                    RuleFor(x => x.Record.AddressCountryCode).MaximumLength(64).WithErrorCode(ModuleConstants.ValidationErrors.ExceedingMaxLength).WithImportState();

                    RuleFor(x => x.Record.AddressCountry).NotEmpty().WithErrorCode(ModuleConstants.ValidationErrors.MissingRequiredColumns).WithImportState();
                    RuleFor(x => x.Record.AddressCountry).MaximumLength(128).WithErrorCode(ModuleConstants.ValidationErrors.ExceedingMaxLength).WithImportState();

                    RuleFor(x => x.Record.AddressZipCode).NotEmpty().WithErrorCode(ModuleConstants.ValidationErrors.MissingRequiredColumns).WithImportState();
                    RuleFor(x => x.Record.AddressZipCode).MaximumLength(32).WithErrorCode(ModuleConstants.ValidationErrors.ExceedingMaxLength).WithImportState();
                });

            RuleFor(x => x.Record.DynamicProperties).SetValidator(new ImportDynamicPropertiesValidator()).WithImportState();
        }
    }
}
