using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper.Configuration;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerExportImportModule.Core.Services;
using VirtoCommerce.CustomerExportImportModule.Data.ExportImport;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.Platform.Core.DynamicProperties;

namespace VirtoCommerce.CustomerExportImportModule.Data.Services
{
    public sealed class CustomerImportPagedDataSourceFactory : ICustomerImportPagedDataSourceFactory
    {
        private readonly IBlobStorageProvider _blobStorageProvider;
        private readonly IDynamicPropertySearchService _dynamicPropertySearchService;

        public CustomerImportPagedDataSourceFactory(IBlobStorageProvider blobStorageProvider, IDynamicPropertySearchService dynamicPropertySearchService)
        {
            _blobStorageProvider = blobStorageProvider;
            _dynamicPropertySearchService = dynamicPropertySearchService;
        }

        public async Task<ICustomerImportPagedDataSource> CreateAsync(string filePath, int pageSize, Configuration configuration = null)
        {
            var dynamicProperties = await _dynamicPropertySearchService.SearchDynamicPropertiesAsync(new DynamicPropertySearchCriteria()
            {
                ObjectTypes = new List<string>() { typeof(Contact).FullName },
                Skip = 0,
                Take = int.MaxValue
            });
            var dynamicPropertyNames = dynamicProperties.Results.Select(x => x.Name).ToArray();

            configuration ??= new ImportConfiguration();
            configuration.RegisterClassMap(new GenericClassMap<CsvContact>(dynamicPropertyNames));
            return new CustomerImportPagedDataSource(filePath, _blobStorageProvider, pageSize, configuration);
        }
    }
}
