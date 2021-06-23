using System;
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
                                Phones = string.Join(", ", new string('0', 65), new string('0', 65))
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
                                Phones = "01234567890,01234567890"
                            }
                        }
                    },
                    ModuleConstants.ValidationErrors.ArrayValuesExceedingMaxLength, "Test1"
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
                                    new DynamicObjectProperty { Values = new List<DynamicPropertyObjectValue>() },
                                    new DynamicObjectProperty { IsArray = false, Values = new List<DynamicPropertyObjectValue> { new DynamicPropertyObjectValue(), new DynamicPropertyObjectValue() } },
                                    new DynamicObjectProperty
                                    {
                                        ValueType = DynamicPropertyValueType.ShortText,
                                        Values = new List<DynamicPropertyObjectValue> { new DynamicPropertyObjectValue { ValueType = DynamicPropertyValueType.ShortText, Value = 0 } }
                                    },
                                    new DynamicObjectProperty
                                    {
                                        ValueType = DynamicPropertyValueType.ShortText,
                                        Values = new List<DynamicPropertyObjectValue> { new DynamicPropertyObjectValue { ValueType = DynamicPropertyValueType.LongText, Value = 0 } }
                                    },
                                    new DynamicObjectProperty
                                    {
                                        ValueType = DynamicPropertyValueType.Decimal,
                                        Values = new List<DynamicPropertyObjectValue> { new DynamicPropertyObjectValue { ValueType = DynamicPropertyValueType.Decimal, Value = "test" } }
                                    },
                                    new DynamicObjectProperty
                                    {
                                        ValueType = DynamicPropertyValueType.Integer,
                                        Values = new List<DynamicPropertyObjectValue> { new DynamicPropertyObjectValue { ValueType = DynamicPropertyValueType.Integer, Value = "test" } }
                                    },
                                    new DynamicObjectProperty
                                    {
                                        ValueType = DynamicPropertyValueType.Boolean,
                                        Values = new List<DynamicPropertyObjectValue> { new DynamicPropertyObjectValue { ValueType = DynamicPropertyValueType.Boolean, Value = "test" } }
                                    },
                                    new DynamicObjectProperty
                                    {
                                        ValueType = DynamicPropertyValueType.DateTime,
                                        Values = new List<DynamicPropertyObjectValue> { new DynamicPropertyObjectValue { ValueType = DynamicPropertyValueType.DateTime, Value = "test" } }
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
                                DynamicProperties = new List<DynamicObjectProperty>
                                {
                                    new DynamicObjectProperty
                                    {
                                        IsArray = true,
                                        ValueType = DynamicPropertyValueType.ShortText,
                                        Values = new List<DynamicPropertyObjectValue>
                                        {
                                            new DynamicPropertyObjectValue { ValueType = DynamicPropertyValueType.ShortText, Value = "-" },
                                            new DynamicPropertyObjectValue { ValueType = DynamicPropertyValueType.ShortText, Value = "+" }
                                        }
                                    },
                                    new DynamicObjectProperty
                                    {
                                        ValueType = DynamicPropertyValueType.ShortText,
                                        Values = new List<DynamicPropertyObjectValue> { new DynamicPropertyObjectValue { ValueType = DynamicPropertyValueType.ShortText, Value = "test" } }
                                    },
                                    new DynamicObjectProperty
                                    {
                                        ValueType = DynamicPropertyValueType.ShortText,
                                        Values = new List<DynamicPropertyObjectValue> { new DynamicPropertyObjectValue { ValueType = DynamicPropertyValueType.LongText, Value = "test" } }
                                    },
                                    new DynamicObjectProperty
                                    {
                                        ValueType = DynamicPropertyValueType.Decimal,
                                        Values = new List<DynamicPropertyObjectValue> { new DynamicPropertyObjectValue { ValueType = DynamicPropertyValueType.Decimal, Value = "3.1415" } }
                                    },
                                    new DynamicObjectProperty
                                    {
                                        ValueType = DynamicPropertyValueType.Integer,
                                        Values = new List<DynamicPropertyObjectValue> { new DynamicPropertyObjectValue { ValueType = DynamicPropertyValueType.Integer, Value = "0" } }
                                    },
                                    new DynamicObjectProperty
                                    {
                                        ValueType = DynamicPropertyValueType.Boolean,
                                        Values = new List<DynamicPropertyObjectValue> { new DynamicPropertyObjectValue { ValueType = DynamicPropertyValueType.Boolean, Value = "true" } }
                                    },
                                    new DynamicObjectProperty
                                    {
                                        ValueType = DynamicPropertyValueType.DateTime,
                                        Values = new List<DynamicPropertyObjectValue> { new DynamicPropertyObjectValue { ValueType = DynamicPropertyValueType.DateTime, Value = "1/1/2000 00:00" } }
                                    }
                                }
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
                                AccountLogin = "Test1",
                                AccountEmail = "test1@example.org",
                                StoreId = "Store"
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
                                AccountLogin = "Test2",
                                AccountEmail = "test2@example.org",
                                StoreId = "Store"
                            }
                        }
                    },
                    ModuleConstants.ValidationErrors.NotUniqueValue, "Test1"
                };
            }
        }


        public static IEnumerable<object[]> Organizations
        {
            get
            {
                yield return new object[]
                {
                    new[]
                    {
                        new ImportRecord<CsvOrganization> { Record = new CsvOrganization { Id = "TestId", OrganizationName = "Test1" } },
                        new ImportRecord<CsvOrganization> { Record = new CsvOrganization { Id = "TestId", OrganizationName = "Test2" } }
                    },
                    ModuleConstants.ValidationErrors.DuplicateError, "Test1"
                };
                yield return new object[]
                {
                    new[]
                    {
                        new ImportRecord<CsvOrganization> { Record = new CsvOrganization { OuterId = "TestId", OrganizationName = "Test1" } },
                        new ImportRecord<CsvOrganization> { Record = new CsvOrganization { OuterId = "TestId", OrganizationName = "Test2" } }
                    },
                    ModuleConstants.ValidationErrors.DuplicateError,
                    "Test1"
                };
                yield return new object[]
                {
                    new[]
                    {
                        new ImportRecord<CsvOrganization>
                        {
                            Record = new CsvOrganization
                            {
                                Id = "TestId1",
                                OrganizationName = "Test1",
                                AddressFirstName = "Test",
                                AddressLastName = "1"
                            }
                        },
                        new ImportRecord<CsvOrganization>
                        {
                            Record = new CsvOrganization
                            {
                                Id = "TestId2",
                                OrganizationName = "Test2",
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
                        new ImportRecord<CsvOrganization>
                        {
                            Record = new CsvOrganization
                            {
                                Id = "TestId1",
                                OrganizationName = "Test1",
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
                        new ImportRecord<CsvOrganization>
                        {
                            Record = new CsvOrganization
                            {
                                Id = "TestId2",
                                OrganizationName = "Test2",
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
            }
        }

        [Theory]
        [MemberData(nameof(Contacts))]
        public async Task ValidateAsync_InvalidContact_WillFailAndReportFirst(ImportRecord<CsvContact>[] importRecords, string errorCode, string failedEntityFullName)
        {
            // Arrange
            var validator = GetContactsValidator();

            // Act
            var validationResult = await validator.ValidateAsync(importRecords);

            // Assert
            var error = validationResult.Errors.First(validationError => validationError.ErrorCode == errorCode);
            Assert.NotNull(error);
            Assert.Equal(failedEntityFullName, (error.CustomState as ImportValidationState<CsvContact>)?.InvalidRecord.Record.ContactFullName);
        }

        [Theory]
        [MemberData(nameof(Organizations))]
        public async Task ValidateAsync_InvalidOrganization_WillFailAndReportFirst(ImportRecord<CsvOrganization>[] importRecords, string errorCode, string failedEntityName)
        {
            // Arrange
            var validator = GetOrganizationsValidator();

            // Act
            var validationResult = await validator.ValidateAsync(importRecords);

            // Assert
            var error = validationResult.Errors.First(validationError => validationError.ErrorCode == errorCode);
            Assert.NotNull(error);
            Assert.Equal(failedEntityName, (error.CustomState as ImportValidationState<CsvOrganization>)?.InvalidRecord.Record.OrganizationName);
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

        private SignInManager<ApplicationUser> GetSignInManager()
        {
            return new FakeSignInManager(new[] { new ApplicationUser { UserName = "Test1", Email = "test1@example.org" } });
        }

        private ImportContactsValidator GetContactsValidator()
        {
            return new ImportContactsValidator(GetCountriesService(), GetSignInManager());
        }

        private ImportOrganizationsValidator GetOrganizationsValidator()
        {
            return new ImportOrganizationsValidator(GetCountriesService());
        }
    }
}
