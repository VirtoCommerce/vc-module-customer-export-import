using System.Linq;
using FluentValidation;
using FluentValidation.Validators;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CustomerExportImportModule.Data.Validation
{
    public sealed class ImportEntitiesAreNotDuplicatesValidator<T> : AbstractValidator<ImportRecord<T>[]>
        where T : IEntity, IHasOuterId
    {
        internal const string Duplicates = nameof(Duplicates);

        public ImportEntitiesAreNotDuplicatesValidator()
        {
            AttachValidators();
        }

        private void AttachValidators()
        {
            RuleFor(importRecord => importRecord)
                .Custom(GetDuplicates)
                .ForEach(rule => rule.SetValidator(_ => new ImportEntityIsNotDuplicateValidator<T>()));
        }

        private void GetDuplicates(ImportRecord<T>[] importRecords, ValidationContext<T> context)
        {
            var duplicatesById = importRecords.Where(importRecord => !string.IsNullOrEmpty(importRecord.Record.Id))
                .GroupBy(importRecord => importRecord.Record.Id)
                .SelectMany(group => group.Take(group.Count() - 1))
                .ToArray();

            var duplicatesByOuterId = importRecords.Where(importRecord => !string.IsNullOrEmpty(importRecord.Record.OuterId))
                .GroupBy(importRecord => importRecord.Record.OuterId)
                .SelectMany(group => group.Take(group.Count() - 1))
                .ToArray();
            context.RootContextData[Duplicates] = duplicatesById.Concat(duplicatesByOuterId).Distinct().ToArray();
        }
    }
}
