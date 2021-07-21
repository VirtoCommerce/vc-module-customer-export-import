using System;
using System.Threading.Tasks;
using VirtoCommerce.CustomerExportImportModule.Core.Models;

namespace VirtoCommerce.CustomerExportImportModule.Core.Services
{
    public interface ICustomerImportPagedDataSource<T> : IDisposable where T : CsvMember
    {
        int CurrentPageNumber { get; }

        int PageSize { get; }

        string GetHeaderRaw();

        int GetTotalCount();

        Task<bool> FetchAsync();

        ImportRecord<T>[] Items { get; }
    }
}
