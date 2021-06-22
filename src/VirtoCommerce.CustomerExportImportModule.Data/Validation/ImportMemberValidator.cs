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
            When(x => new[]
                {
                    x.Record.AddressType, x.Record.AddressLine1, x.Record.AddressLine2, x.Record.AddressCity, x.Record.AddressRegion, x.Record.AddressCountryCode, x.Record.AddressCountry,
                    x.Record.AddressFirstName, x.Record.AddressLastName, x.Record.AddressPhone, x.Record.AddressEmail, x.Record.AddressZipCode
                }.Any(field => !string.IsNullOrEmpty(field))
                , () =>
                {
                    RuleFor(x => x.Record.AddressType).IsEnumName(typeof(AddressType))
                        .WithInvalidValueCodeAndMessage("Address Type")
                        .WithImportState();

                    RuleFor(x => x.Record.AddressFirstName)
                        .MaximumLength(128)
                        .WithExceededMaxLengthCodeAndMessage("Address First Name")
                        .WithImportState();
                    RuleFor(x => x.Record.AddressLastName)
                        .MaximumLength(128)
                        .WithExceededMaxLengthCodeAndMessage("Address Last Name")
                        .WithImportState();

                    RuleFor(x => x.Record.AddressEmail)
                        .MaximumLength(64)
                        .WithExceededMaxLengthCodeAndMessage("Address Email")
                        .WithImportState();
                    RuleFor(x => x.Record.AddressPhone)
                        .MaximumLength(256)
                        .WithExceededMaxLengthCodeAndMessage("Address Phone")
                        .WithImportState();

                    RuleFor(x => x.Record.AddressLine1)
                        .NotEmpty()
                        .WithMissingRequiredValueCodeAndMessage("Address Line1")
                        .WithImportState();
                    RuleFor(x => x.Record.AddressLine1)
                        .MaximumLength(128)
                        .WithExceededMaxLengthCodeAndMessage("Address Line1")
                        .WithImportState();
                    RuleFor(x => x.Record.AddressLine2)
                        .MaximumLength(128)
                        .WithExceededMaxLengthCodeAndMessage("Address Line2")
                        .WithImportState();

                    RuleFor(x => x.Record.AddressCity)
                        .NotEmpty()
                        .WithMissingRequiredValueCodeAndMessage("Address City")
                        .WithImportState();
                    RuleFor(x => x.Record.AddressCity)
                        .MaximumLength(128)
                        .WithExceededMaxLengthCodeAndMessage("Address City")
                        .WithImportState();

                    RuleFor(x => x.Record.AddressRegion)
                        .MaximumLength(128)
                        .WithExceededMaxLengthCodeAndMessage("Address Region")
                        .WithImportState();

                    RuleFor(x => x.Record.AddressCountryCode)
                        .NotEmpty()
                        .WithMissingRequiredValueCodeAndMessage("Address Country Code")
                        .WithImportState();
                    RuleFor(x => x.Record.AddressCountryCode)
                        .MaximumLength(64)
                        .WithExceededMaxLengthCodeAndMessage("Address Country Code")
                        .WithImportState();

                    RuleFor(x => x.Record.AddressCountry)
                        .NotEmpty()
                        .WithMissingRequiredValueCodeAndMessage("Address Country")
                        .WithImportState();
                    RuleFor(x => x.Record.AddressCountry)
                        .MaximumLength(128)
                        .WithExceededMaxLengthCodeAndMessage("Address Country")
                        .WithImportState();

                    RuleFor(x => x.Record.AddressZipCode)
                        .NotEmpty()
                        .WithMissingRequiredValueCodeAndMessage("Address Zip Code")
                        .WithImportState();
                    RuleFor(x => x.Record.AddressZipCode)
                        .MaximumLength(32)
                        .WithExceededMaxLengthCodeAndMessage("Address Zip Code")
                        .WithImportState();
                });

            RuleFor(x => x.Record.DynamicProperties).SetValidator(new ImportDynamicPropertiesValidator()).WithImportState();
        }
    }
}
