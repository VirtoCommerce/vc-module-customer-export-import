using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Moq;
using VirtoCommerce.CustomerExportImportModule.Core;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerExportImportModule.Data.Validation;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.Platform.Core.Security;
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
                        new ImportRecord<CsvContact> { Record = new CsvContact { Id = "TestId", ContactFirstName = "Test", ContactLastName = "1", ContactFullName = "Test1" } },
                        new ImportRecord<CsvContact> { Record = new CsvContact { Id = "TestId", ContactFirstName = "Test", ContactLastName = "2", ContactFullName = "Test2" } }
                    },
                    ModuleConstants.ValidationErrors.DuplicateError, "Test1"
                };
                yield return new object[]
                {
                    new[]
                    {
                        new ImportRecord<CsvContact> { Record = new CsvContact { OuterId = "TestId", ContactFirstName = "Test", ContactLastName = "1", ContactFullName = "Test1" } },
                        new ImportRecord<CsvContact> { Record = new CsvContact { OuterId = "TestId", ContactFirstName = "Test", ContactLastName = "2", ContactFullName = "Test2" } }
                    },
                    ModuleConstants.ValidationErrors.DuplicateError,
                    "Test1"
                };
                yield return new object[]
                {
                    new[]
                    {
                        new ImportRecord<CsvContact>
                        {
                            Record = new CsvContact
                            {
                                Id = "TestId1",
                                ContactFirstName = "Test",
                                ContactLastName = "1",
                                ContactFullName = "Test1",
                                AddressFirstName = "Test",
                                AddressLastName = "1"
                            }
                        },
                        new ImportRecord<CsvContact>
                        {
                            Record = new CsvContact
                            {
                                Id = "TestId2",
                                ContactFirstName = "Test",
                                ContactLastName = "2",
                                ContactFullName = "Test2",
                                AddressLine1 = "Line1",
                                AddressCity = "City",
                                AddressCountryCode = "USA",
                                AddressCountry = "United States",
                                AddressZipCode = "123456"
                            }
                        }
                    },
                    ModuleConstants.ValidationErrors.MissingRequiredValues, "Test1"
                };
                var longString = new string('*', 1000);
                yield return new object[]
                {
                    new[]
                    {
                        new ImportRecord<CsvContact>
                        {
                            Record = new CsvContact
                            {
                                Id = "TestId1",
                                ContactFirstName = "Test",
                                ContactLastName = "1",
                                ContactFullName = "Test1",
                                OuterId = longString,
                                AddressFirstName = longString,
                                AddressLastName = longString,
                                AddressEmail = longString,
                                AddressPhone = longString,
                                AddressLine1 = longString,
                                AddressLine2 = longString,
                                AddressCity = longString,
                                AddressRegion = longString,
                                AddressCountryCode = longString,
                                AddressCountry = longString,
                                AddressZipCode = longString
                            }
                        },
                        new ImportRecord<CsvContact>
                        {
                            Record = new CsvContact
                            {
                                Id = "TestId2",
                                ContactFirstName = "Test",
                                ContactLastName = "2",
                                ContactFullName = "Test2",
                                OuterId = new string('*', 128),
                                AddressFirstName = new string('*', 128),
                                AddressLastName = new string('*', 128),
                                AddressEmail = new string('*', 64),
                                AddressPhone = new string('*', 256),
                                AddressLine1 = new string('*', 128),
                                AddressLine2 = new string('*', 128),
                                AddressCity = new string('*', 128),
                                AddressRegion = new string('*', 128),
                                AddressCountryCode = new string('*', 64),
                                AddressCountry = new string('*', 128),
                                AddressZipCode = new string('*', 32)
                            }
                        }
                    },
                    ModuleConstants.ValidationErrors.ExceedingMaxLength, "Test1"
                };
                yield return new object[]
                {
                    new[]
                    {
                        new ImportRecord<CsvContact>
                        {
                            Record = new CsvContact
                            {
                                Id = "TestId1",
                                ContactFirstName = "Test",
                                ContactLastName = "1",
                                ContactFullName = "Test1",
                                AddressType = "Invalid",
                                AddressLine1 = "Line1",
                                AddressCity = "City",
                                AddressCountryCode = "USA",
                                AddressCountry = "United States",
                                AddressZipCode = "123456"
                            }
                        },
                        new ImportRecord<CsvContact>
                        {
                            Record = new CsvContact
                            {
                                Id = "TestId2",
                                ContactFirstName = "Test",
                                ContactLastName = "2",
                                ContactFullName = "Test2",
                                AddressType = "BillingAndShipping",
                                AddressLine1 = "Line1",
                                AddressCity = "City",
                                AddressCountryCode = "USA",
                                AddressCountry = "United States",
                                AddressZipCode = "123456"
                            }
                        }
                    },
                    ModuleConstants.ValidationErrors.InvalidValue, "Test1"
                };
                yield return new object[]
                {
                    new[]
                    {
                        new ImportRecord<CsvContact>
                        {
                            Record = new CsvContact
                            {
                                Id = "TestId1",
                                ContactFirstName = "Test",
                                ContactLastName = "1",
                                ContactFullName = "Test1",
                                DynamicProperties = new List<DynamicObjectProperty>
                                {
                                    new DynamicObjectProperty
                                    {
                                        Values = new List<DynamicPropertyObjectValue>()
                                    }
                                }
                            }
                        },
                        new ImportRecord<CsvContact>
                        {
                            Record = new CsvContact
                            {
                                Id = "TestId2",
                                ContactFirstName = "Test",
                                ContactLastName = "2",
                                ContactFullName = "Test2",
                            }
                        }
                    },
                    ModuleConstants.ValidationErrors.InvalidValue, "Test1"
                };
            }
        }

        [Theory]
        [MemberData(nameof(Contacts))]
        public async Task ValidateAsync_Duplicates_WillFailAndReportFirst(ImportRecord<CsvContact>[] importRecords, string errorCode, string failedEntityFullName)
        {
            // Arrange
            var validator = GetValidator();

            // Act
            var validationResult = await validator.ValidateAsync(importRecords);

            // Assert
            var error = validationResult.Errors.First(validationError => validationError.ErrorCode == errorCode);
            Assert.NotNull(error);
            Assert.Equal(failedEntityFullName, (error.CustomState as ImportValidationState<CsvContact>)?.InvalidRecord.Record.ContactFullName);
        }

        private ICountriesService GetCountriesService()
        {
            var countriesServiceMock = new Mock<ICountriesService>();
            countriesServiceMock.Setup(x => x.GetCountriesAsync()).ReturnsAsync(() => new List<Country>
            {
                new Country { Id = "RUS", Name = "Russia" }, new Country { Id = "USA", Name = "United States" }
            });
            return countriesServiceMock.Object;
        }

        //private SignInManager<ApplicationUser> GetSignInManager()
        //{
        //    var signInManagerMock = new Mock<SignInManager<ApplicationUser>>();
        //    signInManagerMock.Setup(x => x.UserManager.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(() => null);
        //    return signInManagerMock.Object;
        //}

        private ImportContactsValidator GetValidator()
        {
            return new ImportContactsValidator(GetCountriesService(), null);
        }
    }
}
