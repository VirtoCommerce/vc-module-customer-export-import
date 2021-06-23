using System.Collections.Generic;
using FluentValidation;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.Platform.Core.DynamicProperties;

namespace VirtoCommerce.CustomerExportImportModule.Data.Validation
{
    public class ImportDynamicPropertiesValidator<T>: AbstractValidator<ICollection<DynamicObjectProperty>>
        where T: CsvMember
    {
        private readonly ImportRecord<T> _importRecord;

        public ImportDynamicPropertiesValidator(ImportRecord<T> importRecord)
        {
            _importRecord = importRecord;
            AttachValidators();
        }

        private void AttachValidators()
        {
            RuleForEach(x => x).SetValidator(new ImportDynamicPropertyValidator<T>(_importRecord));
        }
    }
}
