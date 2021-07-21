using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CustomerExportImportModule.Core.Models
{
    public sealed class ImportProgressInfo : ValueObject
    {
        public ImportProgressInfo()
        {
            Errors = new List<string>();
        }

        public string Description { get; set; }

        public int ProcessedCount { get; set; }

        public int TotalCount { get; set; }

        public int ContactsCreated { get; set; }

        public int ContactsUpdated { get; set; }

        public int OrganizationsCreated { get; set; }

        public int OrganizationsUpdated { get; set; }

        public int ErrorCount { get; set; }

        public string ReportUrl { get; set; }

        public ICollection<string> Errors { get; set; }
    }
}
