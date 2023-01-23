using System.Linq;
using FluentValidation;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CustomerExportImportModule.Data.Validation
{
    public sealed class ImportEntitiesAdditionalLinesValidator<T> : AbstractValidator<ImportRecord<T>[]>
        where T : IEntity, IHasOuterId, IImportable
    {
        internal const string WrongAdditionalLines = nameof(WrongAdditionalLines);

        public ImportEntitiesAdditionalLinesValidator()
        {
            AttachValidators();
        }

        private void AttachValidators()
        {
            RuleFor(importRecord => importRecord)
                .Custom(GetWrongAdditionalLines)
                .ForEach(rule => rule.SetValidator(_ => new ImportEntityAdditionalLineValidator<T>()));
        }

        private static void GetWrongAdditionalLines(ImportRecord<T>[] importRecords, ValidationContext<ImportRecord<T>[]> context)
        {
            context.RootContextData[WrongAdditionalLines] = importRecords
                .GroupBy(importRecord => (importRecord.Record.Id, importRecord.Record.OuterId, importRecord.Record.RecordName))
                .Where(group => group.All(x => x.Record.AdditionalLine == true))
                .SelectMany(group => group)
                .ToArray();
        }
    }
}
