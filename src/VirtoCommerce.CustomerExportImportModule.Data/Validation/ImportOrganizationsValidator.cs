using FluentValidation;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DynamicProperties;

namespace VirtoCommerce.CustomerExportImportModule.Data.Validation
{
    public sealed class ImportOrganizationsValidator : AbstractValidator<ImportRecord<ImportableOrganization>[]>
    {
        private readonly ICountriesService _countriesService;
        private readonly IDynamicPropertyDictionaryItemsSearchService _dynamicPropertyDictionaryItemsSearchService;

        public ImportOrganizationsValidator(ICountriesService countriesService, IDynamicPropertyDictionaryItemsSearchService dynamicPropertyDictionaryItemsSearchService)
        {
            _countriesService = countriesService;
            _dynamicPropertyDictionaryItemsSearchService = dynamicPropertyDictionaryItemsSearchService;
            AttachValidators();
        }

        private void AttachValidators()
        {
            RuleFor(importRecords => importRecords).SetValidator(_ => new ImportEntitiesAreNotDuplicatesValidator<ImportableOrganization>());
            RuleFor(importRecords => importRecords).SetValidator(_ => new ImportEntitiesAdditionalLinesValidator<ImportableOrganization>());
            RuleForEach(importRecords => importRecords).SetValidator(new ImportMemberValidator<ImportableOrganization>(_countriesService, _dynamicPropertyDictionaryItemsSearchService));
            RuleForEach(importRecords => importRecords).SetValidator(new ImportOrganizationValidator());
        }
    }
}
