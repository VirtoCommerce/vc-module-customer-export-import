using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using VirtoCommerce.Platform.Core.DynamicProperties;
using Xunit;

namespace VirtoCommerce.CustomerExportImportModule.Tests
{
    [Trait("Category", "CI")]
    public class CustomerImportPagedDataSourceTests
    {
        private const string CsvFileName = "file.csv";
        private const string CsvHeader = "Contact Id;Contact First Name;Contact Last Name;Contact Full Name;Contact Outer Id;Organization Id;Organization Outer Id;Organization Name;Account Id;Account Login;Store Id;Store Name;Account Email;Account Type;Account Status;Email Verified;Contact Status;Associated Organization Ids;Birthday;TimeZone;Phones;User groups;Salutation;Default language;Taxpayer ID;Preferred communication;Preferred delivery;Address Type;Address First Name;Address Last Name;Address Country;Address Region;Address City;Address Line1;Address Line2;Address Zip Code;Address Email;Address Phone;Default shipping address;language test property;Married;occupation;Sex;Test Property";
        private static readonly string[] CsvRecords =
        {
            "47dadad1-8f87-4624-829f-ef45265ea81c;Test user;user;Test user user;;324c9a1a-b501-42de-a89a-e4a42265d12f;;My Test Organization;aec295f5-131a-4671-958f-a300f95f911f;b2b-user-2;B2B-store;B2B-store;qa@mail1.com;Customer;;False;;;;;;;;;;;;;;;;;;;;;;;;;;;;",
            "4c8a0c28-6aa4-4f1c-8e81-4df5f34b2065;Test user;user;Test user user;;d690f3df-8782-4dcc-99be-a1f644220e50;;b2b test organization;73e95cde-5fd1-4747-9ac6-82cc958d13d5;b2b-user-1;B2B-store;B2B-store;test@meem1234eme.test;Customer;;False;;;;;;TEST_GROUP;;;;;;BillingAndShipping;asdfasd;fadsf;United States;California;asgfq;7630 Bridge;1;90019;adafafafsd@avvava;1145145154;test;;True;Boat Builder;Female;test1",
            "82c3c68f-9e85-4a01-b6fa-8e9d8e6f5737;Test user;user;Test user user;;12cc7a94-5f8b-4b9e-ba0c-aff59916743d;;Business Test Organization;5e43c02e-3bba-4baf-b229-c8201a2a9975;b2b-user-test;B2B-store;B2B-store;test@meememe.test;Customer;;False;;;;;;;;;;;;BillingAndShipping;Test user;user;United States;Alabama;City 17;Street Address 5781;51;5814858;test@meememe.test;157471745;;Salut,Hallo,Hello;;;;"
        };

        private static readonly IList<DynamicProperty> DynamicProperties = new[] { "Default shipping address", "language test property", "Married", "occupation", "Sex", "Test Property" }
            .Select(dynamicPropertyName => new DynamicProperty { Name = dynamicPropertyName }).ToList();

        [Theory]
        [MemberData(nameof(GetCsvWithAndWithoutHeader))]
        public async Task GetTotalCount_Calculate_AndReturnTotalCount(string[] records, string header)
        {
            // Arrange
            var csv = TestHelper.GetCsv(records, header);
            var blobStorageProvider = TestHelper.GetBlobStorageProvider(csv);
            var dynamicPropertySearchService = TestHelper.GetDynamicPropertySearchService(DynamicProperties);
            var dynamicPropertyDictionaryItemsSearchService = TestHelper.GetDynamicPropertyDictionaryItemsSearchService(DynamicProperties, new Dictionary<string, IList<DynamicPropertyDictionaryItem>>());
            var customerImportPagedDataSourceFactory = TestHelper.GetCustomerImportPagedDataSourceFactory(blobStorageProvider, dynamicPropertySearchService, dynamicPropertyDictionaryItemsSearchService);
            using var customerImportPagedDataSource = await customerImportPagedDataSourceFactory.CreateAsync(CsvFileName, 10);

            // Act
            var totalCount = customerImportPagedDataSource.GetTotalCount();

            // Assert
            Assert.Equal(3, totalCount);
        }

