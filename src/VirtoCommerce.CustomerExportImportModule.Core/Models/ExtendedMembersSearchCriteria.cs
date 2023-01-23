using VirtoCommerce.CustomerModule.Core.Model.Search;

namespace VirtoCommerce.CustomerExportImportModule.Core.Models
{
    public class ExtendedMembersSearchCriteria : MembersSearchCriteria
    {
        public bool? ExportData { get; set; }
    }
}
