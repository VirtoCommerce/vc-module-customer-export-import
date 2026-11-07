using System;
using System.Collections.Generic;
using System.Linq;
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
    public class ImportJobPayload
    {
        public ImportDataRequest Request { get; set; }
        public ImportPushNotification Notification { get; set; }
    }

    /// <summary>
    /// Engine-agnostic customer import job. Replaces the former Hangfire job that used <c>PerformContext</c>: the job id
    /// now comes from <see cref="IJobExecutionContext.JobId"/> and cancellation from the handler's <see cref="CancellationToken"/>.
    /// </summary>
    public sealed class ImportJob : IBackgroundJobHandler<ImportJobPayload>
    {
        private readonly IPushNotificationManager _pushNotificationManager;
        private readonly IEnumerable<ICsvPagedCustomerDataImporter> _customerDataImporters;

        public ImportJob(IPushNotificationManager pushNotificationManager, IEnumerable<ICsvPagedCustomerDataImporter> customerDataImporters)
        {
            _pushNotificationManager = pushNotificationManager;
            _customerDataImporters = customerDataImporters;
        }

        public async Task Execute(ImportJobPayload payload, IJobExecutionContext context, CancellationToken cancellationToken = default)
        {
            var request = payload.Request;
            var pushNotification = payload.Notification;

            ValidateParameters(request);

            try
            {
                var importer = _customerDataImporters.First(x => x.MemberType == request.MemberType);

                await importer.ImportAsync(request,
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
                pushNotification.Description = "Import finished";
                pushNotification.Finished = DateTime.UtcNow;

                await _pushNotificationManager.SendAsync(pushNotification);
            }
        }


        private void ProgressCallback(ImportProgressInfo x, ImportPushNotification pushNotification, IJobExecutionContext context)
        {
            pushNotification.Patch(x);
            pushNotification.JobId = context.JobId;
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
