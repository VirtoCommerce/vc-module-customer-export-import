using System.Linq;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerExportImportModule.Data.Helpers;
using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.CustomerExportImportModule.Data.Validation
{
    public class ImportContactValidator: AbstractValidator<ImportRecord<ImportableContact>>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ImportContactValidator(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            AttachValidators();
        }

        private void AttachValidators()
        {
            RuleFor(x => x.Record.OuterId)
                .MaximumLength(128)
                .WithExceededMaxLengthCodeAndMessage("Contact Outer Id", 128)
                .WithImportState();

            RuleFor(x => x.Record.ContactFirstName)
                .NotEmpty()
                .WithMissingRequiredValueCodeAndMessage("Contact First Name")
                .WithImportState()
                .DependentRules(() =>
                {
                    RuleFor(x => x.Record.ContactFirstName)
                        .MaximumLength(128)
                        .WithExceededMaxLengthCodeAndMessage("Contact First Name", 128)
                        .WithImportState();
                });

            RuleFor(x => x.Record.ContactLastName)
                .NotEmpty()
                .WithMissingRequiredValueCodeAndMessage("Contact Last Name")
                .WithImportState()
                .DependentRules(() =>
                {
                    RuleFor(x => x.Record.ContactLastName)
                        .MaximumLength(128)
                        .WithExceededMaxLengthCodeAndMessage("Contact Last Name", 128)
                        .WithImportState();
                });

            RuleFor(x => x.Record.ContactFullName)
                .NotEmpty()
                .WithMissingRequiredValueCodeAndMessage("Contact Full Name")
                .WithImportState()
                .DependentRules(() =>
                {
                    RuleFor(x => x.Record.ContactFullName)
                        .MaximumLength(254)
                        .WithExceededMaxLengthCodeAndMessage("Contact Full Name", 254)
                        .WithImportState();
                });

            RuleFor(x => x.Record.OrganizationOuterId)
                .MaximumLength(128)
                .WithExceededMaxLengthCodeAndMessage("Organization Outer Id", 128)
                .WithImportState();
            RuleFor(x => x.Record.OrganizationName)
                .MaximumLength(128)
                .WithExceededMaxLengthCodeAndMessage("Organization Name", 128)
                .WithImportState();

            RuleFor(x => x.Record.ContactStatus)
                .MaximumLength(64)
                .WithExceededMaxLengthCodeAndMessage("Contact Status", 64)
                .WithImportState();
            RuleFor(x => x.Record.TimeZone)
                .MaximumLength(32)
                .WithExceededMaxLengthCodeAndMessage("Time Zone", 32)
                .WithImportState();
            RuleFor(x => x.Record.Salutation)
                .MaximumLength(256)
                .WithExceededMaxLengthCodeAndMessage("Salutation", 256)
                .WithImportState();
            RuleFor(x => x.Record.DefaultLanguage)
                .MaximumLength(32)
                .WithExceededMaxLengthCodeAndMessage("Default Language", 32)
                .WithImportState();
            RuleFor(x => x.Record.TaxPayerId)
                .MaximumLength(64)
                .WithExceededMaxLengthCodeAndMessage("Tax Payer Id", 64)
                .WithImportState();
            RuleFor(x => x.Record.PreferredDelivery)
                .MaximumLength(64)
                .WithExceededMaxLengthCodeAndMessage("Preferred Delivery", 64)
                .WithImportState();
            RuleFor(x => x.Record.PreferredCommunication)
                .MaximumLength(64)
                .WithExceededMaxLengthCodeAndMessage("Preferred Communication", 64)
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
                        .WithImportState()
                        .DependentRules(() =>
                        {
                            RuleFor(x => x.Record.AccountLogin)
                                .MustAsync(async (_, userName, __) => await _userManager.FindByNameAsync(userName) == null)
                                .WithNotUniqueValueCodeAndMessage("Account Login")
                                .WithImportState();
                        });
                    RuleFor(x => x.Record.AccountEmail)
                        .NotEmpty()
                        .WithMissingRequiredValueCodeAndMessage("Account Email")
                        .WithImportState()
                        .DependentRules(() =>
                        {
                            RuleFor(x => x.Record.AccountEmail)
                                .MustAsync(async (_, email, __) => await _userManager.FindByEmailAsync(email) == null)
                                .WithNotUniqueValueCodeAndMessage("Account Email")
                                .WithImportState();
                        });
                    RuleFor(x => x.Record.StoreId)
                        .NotEmpty()
                        .WithMissingRequiredValueCodeAndMessage("Store Id")
                        .WithImportState();
                });
        }
    }
}
