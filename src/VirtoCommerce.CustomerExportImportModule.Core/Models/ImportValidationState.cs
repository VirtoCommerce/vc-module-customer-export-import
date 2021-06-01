namespace VirtoCommerce.CustomerExportImportModule.Core.Models
{
    public sealed class ImportValidationState
    {
        public CsvContact InvalidContact { get; set; }

        public string FieldName { get; set; }
    }
}
