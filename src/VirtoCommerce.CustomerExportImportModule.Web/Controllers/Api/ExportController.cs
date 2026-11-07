using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.CustomerExportImportModule.Core;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerExportImportModule.Web.BackgroundJobs;
using VirtoCommerce.Platform.Core.Jobs;
using VirtoCommerce.Platform.Core.PushNotifications;
using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.CustomerExportExportModule.Web.Controllers.Api
{
    [Route("api/customer/export")]
    [Authorize(ModuleConstants.Security.Permissions.ExportAccess)]
    [ApiController]
    public class ExportController : ControllerBase
    {
        private readonly IUserNameResolver _userNameResolver;
        private readonly IPushNotificationManager _pushNotificationManager;

        public ExportController(IUserNameResolver userNameResolver, IPushNotificationManager pushNotificationManager)
        {
            _userNameResolver = userNameResolver;
            _pushNotificationManager = pushNotificationManager;
        }

        [HttpPost]
        [Route("run")]
        public async Task<ActionResult<ExportPushNotification>> RunExport([FromBody] ExportDataRequest request)
        {
            var notification = new ExportPushNotification(_userNameResolver.GetCurrentUserName())
            {
                Title = "Customers export",
                Description = "Starting export task..."
            };

            await _pushNotificationManager.SendAsync(notification);

            // The static facade rather than an injected IBackgroundJob keeps the controller constructor unchanged.
            notification.JobId = await BackgroundJob.Enqueue<ExportJob>(new ExportJobPayload { Request = request, Notification = notification });

            return Ok(notification);
        }

        [HttpPost]
        [Route("cancel")]
        public async Task<ActionResult> CancelExport([FromBody] ExportCancellationRequest cancellationRequest)
        {
            await BackgroundJob.Cancel(cancellationRequest.JobId);
            return Ok();
        }
    }
}
