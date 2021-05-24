using VirtoCommerce.CustomerExportImportModule.Core.Models;

namespace VirtoCommerce.CustomerExportImportModule.Core.Services
{
    public interface ICustomerExportPagedDataSourceFactory
    {
        ICustomerExportPagedDataSource Create(int pageSize, ExportDataRequest request);
    }
}
