using System;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.CustomerExportImportModule.Core.Models;

namespace VirtoCommerce.CustomerExportImportModule.Core.Services
{
    public interface ICsvPagedCustomerDataImporter
    {
        string MemberType { get; }

        Task ImportAsync(ImportDataRequest request, Action<ImportProgressInfo> progressCallback, CancellationToken cancellationToken);
    }
}
