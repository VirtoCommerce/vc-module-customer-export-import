using System;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerExportImportModule.Core.Services;
using VirtoCommerce.CustomerExportImportModule.Data.Helpers;
using VirtoCommerce.Platform.Core.Exceptions;
using VirtoCommerce.Platform.Core.PushNotifications;
using VirtoCommerce.Platform.Hangfire;

namespace VirtoCommerce.CustomerExportImportModule.Web.BackgroundJobs
{
    public sealed class ImportContactsJob
    {
        private readonly IPushNotificationManager _pushNotificationManager;
        private readonly ICsvPagedCustomerDataImporter _customerDataImporter;

        public ImportContactsJob(IPushNotificationManager pushNotificationManager, ICsvPagedCustomerDataImporter customerDataImporter)
        {
            _pushNotificationManager = pushNotificationManager;
            _customerDataImporter = customerDataImporter;
        }

        public async Task ImportBackgroundAsync(ImportDataRequest request, ImportPushNotification pushNotification, IJobCancellationToken jobCancellationToken, PerformContext context)
        {
            ValidateParameters(pushNotification);

            try
            {
                await _customerDataImporter.ImportAsync(request,
                    progressInfo => ProgressCallback(progressInfo, pushNotification, context),
                    new JobCancellationTokenWrapper(jobCancellationToken));
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


        private void ProgressCallback(ImportProgressInfo x, ImportPushNotification pushNotification, PerformContext context)
        {
            pushNotification.Patch(x);
            pushNotification.JobId = context.BackgroundJob.Id;
            _pushNotificationManager.Send(pushNotification);
        }

        private static void ValidateParameters(ImportPushNotification pushNotification)
        {
            if (pushNotification == null)
            {
                throw new ArgumentNullException(nameof(pushNotification));
            }
        }
    }
}
