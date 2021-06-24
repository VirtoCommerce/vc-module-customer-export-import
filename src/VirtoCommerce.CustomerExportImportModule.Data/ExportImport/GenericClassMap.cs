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
    {
        public GenericClassMap(IList<DynamicProperty> dynamicProperties, Dictionary<string, IList<DynamicPropertyDictionaryItem>> dynamicPropertyDictionaryItems = null)
        {
            AutoMap(new Configuration { Delimiter = ";", CultureInfo = CultureInfo.InvariantCulture });

            var typeHasDynamicProperties = ClassType.GetInterfaces().Contains(typeof(IHasDynamicProperties));

            if (!dynamicProperties.IsNullOrEmpty() && typeHasDynamicProperties)
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
                dynamicPropertyColumnDefinitionAndWriteMap.UsingExpression<ICollection<DynamicObjectProperty>>(null, dynamicObjectProperties =>
                {
                    var dynamicObjectProperty = dynamicObjectProperties.FirstOrDefault(x => x.Name == dynamicProperty.Name && x.Values.Any());
                    var dynamicObjectPropertyValues = Array.Empty<string>();

                    if (dynamicObjectProperty != null)
                    {
                        if (dynamicObjectProperty.IsDictionary)
                        {
                            dynamicObjectPropertyValues = dynamicObjectProperty.Values?
                                .Where(x => x.Value != null)
                                .Select(x => x.Value.ToString())
                                .Distinct()
                                .ToArray();
                        }
                        else
                        {
                            dynamicObjectPropertyValues = dynamicObjectProperty.Values?
                                .Where(x => x.Value != null)
                                .Select(x => x.Value.ToString())
                                .ToArray();
                        }
                    }

                    return string.Join(',', dynamicObjectPropertyValues);
                });

                MemberMaps.Add(dynamicPropertyColumnDefinitionAndWriteMap);
            }
        }

        private void AddDynamicPropertyReadingMap(IList<DynamicProperty> dynamicProperties, Dictionary<string, IList<DynamicPropertyDictionaryItem>> dynamicPropertyDictionaryItems)
        {
            var currentColumnIndex = MemberMaps.Count;

            var dynamicPropertiesPropertyInfo = ClassType.GetProperty(nameof(IHasDynamicProperties.DynamicProperties));

            var dynamicPropertyReadingMap = MemberMap.CreateGeneric(ClassType, dynamicPropertiesPropertyInfo);
            dynamicPropertyReadingMap.Data.ReadingConvertExpression =
                (Expression<Func<IReaderRow, object>>) (row => dynamicProperties.Select(dynamicProperty =>
                    new DynamicObjectProperty
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
                        Values = dynamicProperty.IsArray
                            ? ToDynamicPropertyMultiValue(dynamicProperty, dynamicPropertyDictionaryItems, row.GetField<string>(dynamicProperty.Name))
                            : new List<DynamicPropertyObjectValue> { ToDynamicPropertyValue(dynamicProperty, dynamicPropertyDictionaryItems, row.GetField<string>(dynamicProperty.Name)) }
                    }).Where(x => x.Values.First().Value != null).ToList());
            dynamicPropertyReadingMap.UsingExpression<ICollection<DynamicObjectProperty>>(null, null);
            dynamicPropertyReadingMap.Ignore(true);
            dynamicPropertyReadingMap.Data.IsOptional = true;
            dynamicPropertyReadingMap.Data.Index = currentColumnIndex + 1;
            MemberMaps.Add(dynamicPropertyReadingMap);
        }

        private IList<DynamicPropertyObjectValue> ToDynamicPropertyMultiValue(DynamicProperty dynamicProperty, Dictionary<string, IList<DynamicPropertyDictionaryItem>> dynamicPropertyDictionaryItems, string values)
        {
            return !string.IsNullOrEmpty(values)
                ? values
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(value => ToDynamicPropertyValue(dynamicProperty, dynamicPropertyDictionaryItems, value))
                    .ToList()
                : new List<DynamicPropertyObjectValue>();
        }

        private DynamicPropertyObjectValue ToDynamicPropertyValue(DynamicProperty dynamicProperty, Dictionary<string, IList<DynamicPropertyDictionaryItem>> dynamicPropertyDictionaryItems, string value)
        {
            return new DynamicPropertyObjectValue
            {
                PropertyName = dynamicProperty.Name,
                PropertyId = dynamicProperty.Id,
                Value = value,
                ValueType = dynamicProperty.ValueType,
                ValueId = dynamicProperty.IsDictionary && dynamicPropertyDictionaryItems[dynamicProperty.Id]
                    .Any(dictionaryItem => dictionaryItem.Name == value)
                    ? dynamicPropertyDictionaryItems[dynamicProperty.Id].FirstOrDefault(dictionaryItem => dictionaryItem.Name == value)?.Id
                    : null
            };
        }
    }
}


