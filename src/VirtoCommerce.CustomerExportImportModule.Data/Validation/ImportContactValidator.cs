using System.Linq;
using FluentValidation;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerExportImportModule.Data.Helpers;

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
            RuleFor(x => x.Record.OuterId)
                .MaximumLength(128)
                .WithExceededMaxLengthCodeAndMessage("Contact Outer Id")
                .WithImportState();

            RuleFor(x => x.Record.ContactFirstName)
                .NotEmpty()
                .WithMissingRequiredValueCodeAndMessage("Contact First Name")
                .WithImportState();
            RuleFor(x => x.Record.ContactFirstName)
                .MaximumLength(128)
                .WithExceededMaxLengthCodeAndMessage("Contact First Name")
                .WithImportState();

            RuleFor(x => x.Record.ContactLastName)
                .NotEmpty()
                .WithMissingRequiredValueCodeAndMessage("Contact Last Name")
                .WithImportState();
            RuleFor(x => x.Record.ContactLastName)
                .MaximumLength(128)
                .WithExceededMaxLengthCodeAndMessage("Contact Last Name")
                .WithImportState();

            RuleFor(x => x.Record.ContactFullName)
                .NotEmpty()
                .WithMissingRequiredValueCodeAndMessage("Contact Full Name")
                .WithImportState();
            RuleFor(x => x.Record.ContactFullName)
                .MaximumLength(254)
                .WithExceededMaxLengthCodeAndMessage("Contact Full Name")
                .WithImportState();

            RuleFor(x => x.Record.OrganizationOuterId)
                .MaximumLength(128)
                .WithExceededMaxLengthCodeAndMessage("Organization Outer Id")
                .WithImportState();
            RuleFor(x => x.Record.OrganizationName)
                .MaximumLength(128)
                .WithExceededMaxLengthCodeAndMessage("Organization Name")
                .WithImportState();

            RuleFor(x => x.Record.ContactStatus)
                .MaximumLength(64)
                .WithExceededMaxLengthCodeAndMessage("Contact Status")
                .WithImportState();
            RuleFor(x => x.Record.TimeZone)
                .MaximumLength(32)
                .WithExceededMaxLengthCodeAndMessage("Time Zone")
                .WithImportState();
            RuleFor(x => x.Record.Salutation)
                .MaximumLength(256)
                .WithExceededMaxLengthCodeAndMessage("Salutation")
                .WithImportState();
            RuleFor(x => x.Record.DefaultLanguage)
                .MaximumLength(32)
                .WithExceededMaxLengthCodeAndMessage("Default Language")
                .WithImportState();
            RuleFor(x => x.Record.TaxPayerId)
                .MaximumLength(64)
                .WithExceededMaxLengthCodeAndMessage("Tax Payer Id")
                .WithImportState();
            RuleFor(x => x.Record.PreferredDelivery)
                .MaximumLength(64)
                .WithExceededMaxLengthCodeAndMessage("Preferred Delivery")
                .WithImportState();
            RuleFor(x => x.Record.PreferredCommunication)
                .MaximumLength(64)
                .WithExceededMaxLengthCodeAndMessage("Preferred Communication")
                .WithImportState();

            When(x => new[]
                {
                    x.Record.AccountId, x.Record.AccountType, x.Record.AccountStatus, x.Record.AccountLogin, x.Record.AccountEmail, x.Record.StoreId, x.Record.StoreName,
                    x.Record.EmailVerified.ToString()
                }.Any(field => !string.IsNullOrEmpty(field))
                , () =>
                {

                    RuleFor(x => x.Record.AccountLogin)
                        .NotEmpty()
                        .WithMissingRequiredValueCodeAndMessage("Account Login")
                        .WithImportState();
                    RuleFor(x => x.Record.AccountEmail)
                        .NotEmpty()
                        .WithMissingRequiredValueCodeAndMessage("Account Email")
                        .WithImportState();
                    RuleFor(x => x.Record.StoreId)
                        .NotEmpty()
                        .WithMissingRequiredValueCodeAndMessage("Store Id")
                        .WithImportState();
                });
        }
    }
}
