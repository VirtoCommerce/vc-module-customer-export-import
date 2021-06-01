namespace VirtoCommerce.CustomerExportImportModule.Core.Models
{
    public sealed class ImportDataRequest
    {
        public string OrganizationId { get; set; }

        public string FilePath { get; set; }

        public string MemberType { get; set; }
    }
}
