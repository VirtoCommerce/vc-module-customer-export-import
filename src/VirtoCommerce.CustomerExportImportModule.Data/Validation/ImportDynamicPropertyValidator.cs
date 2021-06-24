using System;
using FluentValidation;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerExportImportModule.Data.Helpers;
using VirtoCommerce.Platform.Core.DynamicProperties;

namespace VirtoCommerce.CustomerExportImportModule.Data.Validation
{
    public class ImportDynamicPropertyValidator<T>: AbstractValidator<DynamicObjectProperty>
        where T : CsvMember
    {
        private readonly ImportRecord<T> _importRecord;

        public ImportDynamicPropertyValidator(ImportRecord<T> importRecord)
        {
            _importRecord = importRecord;
            AttachValidators();
        }

        private void AttachValidators()
        {
            RuleFor(dynamicProperty => dynamicProperty.Values)
                .NotEmpty()
                .WithInvalidValueCodeAndMessage()
                .WithImportState(_importRecord)
                .DependentRules(() =>
                {
                    RuleFor(dynamicProperty => dynamicProperty.Values)
                        .Must(dynamicPropertyValues => dynamicPropertyValues.Count == 1)
                        .When(dynamicProperty => !dynamicProperty.IsArray)
                        .WithInvalidValueCodeAndMessage()
                        .WithImportState(_importRecord)
                        .DependentRules(() =>
                        {
                            RuleForEach(dynamicProperty => dynamicProperty.Values).ChildRules(childRules =>
                            {
                                childRules.When(dynamicPropertyValue => !string.IsNullOrEmpty(dynamicPropertyValue.Value as string), () =>
                                {
                                    When(dynamicProperty => dynamicProperty.ValueType == DynamicPropertyValueType.ShortText, () =>
                                    {
                                        childRules.RuleFor(dynamicPropertyValue => dynamicPropertyValue.Value)
                                            .Must(value => value is string)
                                            .WithInvalidValueCodeAndMessage()
                                            .WithImportState(_importRecord)
                                            .DependentRules(() =>
                                            {
                                                childRules.RuleFor(dynamicPropertyValue => dynamicPropertyValue.Value as string)
                                                    .MaximumLength(512)
                                                    .When(x => x.ValueType == DynamicPropertyValueType.ShortText)
                                                    .WithExceededMaxLengthCodeAndMessage(512)
                                                    .WithImportState(_importRecord);
                                            });
                                    });
                                    childRules.RuleFor(dynamicPropertyValue => dynamicPropertyValue.Value)
                                        .Must(value => value is string)
                                        .When(dynamicProperty => dynamicProperty.ValueType == DynamicPropertyValueType.LongText)
                                        .WithInvalidValueCodeAndMessage()
                                        .WithImportState(_importRecord);
                                    childRules.RuleFor(dynamicPropertyValue => dynamicPropertyValue.Value)
                                        .Must(value => decimal.TryParse(value as string, out _))
                                        .When(dynamicProperty => dynamicProperty.ValueType == DynamicPropertyValueType.Decimal)
                                        .WithInvalidValueCodeAndMessage()
                                        .WithImportState(_importRecord);
                                    childRules.RuleFor(dynamicPropertyValue => dynamicPropertyValue.Value)
                                        .Must(value => int.TryParse(value as string, out _))
                                        .When(dynamicProperty => dynamicProperty.ValueType == DynamicPropertyValueType.Integer)
                                        .WithInvalidValueCodeAndMessage()
                                        .WithImportState(_importRecord);
                                    childRules.RuleFor(dynamicPropertyValue => dynamicPropertyValue.Value)
                                        .Must(value => bool.TryParse(value as string, out _))
                                        .When(dynamicProperty => dynamicProperty.ValueType == DynamicPropertyValueType.Boolean)
                                        .WithInvalidValueCodeAndMessage()
                                        .WithImportState(_importRecord);
                                    childRules.RuleFor(dynamicPropertyValue => dynamicPropertyValue.Value)
                                        .Must(value => DateTime.TryParse(value as string, out _))
                                        .When(dynamicProperty => dynamicProperty.ValueType == DynamicPropertyValueType.DateTime)
                                        .WithInvalidValueCodeAndMessage()
                                        .WithImportState(_importRecord);
                                });
                            });
                        });
                });
        }
    }
}
