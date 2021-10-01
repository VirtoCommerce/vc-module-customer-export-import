using VirtoCommerce.CustomerExportImportModule.Core.Models;

namespace VirtoCommerce.CustomerExportImportModule.Data.Helpers
{
    public static class ImportPushNotificationExtensions
    {
        public static void Patch(this ImportPushNotification target, ImportProgressInfo source)
        {
            target.Description = source.Description;
            target.ProcessedCount = source.ProcessedCount;
            target.TotalCount = source.TotalCount;
            target.CreatedCount = source.CreatedCount;
            target.UpdatedCount = source.UpdatedCount;
            target.ErrorCount = source.ErrorCount;
            target.Errors = source.Errors;
            target.ReportUrl = source.ReportUrl;
        }
    }
}
