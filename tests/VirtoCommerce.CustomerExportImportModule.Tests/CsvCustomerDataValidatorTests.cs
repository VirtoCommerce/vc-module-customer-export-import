using System;
using System.Threading.Tasks;
using Moq;
using VirtoCommerce.CustomerExportImportModule.Core;
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

    }
}
