using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;
using VirtoCommerce.CustomerExportImportModule.Core;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerExportImportModule.Data.Validation;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.Platform.Core;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Model.Search;
using VirtoCommerce.StoreModule.Core.Services;
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
                // Duplicated by internal Id
                yield return new object[]
                {
                    new[]
                    {
                        new ImportRecord<ImportableContact> { Record = new ImportableContact { Id = "TestId", ContactFirstName = "Test", ContactLastName = "1", ContactFullName = "Test1" } },
                        new ImportRecord<ImportableContact> { Record = new ImportableContact { Id = "TestId", ContactFirstName = "Test", ContactLastName = "2", ContactFullName = "Test2" } }
                    },
                    ModuleConstants.ValidationErrors.DuplicateError, "Test1"
                };

                // Duplicated by Outer Id
                yield return new object[]
                {
                    new[]
                    {
                        new ImportRecord<ImportableContact> { Record = new ImportableContact { OuterId = "TestId", ContactFirstName = "Test", ContactLastName = "1", ContactFullName = "Test1" } },
                        new ImportRecord<ImportableContact> { Record = new ImportableContact { OuterId = "TestId", ContactFirstName = "Test", ContactLastName = "2", ContactFullName = "Test2" } }
                    },
                    ModuleConstants.ValidationErrors.DuplicateError,
                    "Test1"
                };

                // Missed required address fields if any optional specified
                yield return new object[]
                {
                    new[]
                    {
                        new ImportRecord<ImportableContact>
                        {
                            Record = new ImportableContact
                            {
                                Id = "TestId1",
                                ContactFirstName = "Test",
                                ContactLastName = "1",
                                ContactFullName = "Test1",
                                AddressFirstName = "Test",
                                AddressLastName = "1"
                            }
                        },
                        new ImportRecord<ImportableContact>
                        {
                            Record = new ImportableContact
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

                // Exceeded max length on contact fields
                var longString = new string('*', 1000);
                yield return new object[]
                {
                    new[]
                    {
                        new ImportRecord<ImportableContact>
                        {
                            Record = new ImportableContact
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
                        new ImportRecord<ImportableContact>
                        {
                            Record = new ImportableContact
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

                // Exceeded max length inside joined values
                yield return new object[]
                {
                    new[]
                    {
                        new ImportRecord<ImportableContact>
                        {
                            Record = new ImportableContact
                            {
                                Id = "TestId1",
                                ContactFirstName = "Test",
                                ContactLastName = "1",
                                ContactFullName = "Test1",
                                Phones = string.Join(", ", new string('0', 65), new string('0', 65)),
                                Emails = string.Join(", ", $"{new string('a', 260)}@example.org", $"{new string('b', 260)}@example.org")
                            }
                        },
                        new ImportRecord<ImportableContact>
                        {
                            Record = new ImportableContact
                            {
                                Id = "TestId2",
                                ContactFirstName = "Test",
                                ContactLastName = "2",
                                ContactFullName = "Test2",
                                Phones = "01234567890,01234567890",
                                Emails = string.Join(", ", $"{new string('a', 242)}@example.org", $"{new string('b', 242)}@example.org")
                            }
                        }
                    },
                    ModuleConstants.ValidationErrors.ArrayValuesExceedingMaxLength, "Test1"
                };

                // Invalid values in address fields
                yield return new object[]
                {
                    new[]
                    {
                        new ImportRecord<ImportableContact>
                        {
                            Record = new ImportableContact
                            {
                                Id = "TestId1",
                                ContactFirstName = "Test",
                                ContactLastName = "1",
                                ContactFullName = "Test1",
                                AddressType = "Invalid",
                                AddressLine1 = "Line1",
                                AddressCity = "City",
                                AddressCountryCode = "RU",
                                AddressCountry = "Great Britain",
                                AddressZipCode = "123456"
                            }
                        },
                        new ImportRecord<ImportableContact>
                        {
                            Record = new ImportableContact
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

                // Invalid dynamic property values
                yield return new object[]
                {
                    new[]
                    {
                        new ImportRecord<ImportableContact>
                        {
                            Record = new ImportableContact
                            {
                                Id = "TestId1",
                                ContactFirstName = "Test",
                                ContactLastName = "1",
                                ContactFullName = "Test1",
                                DynamicProperties = new List<DynamicObjectProperty>
                                {
                                    new DynamicObjectProperty { Values = new List<DynamicPropertyObjectValue>() },
                                    new DynamicObjectProperty { IsDictionary = true, Values = new List<DynamicPropertyObjectValue> { new DynamicPropertyObjectValue() } },
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
                        new ImportRecord<ImportableContact>
                        {
                            Record = new ImportableContact
                            {
                                Id = "TestId2",
                                ContactFirstName = "Test",
                                ContactLastName = "2",
                                ContactFullName = "Test2",
                                DynamicProperties = new List<DynamicObjectProperty>
                                {
                                    new DynamicObjectProperty
                                    {
                                        Id = "TestDictionary",
                                        IsDictionary = true,
                                        ValueType = DynamicPropertyValueType.ShortText,
                                        Values = new List<DynamicPropertyObjectValue>
                                        {
                                            new DynamicPropertyObjectValue { PropertyId = "TestDictionary", ValueType = DynamicPropertyValueType.ShortText, Value = "Test1", ValueId = "Test1" }
                                        }
                                    },
                                    new DynamicObjectProperty
                                    {
                                        IsArray = true,
                                        ValueType = DynamicPropertyValueType.ShortText,
                                        Values = new List<DynamicPropertyObjectValue>
                                        {
                                            new DynamicPropertyObjectValue { ValueType = DynamicPropertyValueType.ShortText, Value = "Test1" },
                                            new DynamicPropertyObjectValue { ValueType = DynamicPropertyValueType.ShortText, Value = "Test2" }
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

                // Invalid values for account
                yield return new object[]
                {
                    new[]
                    {
                        new ImportRecord<ImportableContact>
                        {
                            Record = new ImportableContact
                            {
                                Id = "TestId1",
                                ContactFirstName = "Test",
                                ContactLastName = "1",
                                ContactFullName = "Test1",
                                AccountLogin = "Test2",
                                AccountEmail = "example..org",
                                AccountType = "Invalid",
                                AccountStatus = "Invalid",
                                StoreId = "InvalidStore"
                            }
                        },
                        new ImportRecord<ImportableContact>
                        {
                            Record = new ImportableContact
                            {
                                Id = "TestId2",
                                ContactFirstName = "Test",
                                ContactLastName = "2",
                                ContactFullName = "Test2",
                                AccountLogin = "Test3",
                                AccountEmail = "test3@example.org",
                                AccountType = "Customer",
                                AccountStatus = "Approved",
                                StoreId = "TestStore"
                            }
                        }
                    },
                    ModuleConstants.ValidationErrors.InvalidValue, "Test1"
                };

                // Not unique values (existing in the system) for account
                yield return new object[]
                {
                    new[]
                    {
                        new ImportRecord<ImportableContact>
                        {
                            Record = new ImportableContact
                            {
                                Id = "TestId1",
                                ContactFirstName = "Test",
                                ContactLastName = "1",
                                ContactFullName = "Test1",
                                AccountLogin = "Test1",
                                AccountEmail = "test1@example.org",
                                StoreId = "TestStore"
                            }
                        },
                        new ImportRecord<ImportableContact>
                        {
                            Record = new ImportableContact
                            {
                                Id = "TestId2",
                                ContactFirstName = "Test",
                                ContactLastName = "2",
                                ContactFullName = "Test2",
                                AccountLogin = "Test2",
                                AccountEmail = "test2@example.org",
                                StoreId = "TestStore"
                            }
                        }
                    },
                    ModuleConstants.ValidationErrors.NotUniqueValue, "Test1"
                };

                // Not unique values (existing in the file) for account
                yield return new object[]
                {
                    new[]
                    {
                        new ImportRecord<ImportableContact>
                        {
                            Record = new ImportableContact
                            {
                                Id = "TestId1",
                                ContactFirstName = "Test",
                                ContactLastName = "1",
                                ContactFullName = "Test1",
                                AccountLogin = "Test3",
                                AccountEmail = "test3@example.org",
                                AccountType = "Customer",
                                AccountStatus = "Approved",
                                StoreId = "TestStore"
                            }
                        },
                        new ImportRecord<ImportableContact>
                        {
                            Record = new ImportableContact
                            {
                                Id = "TestId2",
                                ContactFirstName = "Test",
                                ContactLastName = "2",
                                ContactFullName = "Test2",
                                AccountLogin = "Test3",
                                AccountEmail = "test3@example.org",
                                AccountType = "Customer",
                                AccountStatus = "Approved",
                                StoreId = "TestStore"
                            }
                        }
                    },
                    ModuleConstants.ValidationErrors.NotUniqueValue, "Test1"
                };

                // Password
                yield return new object[]
                {
                    new[]
                    {
                        new ImportRecord<ImportableContact>
                        {
                            Record = new ImportableContact
                            {
                                Id = "TestId1",
                                ContactFirstName = "Test",
                                ContactLastName = "1",
                                ContactFullName = "Test1",
                                AccountLogin = "Test2",
                                AccountEmail = "test2@example.org",
                                AccountType = "Customer",
                                AccountStatus = "Approved",
                                StoreId = "TestStore",
                                Password = "Invalid"
                            }
                        },
                        new ImportRecord<ImportableContact>
                        {
                            Record = new ImportableContact
                            {
                                Id = "TestId2",
                                ContactFirstName = "Test",
                                ContactLastName = "2",
                                ContactFullName = "Test2",
                                AccountLogin = "Test3",
                                AccountEmail = "test3@example.org",
                                AccountType = "Customer",
                                AccountStatus = "Approved",
                                StoreId = "TestStore",
                                Password = new string('*', 32)
                            }
                        },
                        new ImportRecord<ImportableContact>
                        {
                            Record = new ImportableContact
                            {
                                Id = "TestId3",
                                ContactFirstName = "Test",
                                ContactLastName = "3",
                                ContactFullName = "Test3",
                                AccountLogin = "Test4",
                                AccountEmail = "test4@example.org",
                                AccountType = "Customer",
                                AccountStatus = "Approved",
                                StoreId = "TestStore"
                            }
                        }
                    },
                    ModuleConstants.ValidationErrors.PasswordDoesntMeetSecurityPolicy, "Test1"
                };
            }
        }


        public static IEnumerable<object[]> Organizations
        {
            get
            {
                // Duplicated by internal Id
                yield return new object[]
                {
                    new[]
                    {
                        new ImportRecord<ImportableOrganization> { Record = new ImportableOrganization { Id = "TestId", OrganizationName = "Test1" } },
                        new ImportRecord<ImportableOrganization> { Record = new ImportableOrganization { Id = "TestId", OrganizationName = "Test2" } }
                    },
                    ModuleConstants.ValidationErrors.DuplicateError, "Test1"
                };

                // Duplicated by Outer Id
                yield return new object[]
                {
                    new[]
                    {
                        new ImportRecord<ImportableOrganization> { Record = new ImportableOrganization { OuterId = "TestId", OrganizationName = "Test1" } },
                        new ImportRecord<ImportableOrganization> { Record = new ImportableOrganization { OuterId = "TestId", OrganizationName = "Test2" } }
                    },
                    ModuleConstants.ValidationErrors.DuplicateError,
                    "Test1"
                };

                // Missed required address fields if any optional specified
                yield return new object[]
                {
                    new[]
                    {
                        new ImportRecord<ImportableOrganization>
                        {
                            Record = new ImportableOrganization
                            {
                                Id = "TestId1",
                                OrganizationName = "Test1",
                                AddressFirstName = "Test",
                                AddressLastName = "1"
                            }
                        },
                        new ImportRecord<ImportableOrganization>
                        {
                            Record = new ImportableOrganization
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

                // Exceeding max length of organization fields
                var longString = new string('*', 1000);
                yield return new object[]
                {
                    new[]
                    {
                        new ImportRecord<ImportableOrganization>
                        {
                            Record = new ImportableOrganization
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
                        new ImportRecord<ImportableOrganization>
                        {
                            Record = new ImportableOrganization
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
        public async Task ValidateAsync_InvalidContact_WillFailAndReportFirst(ImportRecord<ImportableContact>[] importRecords, string errorCode, string failedEntityFullName)
        {
            // Arrange
            var validator = GetContactsValidator();

            // Act
            var validationResult = await validator.ValidateAsync(importRecords);

            // Assert
            var error = validationResult.Errors.Find(validationError => validationError.ErrorCode == errorCode);
            Assert.NotNull(error);
            Assert.Equal(failedEntityFullName, (error.CustomState as ImportValidationState<ImportableContact>)?.InvalidRecord.Record.ContactFullName);
        }

        [Theory]
        [MemberData(nameof(Organizations))]
        public async Task ValidateAsync_InvalidOrganization_WillFailAndReportFirst(ImportRecord<ImportableOrganization>[] importRecords, string errorCode, string failedEntityName)
        {
            // Arrange
            var validator = GetOrganizationsValidator();

            // Act
            var validationResult = await validator.ValidateAsync(importRecords);

            // Assert
            var error = validationResult.Errors.First(validationError => validationError.ErrorCode == errorCode);
            Assert.NotNull(error);
            Assert.Equal(failedEntityName, (error.CustomState as ImportValidationState<ImportableOrganization>)?.InvalidRecord.Record.OrganizationName);
        }

        private static ICountriesService GetCountriesService()
        {
            var countriesServiceMock = new Mock<ICountriesService>();
            countriesServiceMock
                .Setup(x => x.GetCountriesAsync())
                .ReturnsAsync(() => new List<Country>
                {
                    new() { Id = "RUS", Name = "Russia" }, new Country { Id = "USA", Name = "United States" }
                });
            return countriesServiceMock.Object;
        }

        private static IDynamicPropertyDictionaryItemsSearchService GetDynamicPropertyDictionaryItemsSearchService()
        {
            var dynamicPropertyDictionaryItemsSearchService = new Mock<IDynamicPropertyDictionaryItemsSearchService>();
            dynamicPropertyDictionaryItemsSearchService
                .Setup(x => x.SearchAsync(It.IsAny<DynamicPropertyDictionaryItemSearchCriteria>(), It.IsAny<bool>()))
                .Returns<DynamicPropertyDictionaryItemSearchCriteria, bool>((searchCriteria, clone) =>
                {
                    var dynamicPropertyDictionaryItems = new List<DynamicPropertyDictionaryItem>
                    {
                        new() { PropertyId = "TestDictionary", Id = "Value1", Name = "Value1" },
                        new() { PropertyId = "TestDictionary", Id = "Value2", Name = "Value2" }
                    };
                    return Task.FromResult(new DynamicPropertyDictionaryItemSearchResult
                    {
                        Results = dynamicPropertyDictionaryItems.Where(dynamicPropertyDictionaryItem => dynamicPropertyDictionaryItem.PropertyId == searchCriteria.PropertyId).ToList(),
                        TotalCount = dynamicPropertyDictionaryItems.Count
                    });
                });
            return dynamicPropertyDictionaryItemsSearchService.Object;
        }

        private static IPasswordValidator<ApplicationUser> GetPasswordValidator() => new PasswordValidator<ApplicationUser>();

        private static SignInManager<ApplicationUser> GetSignInManager() =>
            new FakeSignInManager(new[] { new ApplicationUser { UserName = "Test1", Email = "test1@example.org", StoreId = "TestStore" } }, GetPasswordValidator(),
                new OptionsWrapper<IdentityOptions>(new IdentityOptions
                {
                    Password = new PasswordOptions { RequireDigit = false, RequireLowercase = false, RequireUppercase = false, RequiredLength = 32, RequiredUniqueChars = 0, RequireNonAlphanumeric = true }
                }));

        private static IMemberService GetMemberService()
        {
            var memberServiceMock = new Mock<IMemberService>();
            memberServiceMock
                .Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((string memberId, string _, string memberType) =>
                {
                    if (memberType != nameof(Contact))
                    {
                        return null;
                    }

                    return new Contact
                    {
                        Id = memberId,
                        FirstName = "First",
                        LastName = "Last",
                        FullName = "First Last"
                    };
                });
            return memberServiceMock.Object;
        }

        private static IStoreSearchService GetStoreSearchService()
        {
            var storeSearchServiceMock = new Mock<IStoreSearchService>();
            storeSearchServiceMock
                .Setup(x => x.SearchAsync(It.IsAny<StoreSearchCriteria>(), It.IsAny<bool>()))
                .ReturnsAsync((StoreSearchCriteria criteria, bool _) =>
                {
                    var stores = new List<Store>
                    {
                        new() { Id = "TestStore", Name = "Test Store" },
                        new() { Id = "TestStore2", Name = "Test Store 2" }
                    };
                    return new StoreSearchResult
                    {
                        Results = stores.Where(store => criteria.ObjectIds.Contains(store.Id)).ToList(),
                        TotalCount = stores.Count,
                    };
                });
            return storeSearchServiceMock.Object;
        }

        private static ISettingsManager GetSettingsManager()
        {
            var settingsManagerMock = new Mock<ISettingsManager>();
            settingsManagerMock
                .Setup(x => x.GetObjectSettingAsync(It.IsAny<string>(), null, null))
                .ReturnsAsync<string, string, string, ISettingsManager, ObjectSettingEntry>((name, _, _) =>
                {
                    var settings = new List<ObjectSettingEntry>
                    {
                        new() { Name = PlatformConstants.Settings.Security.SecurityAccountTypes.Name, AllowedValues = new object[] { "Administrator", "Customer", "Manager" } },
                        new() { Name = PlatformConstants.Settings.Security.AccountStatuses.Name, AllowedValues = new object[] { "Approved", "Deleted", "New", "Rejected" } },
                        new() { Name = ModuleConstants.Settings.General.AddressRegionStrongValidation.Name, DefaultValue = false, ValueType = SettingValueType.Boolean },
                    };
                    return settings.Find(setting => setting.Name == name);
                });
            return settingsManagerMock.Object;
        }

        private static ImportAddressValidator<ImportableContact> GetContactAddressValidator() => new(GetCountriesService(), GetSettingsManager());

        private static ImportAddressValidator<ImportableOrganization> GetOrganizationAddressValidator() => new(GetCountriesService(), GetSettingsManager());

        private static ImportMemberValidator<ImportableContact> GetContactMemberValidator() => new(GetContactAddressValidator(), GetCountriesService(), GetDynamicPropertyDictionaryItemsSearchService());

        private static ImportMemberValidator<ImportableOrganization> GetOrganizationMemberValidator() => new(GetOrganizationAddressValidator(), GetCountriesService(), GetDynamicPropertyDictionaryItemsSearchService());

        private static ImportContactsValidator GetContactsValidator() => new(GetContactMemberValidator(), GetSignInManager(), GetPasswordValidator(), GetMemberService(), GetStoreSearchService(), GetSettingsManager());

        private static ImportOrganizationsValidator GetOrganizationsValidator() => new(GetOrganizationMemberValidator());
    }
}
