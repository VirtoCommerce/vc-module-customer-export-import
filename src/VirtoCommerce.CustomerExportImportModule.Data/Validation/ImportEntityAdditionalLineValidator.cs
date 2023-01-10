using System.Linq;
using FluentValidation;
using VirtoCommerce.CustomerExportImportModule.Core;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerExportImportModule.Data.Helpers;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CustomerExportImportModule.Data.Validation
{
    public sealed class ImportEntityAdditionalLineValidator<T>: AbstractValidator<ImportRecord<T>>
        where T: IEntity, IHasOuterId, IImportable
    {
        public ImportEntityAdditionalLineValidator()
        {
            AttachValidators();
        }

        private void AttachValidators()
        {
            RuleFor(importRecord => importRecord)
                .Must((_, importRecord, context) =>
                {
                    var wrongLines = (ImportRecord<T>[])context.RootContextData[ImportEntitiesAdditionalLinesValidator<T>.WrongAdditionalLines];
                    return !wrongLines.Contains(importRecord);
                })
                .WithErrorCode(ModuleConstants.ValidationErrors.WrongAdditionalLine)
                .WithMessage("This customer's additional line doesn't have corresponding main line.")
                .WithImportState();
        }
    }
}
