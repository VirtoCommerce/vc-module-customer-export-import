namespace VirtoCommerce.CustomerExportImportModule.Core.Models
{
    public sealed class ImportValidationState<T>
    {
        public ImportRecord<T> InvalidRecord { get; set; }

        public string FieldName { get; set; }
    }
}
