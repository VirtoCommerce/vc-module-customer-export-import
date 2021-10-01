using System;
using System.Collections.Generic;
using VirtoCommerce.Platform.Core.PushNotifications;

namespace VirtoCommerce.CustomerExportImportModule.Core.Models
{
    public sealed class ImportPushNotification : PushNotification
    {
        public ImportPushNotification(string creator)
            : base(creator)
        {
            Errors = new List<string>();
        }

        public string JobId { get; set; }

        public int ProcessedCount { get; set; }

        public int TotalCount { get; set; }

        public int CreatedCount { get; set; }

        public int UpdatedCount { get; set; }

        public int ErrorCount { get; set; }

        public ICollection<string> Errors { get; set; }

        public DateTime? Finished { get; set; }

        public string ReportUrl { get; set; }
    }
}
