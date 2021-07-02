using FluentValidation;
using Microsoft.AspNetCore.Identity;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerExportImportModule.Data.Helpers;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.Services;

namespace VirtoCommerce.CustomerExportImportModule.Data.Validation
{
    public class ImportContactValidator: AbstractValidator<ImportRecord<ImportableContact>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IStoreSearchService _storeSearchService;
        private readonly ISettingsManager _settingsManager;

        public ImportContactValidator(UserManager<ApplicationUser> userManager, IStoreSearchService storeSearchService, ISettingsManager settingsManager)
        {
            _userManager = userManager;
            _storeSearchService = storeSearchService;
            _settingsManager = settingsManager;
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

            RuleFor(x => x).SetValidator(_ => new ImportAccountValidator(_userManager, _storeSearchService, _settingsManager));
        }
    }
}
