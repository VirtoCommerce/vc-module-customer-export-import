namespace VirtoCommerce.CustomerExportImportModule.Core.Models
{
    /// <summary>
    /// Interface to implement importable entities.
    /// </summary>
    public interface IImportable
    {
        string RecordName { get; set; }

        bool? AdditionalLine { get; set; }
    }
}
