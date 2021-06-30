using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Validators;
using VirtoCommerce.CustomerExportImportModule.Core;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerExportImportModule.Data.Helpers;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DynamicProperties;

namespace VirtoCommerce.CustomerExportImportModule.Data.Validation
{
    public class ImportMemberValidator<T>: AbstractValidator<ImportRecord<T>>
        where T: CsvMember
    {
        private readonly ICountriesService _countriesService;
        private readonly IDynamicPropertyDictionaryItemsSearchService _dynamicPropertyDictionaryItemsSearchService;

        public ImportMemberValidator(ICountriesService countriesService, IDynamicPropertyDictionaryItemsSearchService dynamicPropertyDictionaryItemsSearchService)
        {
            _countriesService = countriesService;
            _dynamicPropertyDictionaryItemsSearchService = dynamicPropertyDictionaryItemsSearchService;
            AttachValidators();
        }

        private void AttachValidators()
        {
            RuleFor(x => x.Record.Phones)
                .Must(phonesColumnValue =>
                {
                    var phones = string.IsNullOrEmpty(phonesColumnValue) ? null : phonesColumnValue.Split(',').Select(phone => phone.Trim()).ToList();
                    return phones == null || phones.All(phone => phone.Length <= 64);
                })
                .WithErrorCode(ModuleConstants.ValidationErrors.ArrayValuesExceedingMaxLength)
                .WithMessage(string.Format(ModuleConstants.ValidationMessages[ModuleConstants.ValidationErrors.ArrayValuesExceedingMaxLength], "Phones", 64))
                .WithImportState();

            RuleFor(x => x).CustomAsync(LoadCountriesAsync).SetValidator(_ => new ImportAddressValidator<T>());

            RuleFor(x => x.Record.DynamicProperties).CustomAsync(LoadDynamicPropertyDictionaryItems).SetValidator(record => new ImportDynamicPropertiesValidator<T>(record));
        }

        private async Task LoadDynamicPropertyDictionaryItems(ICollection<DynamicObjectProperty> dynamicProperties, CustomContext context, CancellationToken cancellationToken)
        {
            var dynamicPropertyDictionaryItems = new List<DynamicPropertyDictionaryItem>();

            if (dynamicProperties != null)
            {
                foreach (var dynamicProperty in dynamicProperties.Where(dynamicProperty => dynamicProperty.IsDictionary))
                {
                    var dynamicPropertyDictionaryItemsSearchResult =
                        await _dynamicPropertyDictionaryItemsSearchService.SearchDictionaryItemsAsync(new DynamicPropertyDictionaryItemSearchCriteria { PropertyId = dynamicProperty.Id });
                    dynamicPropertyDictionaryItems.AddRange(dynamicPropertyDictionaryItemsSearchResult.Results);
                }
            }

            context.ParentContext.RootContextData[ImportDynamicPropertyValidator<T>.DynamicPropertyDictionaryItems] = dynamicPropertyDictionaryItems;
        }

        private async Task LoadCountriesAsync(ImportRecord<T> importRecord, CustomContext context, CancellationToken cancellationToken)
        {
            context.ParentContext.RootContextData[ImportAddressValidator<T>.Countries] = await _countriesService.GetCountriesAsync();
        }
    }
}
