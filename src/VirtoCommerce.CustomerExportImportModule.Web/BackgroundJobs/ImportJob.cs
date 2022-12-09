using System;
using System.Collections.Generic;
using System.Linq;
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
    public sealed class ImportJob
    {
        private readonly IPushNotificationManager _pushNotificationManager;
        private readonly IEnumerable<ICsvPagedCustomerDataImporter> _customerDataImporters;

        public ImportJob(IPushNotificationManager pushNotificationManager, IEnumerable<ICsvPagedCustomerDataImporter> customerDataImporters)
        {
            _pushNotificationManager = pushNotificationManager;
            _customerDataImporters = customerDataImporters;
        }

        public async Task ImportBackgroundAsync(ImportDataRequest request, ImportPushNotification pushNotification, IJobCancellationToken jobCancellationToken, PerformContext context)
        {
            ValidateParameters(request);

            try
            {
                var importer = _customerDataImporters.First(x => x.MemberType == request.MemberType);

                await importer.ImportAsync(request,
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
                pushNotification.Description = "Import finished";
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

        private void ValidateParameters(ImportDataRequest request)
        {
            var importer = _customerDataImporters.FirstOrDefault(x => x.MemberType == request.MemberType);

            if (importer == null)
            {
                throw new ArgumentException($"Not allowed argument value in field {nameof(request.MemberType)}", nameof(request));
            }
        }
    }
}
