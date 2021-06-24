using FluentValidation;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DynamicProperties;

namespace VirtoCommerce.CustomerExportImportModule.Data.Validation
{
    public sealed class ImportOrganizationsValidator : AbstractValidator<ImportRecord<CsvOrganization>[]>
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
            RuleFor(importRecords => importRecords).SetValidator(_ => new ImportEntitiesAreNotDuplicatesValidator<CsvOrganization>());
            RuleForEach(importRecords => importRecords).SetValidator(new ImportMemberValidator<CsvOrganization>(_countriesService, _dynamicPropertyDictionaryItemsSearchService));
            RuleForEach(importRecords => importRecords).SetValidator(new ImportOrganizationValidator());
        }
    }
}
