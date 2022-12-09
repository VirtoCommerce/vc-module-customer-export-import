using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using CsvHelper;
using CsvHelper.Configuration;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DynamicProperties;

namespace VirtoCommerce.CustomerExportImportModule.Data.ExportImport
{
    public sealed class GenericClassMap<T> : ClassMap<T>
        where T : IHasDynamicProperties
    {
        public GenericClassMap(IList<DynamicProperty> dynamicProperties, IDictionary<string, IList<DynamicPropertyDictionaryItem>> dynamicPropertyDictionaryItems = null)
        {
            AutoMap(new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";" });

            if (!dynamicProperties.IsNullOrEmpty())
            {
                AddDynamicPropertyColumnDefinitionAndWritingMap(dynamicProperties);

                AddDynamicPropertyReadingMap(dynamicProperties, dynamicPropertyDictionaryItems);
            }
        }

        private void AddDynamicPropertyColumnDefinitionAndWritingMap(IList<DynamicProperty> dynamicProperties)
        {
            var currentColumnIndex = MemberMaps.Count;

            var dynamicPropertiesPropertyInfo = ClassType.GetProperty(nameof(IHasDynamicProperties.DynamicProperties));

            // Exporting multiple csv fields from the same property (which is a collection)
            foreach (var dynamicProperty in dynamicProperties)
            {
                // create CsvPropertyMap manually, because this.Map(x =>...) does not allow
                // to export multiple entries for the same property
                var dynamicPropertyColumnDefinitionAndWriteMap = MemberMap.CreateGeneric(ClassType, dynamicPropertiesPropertyInfo);
                dynamicPropertyColumnDefinitionAndWriteMap.Name(dynamicProperty.Name);
                dynamicPropertyColumnDefinitionAndWriteMap.Data.IsOptional = true;
                dynamicPropertyColumnDefinitionAndWriteMap.Data.Index = currentColumnIndex++;

                // create custom converter instance which will get the required record from the collection
                ConvertToString<T> func = args =>
                {
                    string[] dynamicObjectPropertyValues = null;
                    var dynamicObjectProperty = args.Value.DynamicProperties.FirstOrDefault(x => x.Name == dynamicProperty.Name && x.Values?.Count > 0);

                    if (dynamicObjectProperty != null)
                    {
                        if (dynamicObjectProperty.IsDictionary)
                        {
                            dynamicObjectPropertyValues = dynamicObjectProperty.Values?
                                .Where(x => x.Value != null)
                                .Select(x => FormattableString.Invariant($"{x.Value}"))
                                .Distinct()
                                .ToArray();
                        }
                        else
                        {
                            dynamicObjectPropertyValues = dynamicObjectProperty.Values?
                                .Where(x => x.Value != null)
                                .Select(x => FormattableString.Invariant($"{x.Value}"))
                                .ToArray();
                        }
                    }

                    return string.Join(", ", dynamicObjectPropertyValues ?? Array.Empty<string>());
                };

                dynamicPropertyColumnDefinitionAndWriteMap.Data.WritingConvertExpression = (Expression<ConvertToString<T>>)(args => func(args));

                MemberMaps.Add(dynamicPropertyColumnDefinitionAndWriteMap);
            }
        }

        private void AddDynamicPropertyReadingMap(IList<DynamicProperty> dynamicProperties, IDictionary<string, IList<DynamicPropertyDictionaryItem>> dynamicPropertyDictionaryItems)
        {
            var currentColumnIndex = MemberMaps.Count;

            var dynamicPropertiesPropertyInfo = ClassType.GetProperty(nameof(IHasDynamicProperties.DynamicProperties));
            var dynamicPropertyReadingMap = MemberMap.CreateGeneric(ClassType, dynamicPropertiesPropertyInfo);
            dynamicPropertyReadingMap.Ignore(true);
            dynamicPropertyReadingMap.Data.IsOptional = true;
            dynamicPropertyReadingMap.Data.Index = currentColumnIndex + 1;

            ConvertFromString<ICollection<DynamicObjectProperty>> func = args =>
            {
                var row = args.Row;

                return dynamicProperties
                    .Where(x => row.HeaderRecord.Contains(x.Name))
                    .Select(dynamicProperty =>
                    {
                        var values = row.GetField<string>(dynamicProperty.Name);

                        return !string.IsNullOrEmpty(values)
                            ? new DynamicObjectProperty
                            {
                                Id = dynamicProperty.Id,
                                Name = dynamicProperty.Name,
                                DisplayNames = dynamicProperty.DisplayNames,
                                DisplayOrder = dynamicProperty.DisplayOrder,
                                Description = dynamicProperty.Description,
                                IsArray = dynamicProperty.IsArray,
                                IsDictionary = dynamicProperty.IsDictionary,
                                IsMultilingual = dynamicProperty.IsMultilingual,
                                IsRequired = dynamicProperty.IsRequired,
                                ValueType = dynamicProperty.ValueType,
                                Values = ToDynamicPropertyValues(dynamicProperty, dynamicPropertyDictionaryItems, values)
                            }
                            : null;
                    })
                    .Where(x => x != null)
                    .ToList();
            };

            dynamicPropertyReadingMap.Data.ReadingConvertExpression = (Expression<ConvertFromString<ICollection<DynamicObjectProperty>>>)(args => func(args));

            MemberMaps.Add(dynamicPropertyReadingMap);
        }

        private static IList<DynamicPropertyObjectValue> ToDynamicPropertyValues(DynamicProperty dynamicProperty, IDictionary<string, IList<DynamicPropertyDictionaryItem>> dynamicPropertyDictionaryItems, string values) =>
            dynamicProperty.IsArray
                ? ToDynamicPropertyMultiValue(dynamicProperty, dynamicPropertyDictionaryItems, values)
                : new[] { ToDynamicPropertyValue(dynamicProperty, dynamicPropertyDictionaryItems, values) };

        private static DynamicPropertyObjectValue ToDynamicPropertyValue(DynamicProperty dynamicProperty, IDictionary<string, IList<DynamicPropertyDictionaryItem>> dynamicPropertyDictionaryItems, string value) =>
            new()
            {
                PropertyName = dynamicProperty.Name,
                PropertyId = dynamicProperty.Id,
                Value = value,
                ValueType = dynamicProperty.ValueType,
                ValueId = dynamicProperty.IsDictionary
                    ? dynamicPropertyDictionaryItems[dynamicProperty.Id].FirstOrDefault(item => item.Name == value)?.Id
                    : null
            };

        private static IList<DynamicPropertyObjectValue> ToDynamicPropertyMultiValue(DynamicProperty dynamicProperty, IDictionary<string, IList<DynamicPropertyDictionaryItem>> dynamicPropertyDictionaryItems, string values) =>
            values.Split(',')
                .Select(value => ToDynamicPropertyValue(dynamicProperty, dynamicPropertyDictionaryItems, value.Trim()))
                .ToArray();
    }
}


