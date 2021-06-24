using FluentValidation;
using Microsoft.AspNetCore.Identity;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.CustomerExportImportModule.Data.Validation
{
    public sealed class ImportContactsValidator: AbstractValidator<ImportRecord<CsvContact>[]>
    {
        private readonly ICountriesService _countriesService;
        private readonly IDynamicPropertyDictionaryItemsSearchService _dynamicPropertyDictionaryItemsSearchService;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public ImportContactsValidator(ICountriesService countriesService, IDynamicPropertyDictionaryItemsSearchService dynamicPropertyDictionaryItemsSearchService, SignInManager<ApplicationUser> signInManager)
        {
            _countriesService = countriesService;
            _dynamicPropertyDictionaryItemsSearchService = dynamicPropertyDictionaryItemsSearchService;
            _signInManager = signInManager;

            AttachValidators();
        }

        private void AttachValidators()
        {
            RuleFor(importRecords => importRecords).SetValidator(_ => new ImportEntitiesAreNotDuplicatesValidator<CsvContact>());
            RuleForEach(importRecords => importRecords).SetValidator(new ImportMemberValidator<CsvContact>(_countriesService, _dynamicPropertyDictionaryItemsSearchService));
            RuleForEach(importRecords => importRecords).SetValidator(new ImportContactValidator(_signInManager.UserManager));
        }
    }
}