        [Fact]
        public async Task GetTotalCount_CacheTotalCount_AndReturnSameValue()
        {
            // Arrange
            var csv = TestHelper.GetCsv(CsvRecords, CsvHeader);
            var blobStorageProvider = TestHelper.GetBlobStorageProvider(csv);
            var dynamicPropertySearchService = TestHelper.GetDynamicPropertySearchService(DynamicProperties);
            var dynamicPropertyDictionaryItemsSearchService = TestHelper.GetDynamicPropertyDictionaryItemsSearchService(DynamicProperties, new Dictionary<string, IList<DynamicPropertyDictionaryItem>>());
            var customerImportPagedDataSourceFactory = TestHelper.GetCustomerImportPagedDataSourceFactory(blobStorageProvider, dynamicPropertySearchService, dynamicPropertyDictionaryItemsSearchService);
            using var customerImportPagedDataSource = await customerImportPagedDataSourceFactory.CreateAsync(CsvFileName, 10);

            // Act
            customerImportPagedDataSource.GetTotalCount();
            var totalCount = customerImportPagedDataSource.GetTotalCount();

            // Assert
            Assert.Equal(3, totalCount);
        }

        public static IEnumerable<object[]> GetCsvWithAndWithoutHeader()
        {
            yield return new object[] { CsvRecords, CsvHeader };
            yield return new object[] { CsvRecords, null };
        }

        [Fact]
        public async Task FetchAsync_WithMissedHeader_ThrowsException()
        {
            static async Task FetchAsync()
            {
                // Arrange
                var csv = TestHelper.GetCsv(CsvRecords);
                var blobStorageProvider = TestHelper.GetBlobStorageProvider(csv);
                var dynamicPropertySearchService = TestHelper.GetDynamicPropertySearchService(DynamicProperties);
                var dynamicPropertyDictionaryItemsSearchService = TestHelper.GetDynamicPropertyDictionaryItemsSearchService(DynamicProperties, new Dictionary<string, IList<DynamicPropertyDictionaryItem>>());
                var customerImportPagedDataSourceFactory = TestHelper.GetCustomerImportPagedDataSourceFactory(blobStorageProvider, dynamicPropertySearchService, dynamicPropertyDictionaryItemsSearchService);
                using var customerImportPagedDataSource = await customerImportPagedDataSourceFactory.CreateAsync(CsvFileName, 10);

                // Act
                await customerImportPagedDataSource.FetchAsync();
            }

            // Assert
            await Assert.ThrowsAsync<HeaderValidationException>(FetchAsync);
        }

        [Fact]
        public async Task FetchAsync_WithSpecifiedPageSize_WillReturnSpecifiedNumberOfItems()
        {
            // Arrange
            var csv = TestHelper.GetCsv(CsvRecords, CsvHeader);
            var blobStorageProvider = TestHelper.GetBlobStorageProvider(csv);
            var dynamicPropertySearchService = TestHelper.GetDynamicPropertySearchService(DynamicProperties);
            var dynamicPropertyDictionaryItemsSearchService = TestHelper.GetDynamicPropertyDictionaryItemsSearchService(DynamicProperties, new Dictionary<string, IList<DynamicPropertyDictionaryItem>>());
            var customerImportPagedDataSourceFactory = TestHelper.GetCustomerImportPagedDataSourceFactory(blobStorageProvider, dynamicPropertySearchService, dynamicPropertyDictionaryItemsSearchService);
            using var customerImportPagedDataSource = await customerImportPagedDataSourceFactory.CreateAsync(CsvFileName, 1);

            // Act
            await customerImportPagedDataSource.FetchAsync();
            await customerImportPagedDataSource.FetchAsync();

            // Assert
            Assert.Equal("4c8a0c28-6aa4-4f1c-8e81-4df5f34b2065", customerImportPagedDataSource.Items.Single().Record.Id);
        }

