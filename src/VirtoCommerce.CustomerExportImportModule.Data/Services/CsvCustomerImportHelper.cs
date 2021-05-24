using System;
using System.Linq;
using CsvHelper.Configuration.Attributes;
using VirtoCommerce.CustomerExportImportModule.Data.Models;

namespace VirtoCommerce.CustomerExportImportModule.Data.Services
{
    public static class CsvCustomerImportHelper
    {
        public static string[] GetImportPriceRequiredColumns()
        {
            var requiredColumns = typeof(CsvCustomer).GetProperties()
                .Select(p =>
                    ((NameAttribute)Attribute.GetCustomAttribute(p, typeof(NameAttribute)))?.Names.First() ??
                    p.Name).ToArray();

            return requiredColumns;
        }
    }
}
