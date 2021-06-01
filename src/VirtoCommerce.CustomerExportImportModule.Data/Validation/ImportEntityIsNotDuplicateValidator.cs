using System.Linq;
using FluentValidation;
using VirtoCommerce.CustomerExportImportModule.Core;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CustomerExportImportModule.Data.Validation
{
    public sealed class ImportEntityIsNotDuplicateValidator<T>: AbstractValidator<ImportRecord<T>>
        where T: IEntity, IHasOuterId
    {
        public ImportEntityIsNotDuplicateValidator()
        {
            AttachValidators();
        }

        private void AttachValidators()
        {
            RuleFor(importRecord => importRecord)
                .Must((_, importRecord, context) =>
                {
                    var duplicates = (ImportRecord<T>[])context.ParentContext.RootContextData[ImportEntityAreNotDuplicatesValidator<T>.Duplicates];
                    return !duplicates.Contains(importRecord);
                })
                .WithErrorCode(ModuleConstants.ValidationErrors.DuplicateError)
                .WithState(importRecord => new ImportValidationState<T> { InvalidRecord = importRecord })
                .WithMessage("This customer is a duplicate.");
        }
    }
}
