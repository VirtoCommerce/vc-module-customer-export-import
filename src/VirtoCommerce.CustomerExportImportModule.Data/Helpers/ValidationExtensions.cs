using FluentValidation;
using VirtoCommerce.CustomerExportImportModule.Core.Models;

namespace VirtoCommerce.CustomerExportImportModule.Data.Helpers
{
    public static class ValidationExtensions
    {
        public static IRuleBuilderOptions<ImportRecord<T>, TProperty> WithImportState<T, TProperty>(this IRuleBuilderOptions<ImportRecord<T>, TProperty> rule)
        {
            return rule.WithState(importRecord => new ImportValidationState<T> { InvalidRecord = importRecord });
        }
    }
}
