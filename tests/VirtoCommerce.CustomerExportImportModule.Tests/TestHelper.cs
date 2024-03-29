using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Moq;
using VirtoCommerce.AssetsModule.Core.Assets;
using VirtoCommerce.CustomerExportImportModule.Data.Services;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.CustomerExportImportModule.Tests
{
    public static class TestHelper
    {
        public static IBlobStorageProvider GetBlobStorageProvider(string csv, MemoryStream errorReporterMemoryStream = null)
        {
            errorReporterMemoryStream ??= new MemoryStream();
            var blobStorageProviderMock = new Mock<IBlobStorageProvider>();
            var stream = GetStream(csv);
            blobStorageProviderMock.Setup(x => x.OpenRead(It.IsAny<string>())).Returns(() => stream);
            blobStorageProviderMock.Setup(x => x.OpenWrite(It.IsAny<string>())).Returns(() => errorReporterMemoryStream);
            blobStorageProviderMock.Setup(x => x.GetBlobInfoAsync(It.IsAny<string>()))
                .Returns(() => Task.FromResult(new BlobInfo { Size = stream.Length }));
            return blobStorageProviderMock.Object;
        }

        public static IDynamicPropertySearchService GetDynamicPropertySearchService(IList<DynamicProperty> dynamicProperties)
        {
            var dynamicPropertySearchServiceMock = new Mock<IDynamicPropertySearchService>();
            dynamicPropertySearchServiceMock.Setup(x => x.SearchAsync(It.IsAny<DynamicPropertySearchCriteria>(), It.IsAny<bool>()))
                .Returns(Task.FromResult(new DynamicPropertySearchResult
                {
                    Results = dynamicProperties,
                    TotalCount = dynamicProperties.Count
                }));
            return dynamicPropertySearchServiceMock.Object;
        }
        public static IDynamicPropertyDictionaryItemsSearchService GetDynamicPropertyDictionaryItemsSearchService(IList<DynamicProperty> dynamicProperties, Dictionary<string, IList<DynamicPropertyDictionaryItem>> dictionaryItems)
        {
            var dynamicPropertySearchServiceMock = new Mock<IDynamicPropertyDictionaryItemsSearchService>();
            dynamicPropertySearchServiceMock.Setup(x => x.SearchAsync(It.IsAny<DynamicPropertyDictionaryItemSearchCriteria>(), It.IsAny<bool>()))
                .Returns<DynamicPropertyDictionaryItemSearchCriteria, bool>((criteria, clone) =>
                {
                    var results = dictionaryItems.ContainsKey(criteria.PropertyId) ? dictionaryItems[criteria.PropertyId] : new List<DynamicPropertyDictionaryItem>();
                    return Task.FromResult(new DynamicPropertyDictionaryItemSearchResult { Results = results, TotalCount = results.Count });
                });
            return dynamicPropertySearchServiceMock.Object;
        }

        public static CustomerImportPagedDataSourceFactory GetCustomerImportPagedDataSourceFactory(IBlobStorageProvider blobStorageProvider, IDynamicPropertySearchService dynamicPropertySearchService,
            IDynamicPropertyDictionaryItemsSearchService dynamicPropertyDictionaryItemsSearchService)
        {
            return new CustomerImportPagedDataSourceFactory(blobStorageProvider, dynamicPropertySearchService, dynamicPropertyDictionaryItemsSearchService);
        }

        public static Stream GetStream(string csv)
        {
            var stream = new MemoryStream();
            using var writer = new StreamWriter(stream, leaveOpen: true);
            writer.Write(csv);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static string GetCsv(IEnumerable<string> records, string header = null)
        {
            var csv = "";

            if (header != null)
            {
                csv += header + "\r\n";
            }

            return records.Aggregate(csv, (current, record) => current + record + "\r\n");
        }

        public static IEnumerable<PropertyInfo> GetProperties<T>(T obj)
        {
            return obj.GetType()
                .GetTypeInfo()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.Name != nameof(ApplicationUser.SecurityStamp) && p.Name != nameof(ApplicationUser.ConcurrencyStamp))
                .OrderBy(p => p.Name)
                .ToList();
        }

        public static string ToString<T>(T obj)
        {
            var propertiesAndValues = GetProperties(obj).Select(property =>
            {
                var value = property.GetValue(obj);
                return $"{property.Name}: {(value is IEnumerable<object> enumerable ? $"[{string.Join(", ", enumerable.Select(x => x.ToString()))}]" : value)}";
            });
            return $"{{{string.Join(", ", propertiesAndValues)}}}";
        }
    }
}
