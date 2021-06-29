namespace VirtoCommerce.CustomerExportImportModule.Core.Models
{
    public interface IImportRecord<out TRecord>
    {
        int Row { get; set; }

        string RawRecord { get; set; }

        TRecord Record { get; }
    }
}
