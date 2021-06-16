using System.Threading.Tasks;
using CsvHelper.Configuration;
using VirtoCommerce.CustomerExportImportModule.Core.Models;

namespace VirtoCommerce.CustomerExportImportModule.Core.Services
{
    public interface ICustomerImportPagedDataSourceFactory
    {
        Task<ICustomerImportPagedDataSource> CreateAsync(string dataType, string filePath, int pageSize, Configuration configuration = null);
        Task<ICustomerImportPagedDataSource> CreateAsync<TCsvCustomer, TCustomer>(string filePath, int pageSize, Configuration configuration = null) where TCsvCustomer : CsvMember;
    }
}
