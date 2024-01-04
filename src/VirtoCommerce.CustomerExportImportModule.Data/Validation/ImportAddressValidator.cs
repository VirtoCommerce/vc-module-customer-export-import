using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerExportImportModule.Data.Helpers;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.CustomerExportImportModule.Core;


namespace VirtoCommerce.CustomerExportImportModule.Data.Validation
{
    public class ImportAddressValidator<T> : AbstractValidator<ImportRecord<T>>
        where T : CsvMember
    {
        private readonly ICountriesService _countriesService;
        private readonly ISettingsManager _settingsManager;
        internal const string Countries = nameof(Countries);

        public ImportAddressValidator(ICountriesService countriesService, ISettingsManager settingsManager)
        {
            _countriesService = countriesService;
            _settingsManager = settingsManager;
            AttachValidators();
        }

        private void AttachValidators()
        {
            When(x => new[]
                {
                    x.Record.AddressType, x.Record.AddressLine1, x.Record.AddressLine2, x.Record.AddressCity, x.Record.AddressRegion, x.Record.AddressRegionCode, x.Record.AddressCountryCode, x.Record.AddressCountry,
                    x.Record.AddressFirstName, x.Record.AddressLastName, x.Record.AddressPhone, x.Record.AddressEmail, x.Record.AddressZipCode,
                }.Any(field => !string.IsNullOrEmpty(field))
                , () =>
                {
                    RuleFor(x => x.Record.AddressType)
                        .IsEnumName(typeof(AddressType))
                        .When(x => !string.IsNullOrEmpty(x.Record.AddressType))
                        .WithInvalidValueCodeAndMessage("Address Type")
                        .WithImportState();

                    RuleFor(x => x.Record.AddressFirstName).Cascade(CascadeMode.Stop)
                        .MaximumLength(128)
                        .WithExceededMaxLengthCodeAndMessage("Address First Name", 128)
                        .WithImportState();

                    RuleFor(x => x.Record.AddressLastName).Cascade(CascadeMode.Stop)
                        .MaximumLength(128)
                        .WithExceededMaxLengthCodeAndMessage("Address Last Name", 128)
                        .WithImportState();

                    RuleFor(x => x.Record.AddressEmail)
                        .EmailAddress()
                        .When(x => !string.IsNullOrEmpty(x.Record.AddressEmail))
                        .WithInvalidValueCodeAndMessage("Address Email")
                        .WithImportState();

                    RuleFor(x => x.Record.AddressEmail)
                        .MaximumLength(64)
                        .WithExceededMaxLengthCodeAndMessage("Address Email", 64)
                        .WithImportState();

                    RuleFor(x => x.Record.AddressPhone)
                        .MaximumLength(256)
                        .WithExceededMaxLengthCodeAndMessage("Address Phone", 256)
                        .WithImportState();

                    RuleFor(x => x.Record.AddressLine1)
                        .NotEmpty()
                        .WithMissingRequiredValueCodeAndMessage("Address Line1")
                        .WithImportState()
                        .DependentRules(() =>
                        {
                            RuleFor(x => x.Record.AddressLine1)
                                .MaximumLength(128)
                                .WithExceededMaxLengthCodeAndMessage("Address Line1", 128)
                                .WithImportState();
                        });

                    RuleFor(x => x.Record.AddressLine2)
                        .MaximumLength(128)
                        .WithExceededMaxLengthCodeAndMessage("Address Line2", 128)
                        .WithImportState();

                    RuleFor(x => x.Record.AddressCity)
                        .NotEmpty()
                        .WithMissingRequiredValueCodeAndMessage("Address City")
                        .WithImportState()
                        .DependentRules(() =>
                        {
                            RuleFor(x => x.Record.AddressCity)
                                .MaximumLength(128)
                                .WithExceededMaxLengthCodeAndMessage("Address City", 128)
                                .WithImportState();
                        });

                    RuleFor(x => x.Record.AddressRegion)                        
                        .MaximumLength(128)
                        .WithExceededMaxLengthCodeAndMessage("Address Region", 128)
                        .WithImportState()
                        .MustAsync(async (importRecord, region, _, _) =>
                        {
                            var regionValidation = _settingsManager.GetValue<bool>(ModuleConstants.Settings.General.AddressRegionValidation);

                            if (!regionValidation)
                            {
                                return true;
                            }

                            var regions = await _countriesService.GetCountryRegionsAsync(importRecord.Record.AddressCountryCode);
                            var regionResult = regions.FirstOrDefault(r => r.Name.ToLower().Trim() == region.ToLower().Trim());

                            return regionResult is not null;
                        })
                        .WithInvalidValueCodeAndMessage("Address Region");

                    RuleFor(x => x.Record.AddressRegionCode)
                        .MaximumLength(128)
                        .WithExceededMaxLengthCodeAndMessage("Address Region Code", 128)
                        .WithImportState()
                        .MustAsync((importRecord, regionCode, _, _) =>
                        {
                            var regionValidation = _settingsManager.GetValue<bool>(ModuleConstants.Settings.General.AddressRegionValidation);

                            if (!regionValidation)
                            {
                                return System.Threading.Tasks.Task.FromResult(true);
                            }

                            return System.Threading.Tasks.Task.FromResult(!string.IsNullOrEmpty(regionCode));
                        })
                        .WithMissingRequiredValueCodeAndMessage("Address Region code");
                    ;

                    RuleFor(x => x.Record.AddressCountry)
                        .MaximumLength(128)
                        .WithExceededMaxLengthCodeAndMessage("Address Country", 128)
                        .WithImportState();

                    RuleFor(x => x.Record.AddressCountryCode)
                        .NotEmpty()
                        .WithMissingRequiredValueCodeAndMessage("Address Country Code")
                        .WithImportState()
                        .DependentRules(() =>
                        {
                            RuleFor(x => x.Record.AddressCountryCode)
                                .MaximumLength(64)
                                .WithExceededMaxLengthCodeAndMessage("Address Country Code", 64)
                                .WithImportState();
                            RuleFor(x => x.Record.AddressCountryCode)
                                .Must((_, countryCode, context) =>
                                {
                                    var countries = (IList<Country>)context.RootContextData[Countries];
                                    return countries.Any(country => country.Id == countryCode);
                                })
                                .WithInvalidValueCodeAndMessage("Address Country Code")
                                .WithImportState()
                                .DependentRules(() =>
                                {
                                    RuleFor(x => x.Record.AddressRegionCode)
                                        .MustAsync(async (importRecord, regionCode, _, _) =>
                                        {
                                            if (string.IsNullOrEmpty(regionCode))
                                            {
                                                return true;
                                            }

                                            var regions = await _countriesService.GetCountryRegionsAsync(importRecord.Record.AddressCountryCode);
                                            return !regions.Any() || regions.Any(region => region.Id == regionCode);
                                        })
                                        .WithInvalidValueCodeAndMessage("Address Region Code")
                                        .WithImportState();
                                });
                        });

                    RuleFor(x => x.Record.AddressZipCode)
                        .NotEmpty()
                        .WithMissingRequiredValueCodeAndMessage("Address Zip Code")
                        .WithImportState()
                        .DependentRules(() =>
                        {
                            RuleFor(x => x.Record.AddressZipCode)
                                .MaximumLength(32)
                                .WithExceededMaxLengthCodeAndMessage("Address Zip Code", 32)
                                .WithImportState();
                        });
                });
        }
    }
}
