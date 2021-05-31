using VirtoCommerce.CustomerModule.Core.Model;

namespace VirtoCommerce.CustomerExportImportModule.Core.Models
{
    public class ImportContact: ImportRecord<CsvContact>
    {
        public Contact Contact { get; set; }
    }
}
