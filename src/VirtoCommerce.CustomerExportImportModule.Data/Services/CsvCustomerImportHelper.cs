using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CsvHelper.Configuration.Attributes;
using VirtoCommerce.CustomerExportImportModule.Core.Models;

namespace VirtoCommerce.CustomerExportImportModule.Data.Services
{
    public static class CsvCustomerImportHelper
    {
        public static string[] GetImportCustomerRequiredColumns()
        {
            var requiredColumns = typeof(CsvContact).GetProperties()
                .Where(p => Attribute.IsDefined(p, typeof(RequiredAttribute)))
                .Select(p =>
                    ((NameAttribute)Attribute.GetCustomAttribute(p, typeof(NameAttribute)))?.Names.First() ??
                    p.Name).ToArray();

            return requiredColumns;
        }
    }
}
