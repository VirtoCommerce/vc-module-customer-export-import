using System.Collections.Generic;
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
        public static IEnumerable<object[]> Contacts
        {
            get
            {
                yield return new object[]
                {
                    new[]
                    {
                        new ImportRecord<CsvContact> { Record = new CsvContact { Id = "TestId", ContactFullName = "Test1" } },
                        new ImportRecord<CsvContact> { Record = new CsvContact { Id = "TestId", ContactFullName = "Test2" } }
                    }
                };
                yield return new object[]
                {
                    new[]
                    {
                        new ImportRecord<CsvContact> { Record = new CsvContact { OuterId = "TestId", ContactFullName = "Test1" } },
                        new ImportRecord<CsvContact> { Record = new CsvContact { OuterId = "TestId", ContactFullName = "Test2" } }
                    }
                };
            }
        }

        [Theory]
        [MemberData(nameof(Contacts))]
        public async Task ValidateAsync_Duplicates_WillFailAndReportFirst(ImportRecord<CsvContact>[] importRecords)
        {
            // Arrange
            var validator = GetValidator();

            // Act
            var validationResult = await validator.ValidateAsync(importRecords);

            // Assert
            var error = validationResult.Errors.FirstOrDefault(validationError => validationError.ErrorCode == ModuleConstants.ValidationErrors.DuplicateError);
            Assert.NotNull(error);
            Assert.Equal("Test1", (error.CustomState as ImportValidationState<CsvContact>)?.InvalidRecord.Record.ContactFullName);
        }

        private ImportContactsValidator GetValidator()
        {
            return new ImportContactsValidator();
        }
    }
}
