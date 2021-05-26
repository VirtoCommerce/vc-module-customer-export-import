using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using Xunit;

namespace VirtoCommerce.CustomerExportImportModule.Tests
{
    [Trait("Category", "CI")]
    public class CustomerImportPagedDataSourceTests
    {
        private const string CsvFileName = "file.csv";
        private const string CsvHeader = "Contact Id;Contact First Name;Contact Last Name;Contact Full Name;Contact Outer Id;Organization Id;Organization Outer Id;Organization Name;Account Id;Account Login;Store Id;Store Name;Account Email;Account Type;Account Status;Email Verified;Contact Status;Associated Organization Ids;Birthday;TimeZone;Phones;User groups;Default language;Taxpayer ID;Preferred communication;Preferred delivery;Address Type;Address First Name;Address Last Name;Address Country;Address Region;Address City;Address Address Line1;Address Address Line2;Address Zip Code;Address Email;Address Phone";
        private static readonly string[] CsvRecords =
        {
            "550f91d0-99d3-4371-9fc2-edc1633f32fc;Testb2b;b2b;Testb2b b2b;;d690f3df-8782-4dcc-99be-a1f644220e50;;b2b test organization;;;;;;;;;Approved;;;;;tag1, tag2, tag3, Wholesaler;;;;;;;;;;;;;;;;",
            "ebbd6275-53fb-407a-83cb-4f3024d963b9;True;Boroda;True Boroda;;4e8db000-fb75-4250-8118-b00a0ccf5115;;true.boroda;4771b4e2-d8fe-40a4-9498-a55760acafb7;true.boroda;B2B-store;B2B-store;true.boroda@yandex.ru;Customer;;False;New;;;;;tag1, tag2;;;;;BillingAndShipping;True;Boroda;Russia;Kirov Oblast;Kirov;Moscow st 110 b/1;169;;true.boroda@yandex.ru;89229252501;",
            "5f807280-bb1a-42b2-9a96-ed107269ea06;;;Sam Green;;;;;5f807280-bb1a-42b2-9a96-ed107269ea06;goal44@example.com;Electronics;Electronics;goal44@example.com;Customer;;False;;;;;;;;;;;;;;;;;;;;;;"
        };

        [Theory]
        [MemberData(nameof(GetCsvWithAndWithoutHeader))]
        public void GetTotalCount_Calculate_AndReturnTotalCount(string[] records, string header)
        {
            // Arrange
            var csv = TestHelper.GetCsv(records, header);
            var blobStorageProvider = TestHelper.GetBlobStorageProvider(csv);
            var customerImportPagedDataSourceFactory = TestHelper.GetCustomerImportPagedDataSourceFactory(blobStorageProvider);
            using var customerImportPagedDataSource = customerImportPagedDataSourceFactory.Create(CsvFileName, 10);

            // Act
            var totalCount = customerImportPagedDataSource.GetTotalCount();

            // Assert
            Assert.Equal(3, totalCount);
        }

        [Fact]
        public void GetTotalCount_CacheTotalCount_AndReturnSameValue()
        {
            // Arrange
            var csv = TestHelper.GetCsv(CsvRecords, CsvHeader);
            var blobStorageProvider = TestHelper.GetBlobStorageProvider(csv);
            var customerImportPagedDataSourceFactory = TestHelper.GetCustomerImportPagedDataSourceFactory(blobStorageProvider);
            using var customerImportPagedDataSource = customerImportPagedDataSourceFactory.Create(CsvFileName, 10);

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
                var customerImportPagedDataSourceFactory = TestHelper.GetCustomerImportPagedDataSourceFactory(blobStorageProvider);
                using var customerImportPagedDataSource = customerImportPagedDataSourceFactory.Create(CsvFileName, 10);

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
            var customerImportPagedDataSourceFactory = TestHelper.GetCustomerImportPagedDataSourceFactory(blobStorageProvider);
            using var customerImportPagedDataSource = customerImportPagedDataSourceFactory.Create(CsvFileName, 1);

            // Act
            await customerImportPagedDataSource.FetchAsync();
            await customerImportPagedDataSource.FetchAsync();

            // Assert
            Assert.Equal("ebbd6275-53fb-407a-83cb-4f3024d963b9", customerImportPagedDataSource.Contacts.Single().Id);
        }

        [Fact]
        public async Task FetchAsync_AfterGetTotalCount_WillStartReadingFromTheSamePosition()
        {
            // Arrange
            var csv = TestHelper.GetCsv(CsvRecords, CsvHeader);
            var blobStorageProvider = TestHelper.GetBlobStorageProvider(csv);
            var customerImportPagedDataSourceFactory = TestHelper.GetCustomerImportPagedDataSourceFactory(blobStorageProvider);
            using var customerImportPagedDataSource = customerImportPagedDataSourceFactory.Create(CsvFileName, 1);

            // Act
            await customerImportPagedDataSource.FetchAsync();
            customerImportPagedDataSource.GetTotalCount();
            await customerImportPagedDataSource.FetchAsync();

            // Assert
            Assert.Equal("ebbd6275-53fb-407a-83cb-4f3024d963b9", customerImportPagedDataSource.Contacts.Single().Id);
        }

        [Fact]
        public async Task FetchAsync_BeforeEndOfCsvFile_WillReturnTrue()
        {
            // Arrange
            var csv = TestHelper.GetCsv(CsvRecords, CsvHeader);
            var blobStorageProvider = TestHelper.GetBlobStorageProvider(csv);
            var customerImportPagedDataSourceFactory = TestHelper.GetCustomerImportPagedDataSourceFactory(blobStorageProvider);
            using var customerImportPagedDataSource = customerImportPagedDataSourceFactory.Create(CsvFileName, 1);

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
            var customerImportPagedDataSourceFactory = TestHelper.GetCustomerImportPagedDataSourceFactory(blobStorageProvider);
            using var customerImportPagedDataSource = customerImportPagedDataSourceFactory.Create(CsvFileName, 10);

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
            var customerImportPagedDataSourceFactory = TestHelper.GetCustomerImportPagedDataSourceFactory(blobStorageProvider);
            using var customerImportPagedDataSource = customerImportPagedDataSourceFactory.Create(CsvFileName, 10);

            // Act
            await customerImportPagedDataSource.FetchAsync();
            await customerImportPagedDataSource.FetchAsync();

            // Assert
            Assert.Empty(customerImportPagedDataSource.Contacts);
        }
    }
}
