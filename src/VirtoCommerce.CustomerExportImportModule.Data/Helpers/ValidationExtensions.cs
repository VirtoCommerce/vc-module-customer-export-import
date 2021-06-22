using FluentValidation;
using VirtoCommerce.CustomerExportImportModule.Core;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.Platform.Core.DynamicProperties;

namespace VirtoCommerce.CustomerExportImportModule.Data.Helpers
{
    public static class ValidationExtensions
    {
        public static IRuleBuilderOptions<ImportRecord<T>, TProperty> WithImportState<T, TProperty>(this IRuleBuilderOptions<ImportRecord<T>, TProperty> rule)
        {
            return rule.WithState(importRecord => new ImportValidationState<T> { InvalidRecord = importRecord });
        }

        public static IRuleBuilderOptions<ImportRecord<T>, TProperty> WithMissingRequiredValueCodeAndMessage<T, TProperty>(this IRuleBuilderOptions<ImportRecord<T>, TProperty> rule, string columnName)
        {
            return rule
                .WithErrorCode(ModuleConstants.ValidationErrors.MissingRequiredValues)
                .WithMessage(string.Format(ModuleConstants.ValidationMessages[ModuleConstants.ValidationErrors.MissingRequiredValues], columnName));
        }

        public static IRuleBuilderOptions<ImportRecord<T>, TProperty> WithExceededMaxLengthCodeAndMessage<T, TProperty>(this IRuleBuilderOptions<ImportRecord<T>, TProperty> rule, string columnName)
        {
            return rule
                .WithErrorCode(ModuleConstants.ValidationErrors.ExceedingMaxLength)
                .WithMessage(string.Format(ModuleConstants.ValidationMessages[ModuleConstants.ValidationErrors.ExceedingMaxLength], columnName));
        }

        public static IRuleBuilderOptions<DynamicPropertyObjectValue, TProperty> WithExceededMaxLengthCodeAndMessage<TProperty>(this IRuleBuilderOptions<DynamicPropertyObjectValue, TProperty> rule)
        {
            return rule
                .WithErrorCode(ModuleConstants.ValidationErrors.ExceedingMaxLength)
                .WithMessage(dynamicPropertyValue => string.Format(ModuleConstants.ValidationMessages[ModuleConstants.ValidationErrors.ExceedingMaxLength], dynamicPropertyValue.PropertyName));
        }

        public static IRuleBuilderOptions<ImportRecord<T>, TProperty> WithInvalidValueCodeAndMessage<T, TProperty>(this IRuleBuilderOptions<ImportRecord<T>, TProperty> rule, string columnName)
        {
            return rule
                .WithErrorCode(ModuleConstants.ValidationErrors.InvalidValue)
                .WithMessage(string.Format(ModuleConstants.ValidationMessages[ModuleConstants.ValidationErrors.InvalidValue], columnName));
        }

        public static IRuleBuilderOptions<DynamicObjectProperty, TProperty> WithInvalidValueCodeAndMessage<TProperty>(this IRuleBuilderOptions<DynamicObjectProperty, TProperty> rule)
        {
            return rule
                .WithErrorCode(ModuleConstants.ValidationErrors.InvalidValue)
                .WithMessage(dynamicPropertyValue => string.Format(ModuleConstants.ValidationMessages[ModuleConstants.ValidationErrors.InvalidValue], dynamicPropertyValue.Name));
        }

        public static IRuleBuilderOptions<DynamicPropertyObjectValue, TProperty> WithInvalidValueCodeAndMessage<TProperty>(this IRuleBuilderOptions<DynamicPropertyObjectValue, TProperty> rule)
        {
            return rule
                .WithErrorCode(ModuleConstants.ValidationErrors.InvalidValue)
                .WithMessage(dynamicPropertyValue => string.Format(ModuleConstants.ValidationMessages[ModuleConstants.ValidationErrors.InvalidValue], dynamicPropertyValue.PropertyName));
        }
    }
}
