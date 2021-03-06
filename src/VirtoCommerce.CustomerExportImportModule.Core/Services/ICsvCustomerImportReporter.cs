using System;
using System.Threading.Tasks;
using VirtoCommerce.CustomerExportImportModule.Core.Models;

namespace VirtoCommerce.CustomerExportImportModule.Core.Services
{
    public interface ICsvCustomerImportReporter : IAsyncDisposable
    {
        string FilePath { get; }
        bool ReportIsNotEmpty { get; }
        Task WriteHeaderAsync(string header);
        Task WriteAsync(ImportError error);
    }
}
