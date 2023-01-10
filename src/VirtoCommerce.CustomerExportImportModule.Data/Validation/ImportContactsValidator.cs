using FluentValidation;
using Microsoft.AspNetCore.Identity;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.Services;

namespace VirtoCommerce.CustomerExportImportModule.Data.Validation
{
    public sealed class ImportContactsValidator: AbstractValidator<ImportRecord<ImportableContact>[]>
    {
        private readonly ICountriesService _countriesService;
        private readonly IDynamicPropertyDictionaryItemsSearchService _dynamicPropertyDictionaryItemsSearchService;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IPasswordValidator<ApplicationUser> _passwordValidator;
        private readonly IStoreSearchService _storeSearchService;
        private readonly ISettingsManager _settingsManager;

        public ImportContactsValidator(ICountriesService countriesService, IDynamicPropertyDictionaryItemsSearchService dynamicPropertyDictionaryItemsSearchService,
            SignInManager<ApplicationUser> signInManager, IPasswordValidator<ApplicationUser> passwordValidator,
            IStoreSearchService storeSearchService, ISettingsManager settingsManager)
        {
            _countriesService = countriesService;
            _dynamicPropertyDictionaryItemsSearchService = dynamicPropertyDictionaryItemsSearchService;
            _signInManager = signInManager;
            _passwordValidator = passwordValidator;
            _storeSearchService = storeSearchService;
            _settingsManager = settingsManager;

            AttachValidators();
        }

        private void AttachValidators()
        {
            RuleFor(importRecords => importRecords).SetValidator(_ => new ImportEntitiesAreNotDuplicatesValidator<ImportableContact>());
            RuleFor(importRecords => importRecords).SetValidator(_ => new ImportEntitiesAdditionalLinesValidator<ImportableContact>());
            RuleForEach(importRecords => importRecords).SetValidator(new ImportMemberValidator<ImportableContact>(_countriesService, _dynamicPropertyDictionaryItemsSearchService));
            RuleForEach(importRecords => importRecords).SetValidator(allRecords => new ImportContactValidator(_signInManager.UserManager, _passwordValidator, _storeSearchService, _settingsManager, allRecords));
        }
    }
}
