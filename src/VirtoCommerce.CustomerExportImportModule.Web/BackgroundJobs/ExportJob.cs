using System;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerExportImportModule.Core.Services;
using VirtoCommerce.CustomerExportImportModule.Data.Helpers;
using VirtoCommerce.Platform.Core.Exceptions;
using VirtoCommerce.Platform.Core.PushNotifications;

namespace VirtoCommerce.CustomerExportImportModule.Web.BackgroundJobs
{
    public sealed class ExportJob
    {
        private readonly IPushNotificationManager _pushNotificationManager;
        private readonly ICustomerDataExporter _customerDataExporter;

        public ExportJob(IPushNotificationManager pushNotificationManager, ICustomerDataExporter customerDataExporter)
        {
            _pushNotificationManager = pushNotificationManager;
            _customerDataExporter = customerDataExporter;
        }

        public async Task ExportBackgroundAsync(ExportDataRequest request, ExportPushNotification pushNotification, CancellationToken cancellationToken, PerformContext context)
        {
            ValidateParameters(pushNotification);

            try
            {
                await _customerDataExporter.ExportAsync(request,
                    progressInfo => ProgressCallback(progressInfo, pushNotification, context),
                    cancellationToken);
            }
            catch (JobAbortedException)
            {
                // job is aborted, do nothing
            }
            catch (Exception ex)
            {
                pushNotification.Errors.Add(ex.ExpandExceptionMessage());
            }
            finally
            {
                pushNotification.Description = "Export finished";
                pushNotification.Finished = DateTime.UtcNow;

                await _pushNotificationManager.SendAsync(pushNotification);
            }
        }


        private void ProgressCallback(ExportProgressInfo x, ExportPushNotification pushNotification, PerformContext context)
        {
            pushNotification.Patch(x);
            pushNotification.JobId = context.BackgroundJob.Id;
            _pushNotificationManager.Send(pushNotification);
        }

        private static void ValidateParameters(ExportPushNotification pushNotification)
        {
            if (pushNotification == null)
            {
                throw new ArgumentNullException(nameof(pushNotification));
            }
        }
    }
}
