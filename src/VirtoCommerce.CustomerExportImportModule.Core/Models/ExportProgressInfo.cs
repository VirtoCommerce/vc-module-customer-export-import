namespace VirtoCommerce.CustomerExportImportModule.Core.Models
{
    public sealed class ExportProgressInfo
    {
        public int ProcessedCount { get; set; }

        public int TotalCount { get; set; }

        public string Description { get; set; }

        public string ContactsFileUrl { get; set; }
        public string OrganizationsFileUrl { get; set; }
    }
}
