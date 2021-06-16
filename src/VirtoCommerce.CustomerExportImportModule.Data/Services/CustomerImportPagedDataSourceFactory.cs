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
    public sealed class CustomerImportPagedDataSourceFactory<T> : ICustomerImportPagedDataSourceFactory<T>
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

        public async Task<ICustomerImportPagedDataSource<T>> CreateAsync(string filePath, int pageSize, Configuration configuration = null)
        {
            var dynamicPropertiesSearchResult = await _dynamicPropertySearchService.SearchDynamicPropertiesAsync(new DynamicPropertySearchCriteria()
            {
                ObjectTypes = new List<string> { typeof(Contact).FullName },
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
            configuration.RegisterClassMap(new GenericClassMap<T>(dynamicProperties, dynamicPropertyDictionaryItems));
            return new CustomerImportPagedDataSource<T>(filePath, _blobStorageProvider, pageSize, configuration);
        }
    }
}
