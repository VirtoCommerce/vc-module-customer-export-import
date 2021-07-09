using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bogus;
using Moq;
using VirtoCommerce.CustomerExportImportModule.Core;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerExportImportModule.Data.Services;
using VirtoCommerce.Platform.Core.Assets;
using Xunit;

namespace VirtoCommerce.CustomerExportImportModule.Tests
{
    [Trait("Category", "CI")]
    public class CsvCustomerDataValidatorTests
    {
        private const string ContactDataType = "Contact";
        private const string OrganizationDataType = "Organization";

        public static IEnumerable<object[]> ValidCsvList
        {
            get
            {
                yield return new object[] { ContactDataType, GetValidContactsCsv() };
            }
        }

        private static string GetValidContactsCsv()
        {
            var faker = new Faker<ImportableContact>();
            var contacts = faker.Generate(5);

            var csv = TestHelper.GetCsvStringFromObjects(contacts);

            return csv;
        }

        [Theory]
        [InlineData(ContactDataType)]
        [InlineData(OrganizationDataType)]
        public async Task Validate_FileNotExists_ReturnErrorCode(string dataType)
        {
            // Arrange
            var blobStorageProviderMoq = new Mock<IBlobStorageProvider>();

            blobStorageProviderMoq.Setup(x => x.GetBlobInfoAsync(It.IsAny<string>()))
                .ReturnsAsync((BlobInfo)null);

            var settingsManagerMoq = TestHelper.GetSettingsManagerMoq();

            var validator = new CsvCustomerDataValidator(blobStorageProviderMoq.Object, settingsManagerMoq.Object);

            // Act
            var result = await validator.ValidateAsync(dataType, "file url");

            // Assert
            Assert.Single(result.Errors);
            Assert.True(result.Errors[0].ErrorCode == ModuleConstants.ValidationErrors.FileNotExisted);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("invalid_data_type")]
        public async Task Validate_InvalidDataType_ThrowArgumentException(string dataType)
        {
            // Arrange
            var blobStorageProviderMoq = new Mock<IBlobStorageProvider>();

            blobStorageProviderMoq.Setup(x => x.GetBlobInfoAsync(It.IsAny<string>()))
                .ReturnsAsync((BlobInfo)null);

            var settingsManagerMoq = TestHelper.GetSettingsManagerMoq();

            var validator = new CsvCustomerDataValidator(blobStorageProviderMoq.Object, settingsManagerMoq.Object);

            // Act Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
           {
               await validator.ValidateAsync(dataType, "file url");
           });
        }


        [Theory]
        [InlineData(ContactDataType)]
        [InlineData(OrganizationDataType)]
        public async Task Validate_FileWithLargeSize_ReturnErrorCode(string dataType)
        {
            // Arrange
            var blobStorageProviderMoq = new Mock<IBlobStorageProvider>();

            var blobInfo = new BlobInfo() { Size = (int)ModuleConstants.Settings.General.ImportFileMaxSize.DefaultValue * ModuleConstants.MByte + 1 };
            blobStorageProviderMoq.Setup(x => x.GetBlobInfoAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(blobInfo));

            var settingsManagerMoq = TestHelper.GetSettingsManagerMoq();

            var validator = new CsvCustomerDataValidator(blobStorageProviderMoq.Object, settingsManagerMoq.Object);

            // Act
            var result = await validator.ValidateAsync(dataType, "file url");

            // Assert
            Assert.Single(result.Errors);
            Assert.True(result.Errors[0].ErrorCode == ModuleConstants.ValidationErrors.ExceedingFileMaxSize);
        }

        [Theory]
        [MemberData(nameof(ValidCsvList))]

        public async Task Validate_FileWithoutError_ReturnEmptyErrors(string dataType, string csv)
        {
            // Arrange
            var blobStorageProviderMoq = GetBlobStorageProviderMoq();

            var stream = TestHelper.GetStream(csv);
            blobStorageProviderMoq.Setup(x => x.OpenRead(It.IsAny<string>()))
                .Returns(stream);

            var settingsManagerMoq = TestHelper.GetSettingsManagerMoq();

            var validator = new CsvCustomerDataValidator(blobStorageProviderMoq.Object, settingsManagerMoq.Object);

            // Act
            var result = await validator.ValidateAsync(dataType, "file url");

            // Assert
            Assert.Empty(result.Errors);
        }


        private static Mock<IBlobStorageProvider> GetBlobStorageProviderMoq()
        {
            var blobStorageProviderMoq = new Mock<IBlobStorageProvider>();

            var blobInfo = new BlobInfo()
            { Size = (int)ModuleConstants.Settings.General.ImportFileMaxSize.DefaultValue * ModuleConstants.MByte };
            blobStorageProviderMoq.Setup(x => x.GetBlobInfoAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(blobInfo));
            return blobStorageProviderMoq;
        }
    }
}
