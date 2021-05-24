namespace VirtoCommerce.CustomerExportImportModule.Core.Models
{
    public sealed class ImportValidationState
    {
        public ImportableContact InvalidContact { get; set; }

        public string FieldName { get; set; }
    }
}
