using System.Threading.Tasks;
using CsvHelper.Configuration;

namespace VirtoCommerce.CustomerExportImportModule.Core.Services
{
    public interface ICustomerImportPagedDataSourceFactory<T>
    {
        Task<ICustomerImportPagedDataSource<T>> CreateAsync(string filePath, int pageSize, Configuration configuration = null);
    }
}
