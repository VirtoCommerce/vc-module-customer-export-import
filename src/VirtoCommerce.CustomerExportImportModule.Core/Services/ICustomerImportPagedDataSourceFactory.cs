using CsvHelper.Configuration;

namespace VirtoCommerce.CustomerExportImportModule.Core.Services
{
    public interface ICustomerImportPagedDataSourceFactory
    {
        ICustomerImportPagedDataSource Create(string filePath, int pageSize, Configuration configuration = null);
    }
}
