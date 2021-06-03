using System.Collections.Generic;
using FluentValidation;
using VirtoCommerce.Platform.Core.DynamicProperties;

namespace VirtoCommerce.CustomerExportImportModule.Data.Validation
{
    public class ImportDynamicPropertiesValidator: AbstractValidator<ICollection<DynamicObjectProperty>>
    {
        public ImportDynamicPropertiesValidator()
        {
            AttachValidators();
        }

        private void AttachValidators()
        {
            RuleForEach(x => x).SetValidator(new ImportDynamicPropertyValidator());
        }
    }
}
