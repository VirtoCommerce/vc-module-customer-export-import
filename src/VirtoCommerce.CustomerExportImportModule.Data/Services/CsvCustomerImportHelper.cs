using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CsvHelper.Configuration.Attributes;

namespace VirtoCommerce.CustomerExportImportModule.Data.Services
{
    public static class CsvCustomerImportHelper
    {
        public static string[] GetImportCustomerRequiredColumns<T>()
        {
            var requiredColumns = typeof(T).GetProperties()
                .Where(p => Attribute.IsDefined(p, typeof(RequiredAttribute)))
                .Select(p =>
                    ((NameAttribute)Attribute.GetCustomAttribute(p, typeof(NameAttribute)))?.Names.First() ??
                    p.Name).ToArray();

            return requiredColumns;
        }
    }
}
