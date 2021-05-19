using System.Threading.Tasks;
using VirtoCommerce.CustomerModule.Core.Model;

namespace VirtoCommerce.CustomerExportImportModule.Core.Services
{
    public interface ICustomerExportPagedDataSource
    {
        int CurrentPageNumber { get; }

        int PageSize { get; }

        Task<int> GetTotalCountAsync();

        Task<bool> FetchAsync();

        Member[] Items { get; }
    }
}