        [Fact]
        public async Task FetchAsync_AfterGetTotalCount_WillStartReadingFromTheSamePosition()
        {
            // Arrange
            var csv = TestHelper.GetCsv(CsvRecords, CsvHeader);
            var blobStorageProvider = TestHelper.GetBlobStorageProvider(csv);
            var dynamicPropertySearchService = TestHelper.GetDynamicPropertySearchService(DynamicProperties);
            var dynamicPropertyDictionaryItemsSearchService = TestHelper.GetDynamicPropertyDictionaryItemsSearchService(DynamicProperties, new Dictionary<string, IList<DynamicPropertyDictionaryItem>>());
            var customerImportPagedDataSourceFactory = TestHelper.GetCustomerImportPagedDataSourceFactory(blobStorageProvider, dynamicPropertySearchService, dynamicPropertyDictionaryItemsSearchService);
            using var customerImportPagedDataSource = await customerImportPagedDataSourceFactory.CreateAsync(CsvFileName, 1);

            // Act
            await customerImportPagedDataSource.FetchAsync();
            customerImportPagedDataSource.GetTotalCount();
            await customerImportPagedDataSource.FetchAsync();

            // Assert
            Assert.Equal("4c8a0c28-6aa4-4f1c-8e81-4df5f34b2065", customerImportPagedDataSource.Items.Single().Record.Id);
        }

        [Fact]
        public async Task FetchAsync_BeforeEndOfCsvFile_WillReturnTrue()
        {
            // Arrange
            var csv = TestHelper.GetCsv(CsvRecords, CsvHeader);
            var blobStorageProvider = TestHelper.GetBlobStorageProvider(csv);
            var dynamicPropertySearchService = TestHelper.GetDynamicPropertySearchService(DynamicProperties);
            var dynamicPropertyDictionaryItemsSearchService = TestHelper.GetDynamicPropertyDictionaryItemsSearchService(DynamicProperties, new Dictionary<string, IList<DynamicPropertyDictionaryItem>>());
            var customerImportPagedDataSourceFactory = TestHelper.GetCustomerImportPagedDataSourceFactory(blobStorageProvider, dynamicPropertySearchService, dynamicPropertyDictionaryItemsSearchService);
            using var customerImportPagedDataSource = await customerImportPagedDataSourceFactory.CreateAsync(CsvFileName, 1);

            // Act
            var result = await customerImportPagedDataSource.FetchAsync();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task FetchAsync_AfterEndOfCsvFile_WillReturnFalse()
        {
            // Arrange
            var csv = TestHelper.GetCsv(CsvRecords, CsvHeader);
            var blobStorageProvider = TestHelper.GetBlobStorageProvider(csv);
            var dynamicPropertySearchService = TestHelper.GetDynamicPropertySearchService(DynamicProperties);
            var dynamicPropertyDictionaryItemsSearchService = TestHelper.GetDynamicPropertyDictionaryItemsSearchService(DynamicProperties, new Dictionary<string, IList<DynamicPropertyDictionaryItem>>());
            var customerImportPagedDataSourceFactory = TestHelper.GetCustomerImportPagedDataSourceFactory(blobStorageProvider, dynamicPropertySearchService, dynamicPropertyDictionaryItemsSearchService);
            using var customerImportPagedDataSource = await customerImportPagedDataSourceFactory.CreateAsync(CsvFileName, 10);

            // Act
            await customerImportPagedDataSource.FetchAsync();
            var result = await customerImportPagedDataSource.FetchAsync();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task FetchAsync_AfterEndOfCsvFile_WillFetchNoItems()
        {
            // Arrange
            var csv = TestHelper.GetCsv(CsvRecords, CsvHeader);
            var blobStorageProvider = TestHelper.GetBlobStorageProvider(csv);
            var dynamicPropertySearchService = TestHelper.GetDynamicPropertySearchService(DynamicProperties);
            var dynamicPropertyDictionaryItemsSearchService = TestHelper.GetDynamicPropertyDictionaryItemsSearchService(DynamicProperties, new Dictionary<string, IList<DynamicPropertyDictionaryItem>>());
            var customerImportPagedDataSourceFactory = TestHelper.GetCustomerImportPagedDataSourceFactory(blobStorageProvider, dynamicPropertySearchService, dynamicPropertyDictionaryItemsSearchService);
            using var customerImportPagedDataSource = await customerImportPagedDataSourceFactory.CreateAsync(CsvFileName, 10);

            // Act
            await customerImportPagedDataSource.FetchAsync();
            await customerImportPagedDataSource.FetchAsync();

            // Assert
            Assert.Empty(customerImportPagedDataSource.Items);
        }
    }
}
