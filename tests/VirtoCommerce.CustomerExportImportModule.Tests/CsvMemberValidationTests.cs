using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CustomerExportImportModule.Core;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerExportImportModule.Data.Validation;
using Xunit;

namespace VirtoCommerce.CustomerExportImportModule.Tests
{
    [Trait("Category", "CI")]
    public class CsvMemberValidationTests
    {
        private static readonly ImportRecord<CsvContact>[] Contacts =
        {
            new ImportRecord<CsvContact>
            {
                Record = new CsvContact { Id = "TestId1", ContactFullName = "Test1" }
            },
            new ImportRecord<CsvContact>
            {
                Record = new CsvContact { Id = "TestId1", OuterId = "TestOuterId1", ContactFullName = "Test2" }
            },
            new ImportRecord<CsvContact>
            {
                Record = new CsvContact { Id = "TestId2", OuterId = "TestOuterId1", ContactFullName = "Test3" }
            }
        };

        [Fact]
        public async Task ValidateAsync_Duplicates_WillFailAndReportFirst()
        {
            // Arrange
            var validator = GetValidator();

            // Act
            var validationResult = await validator.ValidateAsync(Contacts);

            // Assert
            var errors = validationResult.Errors.Where(validationError => validationError.ErrorCode == ModuleConstants.ValidationErrors.DuplicateError).ToArray();
            Assert.NotNull(errors[0]);
            Assert.Equal("Test1", (errors[0].CustomState as ImportValidationState<CsvContact>)?.InvalidRecord.Record.ContactFullName);
            Assert.NotNull(errors[1]);
            Assert.Equal("Test2", (errors[1].CustomState as ImportValidationState<CsvContact>)?.InvalidRecord.Record.ContactFullName);
        }

        private ImportContactValidator GetValidator()
        {
            return new ImportContactValidator();
        }
    }
}
