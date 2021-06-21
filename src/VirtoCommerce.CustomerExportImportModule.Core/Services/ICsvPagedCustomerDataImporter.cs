using System;
using System.Threading.Tasks;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CustomerExportImportModule.Core.Services
{
    public interface ICsvPagedCustomerDataImporter
    {
        string MemberType { get; }

        Task ImportAsync(ImportDataRequest request, Action<ImportProgressInfo> progressCallback, ICancellationToken cancellationToken);
    }
}
