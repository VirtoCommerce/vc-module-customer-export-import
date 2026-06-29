using System;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.CustomerExportImportModule.Core.Models;

namespace VirtoCommerce.CustomerExportImportModule.Core.Services
{
    public interface ICustomerDataExporter
    {
        Task ExportAsync(ExportDataRequest request, Action<ExportProgressInfo> progressCallback, CancellationToken cancellationToken);
    }
}
