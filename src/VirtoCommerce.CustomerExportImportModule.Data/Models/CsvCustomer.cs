using CsvHelper.Configuration.Attributes;

namespace VirtoCommerce.CustomerExportImportModule.Data.Models
{
    public sealed class CsvCustomer
    {
        [Name("First name")]
        public string FirstName { get; set; }

        [Name("Last name")]
        public string LastName { get; set; }

        [Name("Full name")]
        public string FullName { get; set; }
    }
}
