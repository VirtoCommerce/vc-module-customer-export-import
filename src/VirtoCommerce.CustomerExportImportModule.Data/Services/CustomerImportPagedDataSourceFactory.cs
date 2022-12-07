using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper.Configuration;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerExportImportModule.Core.Services;
using VirtoCommerce.CustomerExportImportModule.Data.ExportImport;
using VirtoCommerce.AssetsModule.Core.Assets;
using VirtoCommerce.Platform.Core.DynamicProperties;

namespace VirtoCommerce.CustomerExportImportModule.Data.Services
{
    public sealed class CustomerImportPagedDataSourceFactory : ICustomerImportPagedDataSourceFactory
    {
        private readonly IBlobStorageProvider _blobStorageProvider;
        private readonly IDynamicPropertySearchService _dynamicPropertySearchService;
        private readonly IDynamicPropertyDictionaryItemsSearchService _dynamicPropertyDictionaryItemsSearchService;

        public CustomerImportPagedDataSourceFactory(IBlobStorageProvider blobStorageProvider, IDynamicPropertySearchService dynamicPropertySearchService, IDynamicPropertyDictionaryItemsSearchService dynamicPropertyDictionaryItemsSearchService)
        {
            _blobStorageProvider = blobStorageProvider;
            _dynamicPropertySearchService = dynamicPropertySearchService;
            _dynamicPropertyDictionaryItemsSearchService = dynamicPropertyDictionaryItemsSearchService;
        }

        public async Task<ICustomerImportPagedDataSource<TCsvCustomer>> CreateAsync<TCsvCustomer, TCustomer>(string filePath, int pageSize, CsvConfiguration configuration = null) where TCsvCustomer : CsvMember
        {
            var dynamicPropertiesSearchResult = await _dynamicPropertySearchService.SearchDynamicPropertiesAsync(new DynamicPropertySearchCriteria
            {
                ObjectTypes = new List<string> { typeof(TCustomer).FullName },
                Skip = 0,
                Take = int.MaxValue
            });
            var dynamicProperties = dynamicPropertiesSearchResult.Results;

            var dynamicPropertyDictionaryItems = new Dictionary<string, IList<DynamicPropertyDictionaryItem>>();
            foreach (var dynamicProperty in dynamicProperties.Where(dynamicProperty => dynamicProperty.IsDictionary))
            {
                var dynamicPropertyDictionaryItemsSearchResult =
                    await _dynamicPropertyDictionaryItemsSearchService.SearchDictionaryItemsAsync(new DynamicPropertyDictionaryItemSearchCriteria { PropertyId = dynamicProperty.Id });
                dynamicPropertyDictionaryItems.Add(dynamicProperty.Id, dynamicPropertyDictionaryItemsSearchResult.Results);
            }

            configuration ??= new ImportConfiguration();
            return new CustomerImportPagedDataSource<TCsvCustomer>(filePath, _blobStorageProvider, pageSize, configuration, new GenericClassMap<TCsvCustomer>(dynamicProperties, dynamicPropertyDictionaryItems));
        }
    }
}
