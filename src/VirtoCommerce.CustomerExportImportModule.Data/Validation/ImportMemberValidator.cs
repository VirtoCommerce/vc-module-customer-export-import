using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Validators;
using VirtoCommerce.CustomerExportImportModule.Core;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerExportImportModule.Data.Helpers;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CustomerExportImportModule.Data.Validation
{
    public class ImportMemberValidator<T>: AbstractValidator<ImportRecord<T>>
        where T: CsvMember
    {
        private readonly ICountriesService _countriesService;

        public ImportMemberValidator(ICountriesService countriesService)
        {
            _countriesService = countriesService;
            AttachValidators();
        }

        private void AttachValidators()
        {
            RuleFor(x => x.Record.Phones)
                .Must(phonesColumnValue =>
                {
                    var phones = string.IsNullOrEmpty(phonesColumnValue) ? null : phonesColumnValue.Split(", ");
                    return phones == null || phones.All(phone => phone.Length <= 64);
                })
                .WithErrorCode(ModuleConstants.ValidationErrors.ArrayValuesExceedingMaxLength)
                .WithMessage(string.Format(ModuleConstants.ValidationMessages[ModuleConstants.ValidationErrors.ArrayValuesExceedingMaxLength], "Phones", 64))
                .WithImportState();

            RuleFor(x => x).CustomAsync(LoadCountriesAsync).SetValidator(_ => new ImportAddressValidator<T>());

            RuleFor(x => x.Record.DynamicProperties).SetValidator(new ImportDynamicPropertiesValidator()).WithImportState();
        }

        private async Task LoadCountriesAsync(ImportRecord<T> importRecord, CustomContext context, CancellationToken cancellationToken)
        {
            context.ParentContext.RootContextData[ImportAddressValidator<T>.Countries] = await _countriesService.GetCountriesAsync();
        }
    }
}
