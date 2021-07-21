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
            target.ContactsCreated = source.ContactsCreated;
            target.ContactsUpdated = source.ContactsUpdated;
            target.ErrorCount = source.ErrorCount;
            target.Errors = source.Errors;
            target.ReportUrl = source.ReportUrl;
        }
    }
}
