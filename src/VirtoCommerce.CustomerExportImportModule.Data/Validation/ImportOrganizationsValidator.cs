using FluentValidation;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.CustomerExportImportModule.Data.Validation
{
    public sealed class ImportOrganizationsValidator : AbstractValidator<ImportRecord<ImportableOrganization>[]>
    {
        private readonly ICountriesService _countriesService;
        private readonly IDynamicPropertyDictionaryItemsSearchService _dynamicPropertyDictionaryItemsSearchService;
        private readonly ISettingsManager _settingsManager;

        public ImportOrganizationsValidator(ICountriesService countriesService, IDynamicPropertyDictionaryItemsSearchService dynamicPropertyDictionaryItemsSearchService, ISettingsManager settingsManager)
        {
            _countriesService = countriesService;
            _dynamicPropertyDictionaryItemsSearchService = dynamicPropertyDictionaryItemsSearchService;
            _settingsManager = settingsManager;
            AttachValidators();
        }

        private void AttachValidators()
        {
            RuleFor(importRecords => importRecords).SetValidator(_ => new ImportEntitiesAreNotDuplicatesValidator<ImportableOrganization>());
            RuleFor(importRecords => importRecords).SetValidator(_ => new ImportEntitiesAdditionalLinesValidator<ImportableOrganization>());
            RuleForEach(importRecords => importRecords).SetValidator(new ImportMemberValidator<ImportableOrganization>(_countriesService, _dynamicPropertyDictionaryItemsSearchService, _settingsManager));
            RuleForEach(importRecords => importRecords).SetValidator(new ImportOrganizationValidator());
        }
    }
}
