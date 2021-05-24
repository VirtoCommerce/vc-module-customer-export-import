using System.Threading.Tasks;
using VirtoCommerce.CustomerExportImportModule.Core.Models;

namespace VirtoCommerce.CustomerExportImportModule.Core.Services
{
    public interface ICustomerExportPagedDataSource
    {
        int CurrentPageNumber { get; }

        int PageSize { get; }

        Task<int> GetTotalCountAsync();

        Task<bool> FetchAsync();

        IExportable[] Items { get; }
    }
}
