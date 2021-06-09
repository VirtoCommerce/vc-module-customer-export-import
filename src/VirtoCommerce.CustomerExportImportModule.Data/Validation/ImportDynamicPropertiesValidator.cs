using FluentValidation;
using VirtoCommerce.CustomerExportImportModule.Core.Models;

namespace VirtoCommerce.CustomerExportImportModule.Data.Validation
{
    public class ImportDynamicPropertiesValidator: AbstractValidator<ImportRecord<CsvContact>>
    {
        public ImportDynamicPropertiesValidator()
        {
            AttachValidators();
        }

        private void AttachValidators()
        {
            RuleForEach(x => x.Record.DynamicProperties).SetValidator(new ImportDynamicPropertyValidator());
        }
    }
}
