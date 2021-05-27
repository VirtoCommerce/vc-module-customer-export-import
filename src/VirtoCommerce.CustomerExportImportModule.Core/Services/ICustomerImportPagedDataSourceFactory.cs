using System.Threading.Tasks;
using CsvHelper.Configuration;

namespace VirtoCommerce.CustomerExportImportModule.Core.Services
{
    public interface ICustomerImportPagedDataSourceFactory
    {
        Task<ICustomerImportPagedDataSource> CreateAsync(string filePath, int pageSize, Configuration configuration = null);
    }
}
