using System;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerExportImportModule.Core.Services;
using VirtoCommerce.CustomerExportImportModule.Data.Helpers;
using VirtoCommerce.Platform.Core.Exceptions;
using VirtoCommerce.Platform.Core.Jobs;
using VirtoCommerce.Platform.Core.PushNotifications;

namespace VirtoCommerce.CustomerExportImportModule.Web.BackgroundJobs
{
    public class ExportJobPayload
    {
        public ExportDataRequest Request { get; set; }
        public ExportPushNotification Notification { get; set; }
    }

    /// <summary>
    /// Engine-agnostic customer export job. Replaces the former Hangfire job that used <c>PerformContext</c>: the job id
    /// now comes from <see cref="IJobExecutionContext.JobId"/> and cancellation from the handler's <see cref="CancellationToken"/>.
    /// </summary>
    public sealed class ExportJob : IBackgroundJobHandler<ExportJobPayload>
    {
        private readonly IPushNotificationManager _pushNotificationManager;
        private readonly ICustomerDataExporter _customerDataExporter;

        public ExportJob(IPushNotificationManager pushNotificationManager, ICustomerDataExporter customerDataExporter)
        {
            _pushNotificationManager = pushNotificationManager;
            _customerDataExporter = customerDataExporter;
        }

        public async Task Execute(ExportJobPayload payload, IJobExecutionContext context, CancellationToken cancellationToken = default)
        {
            var request = payload.Request;
            var pushNotification = payload.Notification;

            ValidateParameters(pushNotification);

            try
            {
                await _customerDataExporter.ExportAsync(request,
                    progressInfo => ProgressCallback(progressInfo, pushNotification, context),
                    cancellationToken);
            }
            catch (OperationCanceledException)
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


        private void ProgressCallback(ExportProgressInfo x, ExportPushNotification pushNotification, IJobExecutionContext context)
        {
            pushNotification.Patch(x);
            pushNotification.JobId = context.JobId;
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
