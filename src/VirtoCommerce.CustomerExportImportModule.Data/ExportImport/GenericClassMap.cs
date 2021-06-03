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
        public GenericClassMap(IList<DynamicProperty> dynamicProperties = null)
        {
            AutoMap(new Configuration { Delimiter = ";", CultureInfo = CultureInfo.InvariantCulture });
            
            var columnIndex = MemberMaps.Count;

            var typeHasDynamicProperties = ClassType.GetInterfaces().Contains(typeof(IHasDynamicProperties));

            if (!dynamicProperties.IsNullOrEmpty() && typeHasDynamicProperties)
            {
                var currentClassMap = this;

                var dynamicPropertiesPropertyInfo = ClassType.GetProperty(nameof(IHasDynamicProperties.DynamicProperties));

                // Exporting multiple csv fields from the same property (which is a collection)
                foreach (var dynamicProperty in dynamicProperties)
                {
                    // create CsvPropertyMap manually, because this.Map(x =>...) does not allow
                    // to export multiple entries for the same property
                    var dynamicPropertyColumnDefinitionAndWriteMap = MemberMap.CreateGeneric(ClassType, dynamicPropertiesPropertyInfo);
                    dynamicPropertyColumnDefinitionAndWriteMap.Name(dynamicProperty.Name);
                    dynamicPropertyColumnDefinitionAndWriteMap.Data.IsOptional = true;
                    dynamicPropertyColumnDefinitionAndWriteMap.Data.Index = columnIndex++;

                    // create custom converter instance which will get the required record from the collection
                    dynamicPropertyColumnDefinitionAndWriteMap.UsingExpression<ICollection<DynamicObjectProperty>>(null, dynamicObjectProperties =>
                    {
                        var dynamicObjectProperty = dynamicObjectProperties.FirstOrDefault(x => x.Name == dynamicProperty.Name && x.Values.Any());
                        var dynamicObjectPropertyValues = Array.Empty<string>();

                        if (dynamicObjectProperty != null)
                        {
                            if (dynamicObjectProperty.IsDictionary)
                            {
                                dynamicObjectPropertyValues = dynamicObjectProperty.Values
                                    ?.Where(x => x.Value != null)
                                    .Select(x => x.Value.ToString())
                                    .Distinct()
                                    .ToArray();
                            }
                            else
                            {
                                dynamicObjectPropertyValues = dynamicObjectProperty.Values
                                    ?.Where(x => x.Value != null)
                                    .Select(x => x.Value.ToString())
                                    .ToArray();
                            }
                        }

                        return string.Join(',', dynamicObjectPropertyValues);
                    });

                    currentClassMap.MemberMaps.Add(dynamicPropertyColumnDefinitionAndWriteMap);
                }

                var dynamicPropertyReadingMap = MemberMap.CreateGeneric(ClassType, dynamicPropertiesPropertyInfo);
                dynamicPropertyReadingMap.Data.ReadingConvertExpression =
                    (Expression<Func<IReaderRow, object>>) (row => dynamicProperties.Select(dynamicProperty =>
                        new DynamicObjectProperty
                        {
                            Name = dynamicProperty.Name,
                            DisplayNames = dynamicProperty.DisplayNames,
                            DisplayOrder = dynamicProperty.DisplayOrder,
                            Description = dynamicProperty.Description,
                            IsArray = dynamicProperty.IsArray,
                            IsDictionary = dynamicProperty.IsDictionary,
                            IsMultilingual = dynamicProperty.IsMultilingual,
                            IsRequired = dynamicProperty.IsRequired,
                            ValueType = dynamicProperty.ValueType,
                            Values = new List<DynamicPropertyObjectValue>
                            {
                                new DynamicPropertyObjectValue
                                {
                                    PropertyName = dynamicProperty.Name,
                                    PropertyId = dynamicProperty.Id,
                                    Value = row.GetField<string>(dynamicProperty.Name),
                                    ValueType = dynamicProperty.ValueType
                                }
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
