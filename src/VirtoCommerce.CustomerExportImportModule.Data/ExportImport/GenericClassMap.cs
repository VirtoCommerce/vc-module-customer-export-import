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
        public GenericClassMap(string[] dynamicPropertyNames = null)
        {
            AutoMap(new Configuration { Delimiter = ";", CultureInfo = CultureInfo.InvariantCulture });
            
            var columnIndex = MemberMaps.Count;

            var typeHasDynamicProperties = ClassType.GetInterfaces().Contains(typeof(IHasDynamicProperties));

            if (!dynamicPropertyNames.IsNullOrEmpty() && typeHasDynamicProperties)
            {
                var currentClassMap = this;

                var dynamicPropertiesPropertyInfo = ClassType.GetProperty(nameof(IHasDynamicProperties.DynamicProperties));

                // Exporting multiple csv fields from the same property (which is a collection)
                foreach (var dynamicPropertyName in dynamicPropertyNames)
                {
                    // create CsvPropertyMap manually, because this.Map(x =>...) does not allow
                    // to export multiple entries for the same property
                    var dynamicPropertyColumnDefinitionAndWriteMap = MemberMap.CreateGeneric(ClassType, dynamicPropertiesPropertyInfo);
                    dynamicPropertyColumnDefinitionAndWriteMap.Name(dynamicPropertyName);
                    dynamicPropertyColumnDefinitionAndWriteMap.Data.IsOptional = true;
                    dynamicPropertyColumnDefinitionAndWriteMap.Data.Index = columnIndex++;

                    // create custom converter instance which will get the required record from the collection
                    dynamicPropertyColumnDefinitionAndWriteMap.UsingExpression<ICollection<DynamicObjectProperty>>(null, dynamicProperties =>
                    {
                        var dynamicProperty = dynamicProperties.FirstOrDefault(x => x.Name == dynamicPropertyName && x.Values.Any());
                        var dynamicPropertyValues = Array.Empty<string>();

                        if (dynamicProperty != null)
                        {
                            if (dynamicProperty.IsDictionary)
                            {
                                dynamicPropertyValues = dynamicProperty.Values
                                    ?.Where(x => x.Value != null)
                                    .Select(x => x.Value.ToString())
                                    .Distinct()
                                    .ToArray();
                            }
                            else
                            {
                                dynamicPropertyValues = dynamicProperty.Values
                                    ?.Where(x => x.Value != null)
                                    .Select(x => x.Value.ToString())
                                    .ToArray();
                            }
                        }

                        return string.Join(',', dynamicPropertyValues);
                    });

                    currentClassMap.MemberMaps.Add(dynamicPropertyColumnDefinitionAndWriteMap);
                }

                var dynamicPropertyReadingMap = MemberMap.CreateGeneric(ClassType, dynamicPropertiesPropertyInfo);
                dynamicPropertyReadingMap.Data.ReadingConvertExpression =
                    (Expression<Func<IReaderRow, object>>) (row => dynamicPropertyNames.Select(dynamicPropertyName =>
                        new DynamicObjectProperty
                        {
                            Name = dynamicPropertyName,
                            Values = new List<DynamicPropertyObjectValue>
                            {
                                new DynamicPropertyObjectValue { PropertyName = dynamicPropertyName, Value = row.GetField<string>(dynamicPropertyName) }
                            }
                        }).Where(x => x.Values.First().Value != null).ToList());
                dynamicPropertyReadingMap.UsingExpression<ICollection<DynamicObjectProperty>>(null, null);
                dynamicPropertyReadingMap.Ignore(true);
                dynamicPropertyReadingMap.Data.IsOptional = true;
                dynamicPropertyReadingMap.Data.Index = columnIndex + 1;
                MemberMaps.Add(dynamicPropertyReadingMap);
            }
        }
    }
}
