using VirtoCommerce.CustomerExportImportModule.Core.Models;

namespace VirtoCommerce.CustomerExportImportModule.Data.Helpers
{
    public static class ExportPushNotificationExtensions
    {
        public static void Patch(this ExportPushNotification target, ExportProgressInfo source)
        {
            target.Description = source.Description;
            target.ProcessedCount = source.ProcessedCount;
            target.TotalCount = source.TotalCount;
        }
    }
}
