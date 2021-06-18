using System.Threading.Tasks;
using CsvHelper.Configuration;
using VirtoCommerce.CustomerExportImportModule.Core.Models;

namespace VirtoCommerce.CustomerExportImportModule.Core.Services
{
    public interface ICustomerImportPagedDataSourceFactory
    {
        Task<ICustomerImportPagedDataSource<TCsvCustomer>> CreateAsync<TCsvCustomer, TCustomer>(string filePath, int pageSize, Configuration configuration = null) where TCsvCustomer : CsvMember;
    }
}
