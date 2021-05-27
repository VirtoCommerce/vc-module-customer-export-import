using System;
using System.Threading.Tasks;
using VirtoCommerce.CustomerExportImportModule.Core.Models;

namespace VirtoCommerce.CustomerExportImportModule.Core.Services
{
    public interface ICustomerImportPagedDataSource : IDisposable
    {
        int CurrentPageNumber { get; }

        int PageSize { get; }

        int GetTotalCount();

        Task<bool> FetchAsync();

        ImportRecord<CsvContact>[] Items { get; }
    }
}
