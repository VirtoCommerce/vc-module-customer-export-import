using System.Linq;
using FluentValidation;
using VirtoCommerce.CustomerExportImportModule.Core;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerExportImportModule.Data.Helpers;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CustomerExportImportModule.Data.Validation
{
    public sealed class ImportEntityIsNotDuplicateValidator<T>: AbstractValidator<ImportRecord<T>>
        where T: IEntity, IHasOuterId, IImportable
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
                    var duplicates = (ImportRecord<T>[])context.RootContextData[ImportEntitiesAreNotDuplicatesValidator<T>.Duplicates];
                    return !duplicates.Contains(importRecord);
                })
                .WithErrorCode(ModuleConstants.ValidationErrors.DuplicateError)
                .WithMessage("This customer is a duplicate.")
                .WithImportState();
        }
    }
}
