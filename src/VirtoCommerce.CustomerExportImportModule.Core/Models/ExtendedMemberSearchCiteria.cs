using VirtoCommerce.CustomerModule.Core.Model.Search;

namespace VirtoCommerce.CustomerExportImportModule.Core.Models
{
    public class ExtendedMemberSearchCiteria : MembersSearchCriteria
    {
        public string[] OuterIds { get; set; }
    }
}
