using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.CustomerExportImportModule.Core;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerExportImportModule.Data.Helpers;
using VirtoCommerce.CustomerExportImportModule.Data.Models;

namespace VirtoCommerce.CustomerExportImportModule.Data.Validation
{
    public class ImportAddressValidator<T> : AbstractValidator<ImportRecord<T>>
        where T : CsvMember
    {
        private readonly Country[] _countries;

        public ImportAddressValidator(Country[] countries)
        {
            _countries = countries;
            AttachValidators();
        }

        private void AttachValidators()
        {
            var countryCodes = _countries.Select(country => country.Id);
            var countryNames = _countries.Select(country => country.Name);

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

                    RuleFor(x => x.Record.AddressCountryCode).Must(countryCode => countryCodes.Contains(countryCode)).WithErrorCode(ModuleConstants.ValidationErrors.InvalidValue).WithImportState();
                    RuleFor(x => x.Record.AddressCountryCode).NotEmpty().WithErrorCode(ModuleConstants.ValidationErrors.MissingRequiredColumns).WithImportState();
                    RuleFor(x => x.Record.AddressCountryCode).MaximumLength(64).WithErrorCode(ModuleConstants.ValidationErrors.ExceedingMaxLength).WithImportState();

                    RuleFor(x => x.Record.AddressCountry).Must(countryName => countryNames.Contains(countryName)).WithErrorCode(ModuleConstants.ValidationErrors.InvalidValue).WithImportState();
                    RuleFor(x => x.Record.AddressCountry).NotEmpty().WithErrorCode(ModuleConstants.ValidationErrors.MissingRequiredColumns).WithImportState();
                    RuleFor(x => x.Record.AddressCountry).MaximumLength(128).WithErrorCode(ModuleConstants.ValidationErrors.ExceedingMaxLength).WithImportState();

                    RuleFor(x => x.Record.AddressZipCode).NotEmpty().WithErrorCode(ModuleConstants.ValidationErrors.MissingRequiredColumns).WithImportState();
                    RuleFor(x => x.Record.AddressZipCode).MaximumLength(32).WithErrorCode(ModuleConstants.ValidationErrors.ExceedingMaxLength).WithImportState();
                });
        }
    }
}
