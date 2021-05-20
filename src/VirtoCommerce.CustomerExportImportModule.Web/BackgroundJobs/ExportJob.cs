using System;
using System.Threading.Tasks;
using Hangfire;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerExportImportModule.Core.Services;
using VirtoCommerce.Platform.Core.PushNotifications;

namespace VirtoCommerce.CustomerExportImportModule.Web.BackgroundJobs
{
    public class ExportJob
    {
        private readonly IPushNotificationManager _pushNotificationManager;
        private readonly ICustomerDataExporter _customerDataExporter;

        public ExportJob(IPushNotificationManager pushNotificationManager, ICustomerDataExporter customerDataExporter)
        {
            _pushNotificationManager = pushNotificationManager;
            _customerDataExporter = customerDataExporter;
        }

        public async Task ExportBackgroundAsync(ExportDataRequest request, ExportPushNotification notification, IJobCancellationToken @null, object p)
        {
            throw new NotImplementedException();
        }
    }
}
