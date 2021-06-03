using System;
using FluentValidation;
using VirtoCommerce.CustomerExportImportModule.Core;
using VirtoCommerce.Platform.Core.DynamicProperties;

namespace VirtoCommerce.CustomerExportImportModule.Data.Validation
{
    public class ImportDynamicPropertyValidator: AbstractValidator<DynamicObjectProperty>
    {
        public ImportDynamicPropertyValidator()
        {
            AttachValidators();
        }

        private void AttachValidators()
        {
            RuleFor(dynamicProperty => dynamicProperty.Values).NotEmpty().WithErrorCode(ModuleConstants.ValidationErrors.InvalidValue);
            RuleFor(dynamicProperty => dynamicProperty.Values).Must(dynamicPropertyValues => dynamicPropertyValues.Count == 1).When(dynamicProperty => !dynamicProperty.IsDictionary);
            RuleForEach(dynamicProperty => dynamicProperty.Values).ChildRules(childRules =>
            {
                childRules.When(dynamicPropertyValue => dynamicPropertyValue.Value != null, () =>
                {
                    When(dynamicProperty => dynamicProperty.ValueType == DynamicPropertyValueType.ShortText, () =>
                    {
                        childRules.RuleFor(dynamicPropertyValue => dynamicPropertyValue.Value).Must(value => value is string);
                        childRules.RuleFor(dynamicPropertyValue => dynamicPropertyValue.Value as string).MaximumLength(512).When(x => x.ValueType == DynamicPropertyValueType.ShortText);
                    });
                    childRules.RuleFor(dynamicPropertyValue => dynamicPropertyValue.Value).Must(value => decimal.TryParse(value as string, out _))
                        .When(dynamicProperty => dynamicProperty.ValueType == DynamicPropertyValueType.Decimal);
                    childRules.RuleFor(dynamicPropertyValue => dynamicPropertyValue.Value).Must(value => int.TryParse(value as string, out _))
                        .When(dynamicProperty => dynamicProperty.ValueType == DynamicPropertyValueType.Integer);
                    childRules.RuleFor(dynamicPropertyValue => dynamicPropertyValue.Value).Must(value => bool.TryParse(value as string, out _))
                        .When(dynamicProperty => dynamicProperty.ValueType == DynamicPropertyValueType.Boolean);
                    childRules.RuleFor(dynamicPropertyValue => dynamicPropertyValue.Value).Must(value => DateTime.TryParse(value as string, out _))
                        .When(dynamicProperty => dynamicProperty.ValueType == DynamicPropertyValueType.DateTime);
                });
            }).WithErrorCode(ModuleConstants.ValidationErrors.InvalidValue);
        }
    }
}
