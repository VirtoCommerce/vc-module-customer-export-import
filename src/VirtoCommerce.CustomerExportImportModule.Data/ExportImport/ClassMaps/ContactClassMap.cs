using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CsvHelper.Configuration;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.Platform.Core.DynamicProperties;

namespace VirtoCommerce.CustomerExportImportModule.Data.ExportImport.ClassMaps
{
    public sealed class ContactClassMap : ClassMap<ExportableContact>
    {

        public ContactClassMap(string[] dynamicProperties)
        {
            var exportedType = typeof(ExportableContact);
            var columnIndex = MemberMaps.Count;

            if (dynamicProperties.Any())
            {
                var currentClassMap = this;

                // Exporting multiple csv fields from the same property (which is a collection)
                foreach (var dynamicProperty in dynamicProperties)
                {
                    // create CsvPropertyMap manually, because this.Map(x =>...) does not allow
                    // to export multiple entries for the same property

                    var propertyValuesInfo = exportedType.GetProperty(nameof(IHasDynamicProperties.DynamicProperties));
                    var csvPropertyMap = MemberMap.CreateGeneric(exportedType, propertyValuesInfo);
                    csvPropertyMap.Name(dynamicProperty);

                    csvPropertyMap.Data.Index = ++columnIndex;

                    // create custom converter instance which will get the required record from the collection
                    csvPropertyMap.UsingExpression<ICollection<DynamicObjectProperty>>(null, properties =>
                    {
                        var property = properties.FirstOrDefault(x => x.Name == dynamicProperty && x.Values.Any());
                        var propertyValues = Array.Empty<string>();
                        if (property != null)
                        {
                            if (property.IsDictionary)
                            {
                                propertyValues = property.Values
                                    ?.Where(x => x.Value != null)
                                    .Select(x => x.Value.ToString())
                                    .Distinct()
                                    .ToArray();
                            }
                            else
                            {
                                propertyValues = property.Values
                                    ?.Where(x => x.Value != null)
                                    .Select(x => x.Value.ToString())
                                    .ToArray();
                            }
                        }

                        return string.Join(',', propertyValues);
                    });

                    currentClassMap.MemberMaps.Add(csvPropertyMap);
                }
            }
        }

        private static MemberMap CreateMemberMap(Type currentType, PropertyInfo propertyInfo, string columnName, ref int columnIndex)
        {
            var memberMap = MemberMap.CreateGeneric(currentType, propertyInfo);



            memberMap.Data.TypeConverterOptions.CultureInfo = CultureInfo.InvariantCulture;
            memberMap.Data.TypeConverterOptions.NumberStyle = NumberStyles.Any;
            memberMap.Data.TypeConverterOptions.BooleanTrueValues.AddRange(new List<string>() { "yes", "true" });
            memberMap.Data.TypeConverterOptions.BooleanFalseValues.AddRange(new List<string>() { "false", "no" });
            memberMap.Data.Names.Add(columnName);
            memberMap.Data.NameIndex = memberMap.Data.Names.Count - 1;
            memberMap.Data.Index = ++columnIndex;

            return memberMap;
        }
    }
}
