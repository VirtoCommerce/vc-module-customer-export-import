using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerExportImportModule.Data.ExportImport;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.StoreModule.Core.Model;
using Xunit;
using Address = VirtoCommerce.CustomerModule.Core.Model.Address;

namespace VirtoCommerce.CustomerExportImportModule.Tests
{

    [Trait("Category", "CI")]
    public class CsvClassMapsTest
    {
        private static readonly List<DynamicProperty> ContactDynamicProperties = new List<DynamicProperty>
        {
            new DynamicObjectProperty
            {
                Id = "SexId",
                Name = "Sex",
                ValueType = DynamicPropertyValueType.ShortText,
                Values = new[] { new DynamicPropertyObjectValue { PropertyId = "SexId", PropertyName = "Sex", ValueType = DynamicPropertyValueType.ShortText, Value = "Male" } }
            },
            new DynamicObjectProperty
            {
                Id = "JobId",
                Name = "Job",
                IsDictionary = true,
                ValueType = DynamicPropertyValueType.ShortText,
                Values = new[]
                {
                    new DynamicPropertyObjectValue
                    {
                        PropertyId = "JobId",
                        PropertyName = "Job",
                        ValueId = "DeveloperId",
                        ValueType = DynamicPropertyValueType.ShortText,
                        Value = "Developer"
                    }
                }
            }
        };

        private static readonly Dictionary<string, IList<DynamicPropertyDictionaryItem>> ContactDynamicPropertyDictionaryItems = new Dictionary<string, IList<DynamicPropertyDictionaryItem>>
        {
            {
                "JobId",
                new List<DynamicPropertyDictionaryItem>
                {
                    new DynamicPropertyDictionaryItem { Id = "DeveloperId", PropertyId = "JobId", Name = "Developer" },
                    new DynamicPropertyDictionaryItem { Id = "QAId", PropertyId = "JobId", Name = "QA" },
                    new DynamicPropertyDictionaryItem { Id = "BAId", PropertyId = "JobId", Name = "BA" }
                }
            }
        };

        private static readonly Contact Contact = new Contact
        {
            Id = "contact_id",
            FirstName = "Anton",
            LastName = "Boroda",
            FullName = "Anton Boroda",
            OuterId = "outer_id",
            SecurityAccounts = new List<ApplicationUser>
            {
                new ApplicationUser
                {
                    Id = "account_id",
                    UserName = "login",
                    StoreId = "b2b-store",
                    Email = "c@mail.com",
                    UserType = "customer",
                    Status = "new",
                    EmailConfirmed = true,
                    PasswordExpired = true,
                }
            },
            Status = "new",
            AssociatedOrganizations = new List<string>(new[] { "org_id1", "org_id2" }),
            BirthDate = new DateTime(1986, 04, 14),
            TimeZone = "MSK",
            Phones = new List<string>(new[] { "777", "555" }),
            Emails = new List<string>(new[] { "boroda@ya.ru" }),
            Groups = new List<string>(new[] { "tag1", "tag2" }),
            Salutation = "mr",
            DefaultLanguage = "en_US",
            TaxPayerId = "TaxId",
            PreferredCommunication = "email",
            PreferredDelivery = "pickup",
            Addresses = new List<Address>
            {
                new Address
                {
                    AddressType = AddressType.Pickup,
                    FirstName = "Anton",
                    LastName = "Boroda",
                    CountryName = "Russia",
                    CountryCode = "RUS",
                    RegionName = "Kirov region",
                    City = "Kirov",
                    Line1 = "1 st",
                    Line2 = "169",
                    Email = "c@mail.com",
                    PostalCode = "610033",
                    Phone = "777"
                }
            },
            DynamicProperties = ContactDynamicProperties.OfType<DynamicObjectProperty>().ToList()
        };

        private static readonly Organization ContactOrganization = new Organization { Id = "org_id", Name = "Boroda ltd", OuterId = "org_outer_id" };

        private static readonly Store Store = new Store { Id = "b2b-store", Name = "b2b-store" };

        private static readonly string ContactCsvHeader =
            "Contact Id;Contact First Name;Contact Last Name;Contact Full Name;Contact Outer Id;Organization Id;Organization Outer Id;Organization Name;Associated Organization Ids;User groups;Account Id;Account Login;Store Id;Store Name;Account Email;Account Type;Account Status;Email Verified;Contact Status;Birthday;TimeZone;Salutation;Default language;Taxpayer ID;Preferred communication;Preferred delivery;Phones;Address Type;Address First Name;Address Last Name;Address Country;Address Country Code;Address Region;Address City;Address Line1;Address Line2;Address Zip Code;Address Email;Address Phone;Emails;Sex;Job";
        private static readonly string ContactCsvRecord =
            "contact_id;Anton;Boroda;Anton Boroda;outer_id;org_id;org_outer_id;Boroda ltd;org_id1, org_id2;tag1, tag2;account_id;login;b2b-store;b2b-store;c@mail.com;customer;new;True;new;04/14/1986 00:00:00;MSK;mr;en_US;TaxId;email;pickup;777, 555;BillingAndShipping;Anton;Boroda;Russia;RUS;Kirov region;Kirov;1 st;169;610033;c@mail.com;777;boroda@ya.ru;Male;Developer";

        [Fact]
        public void Export_ContactWithDynamicProperty_HeaderAndValuesAreCorrect()
        {
            // Arrange
            var exportableContact = new ExportableContact();
            exportableContact.FromModels(Contact, ContactOrganization, Store);

            var stream = new MemoryStream();
            var streamWriter = new StreamWriter(stream, leaveOpen: true);
            var csvWriter = new CsvWriter(streamWriter, new ExportConfiguration());
            csvWriter.Configuration.RegisterClassMap(new GenericClassMap<ExportableContact>(ContactDynamicProperties));

            // Act
            csvWriter.WriteRecords(new[] { exportableContact });

            streamWriter.Dispose();
            csvWriter.Dispose();

            // Assert
            stream.Seek(0, SeekOrigin.Begin);

            var sr = new StreamReader(stream);
            var csv = sr.ReadToEnd();

            var expectedCsv = TestHelper.GetCsv(new[] { ContactCsvRecord }, ContactCsvHeader);

            Assert.Equal(expectedCsv, csv);
        }

        [Fact]
        public void Export_OrganizationWithDynamicProperty_HeaderAndValuesAreCorrect()
        {
            //Arrange
            var dynamicProperties = new List<DynamicProperty>
            {
                new DynamicObjectProperty
                {
                    Name = "Size",
                    ValueType = DynamicPropertyValueType.ShortText,
                    Values = new[] {
                    new DynamicPropertyObjectValue
                    {
                        Value = "Huge",
                        ValueType = DynamicPropertyValueType.ShortText
                    }}
                }
            };

            var organization = new Organization
            {
                Id = "org_id1",
                Name = "Boroda ltd",
                OuterId = "OuterId1",
                ParentId = "parent_otg_id",
                Phones = new List<string>(new[] { "777", "555" }),
                BusinessCategory = "Market Place",
                Description = "org desc",
                Groups = new List<string>(new[] { "tag1", "tag2" }),
                Addresses = new List<Address>
                {
                    new Address
                    {
                        AddressType = AddressType.Pickup,
                        FirstName = "Anton",
                        LastName = "Boroda",
                        CountryName = "Russia",
                        CountryCode = "RUS",
                        RegionName = "Kirov region",
                        City = "Kirov",
                        Line1 = "1 st",
                        Line2 = "169",
                        Email = "c@mail.com",
                        PostalCode = "610033",
                        Phone = "777"
                    }
                },
                DynamicProperties = dynamicProperties.OfType<DynamicObjectProperty>().ToList()
            };

            var parent = new Organization { Id = "parent_org_id", OuterId = "parent_outer_id", Name = "parent_outer_id" };

            var exportableOrganization = new ExportableOrganization();
            exportableOrganization.FromModels(organization, parent);

            var stream = new MemoryStream();
            var sw = new StreamWriter(stream, leaveOpen: true);
            var csvWriter = new CsvWriter(sw, new ExportConfiguration());

            //Act
            csvWriter.Configuration.RegisterClassMap(new GenericClassMap<ExportableOrganization>(dynamicProperties));

            csvWriter.WriteRecords(new[] { exportableOrganization });

            sw.Dispose();
            csvWriter.Dispose();

            stream.Seek(0, SeekOrigin.Begin);

            //Assert
            var expected = "Organization Id;Organization Outer Id;Organization Name;Parent Organization Name;Parent Organization Id;Parent Organization Outer Id;Business category;Description;Organization Groups;Phones;Address Type;Address First Name;Address Last Name;Address Country;Address Country Code;Address Region;Address City;Address Line1;Address Line2;Address Zip Code;Address Email;Address Phone;Emails;Size\r\n"
                           + "org_id1;OuterId1;Boroda ltd;parent_outer_id;parent_otg_id;parent_outer_id;Market Place;org desc;tag1, tag2;777, 555;BillingAndShipping;Anton;Boroda;Russia;RUS;Kirov region;Kirov;1 st;169;610033;c@mail.com;777;;Huge\r\n";

            var sr = new StreamReader(stream);
            var csv = sr.ReadToEnd();

            Assert.Equal(expected, csv);
        }

        [Theory]
        [MemberData(nameof(ContactImportData))]
        public void Import_ContactWithAndWithoutOptionalFields_HeaderAndValuesAreCorrect(string header, string record, Contact expectedContact, Organization expectedOrganization)
        {
            // Arrange
            var csv = TestHelper.GetCsv(new[] { record }, header);
            using var stream = TestHelper.GetStream(csv);
            using var streamReader = new StreamReader(stream);
            using var csvReader = new CsvReader(streamReader, new ImportConfiguration());
            csvReader.Configuration.RegisterClassMap(new GenericClassMap<ImportableContact>(ContactDynamicProperties, ContactDynamicPropertyDictionaryItems));

            // Act
            csvReader.Read();
            csvReader.ReadHeader();
            csvReader.ValidateHeader<ImportableContact>();
            csvReader.Read();
            var csvContact = csvReader.GetRecord<ImportableContact>();

            // Assert
            var contact = new Contact();
            csvContact.PatchModel(contact);
            var organization = csvContact.ToOrganization();

            if (expectedContact.SecurityAccounts.Any() && contact.SecurityAccounts.Any())
            {
                contact.SecurityAccounts.First().Id = expectedContact.SecurityAccounts.First().Id;
            }

            Assert.Equal(expectedContact, contact, new ByFieldValuesEqualityComparer<Contact>());
            Assert.Equal(expectedOrganization, organization, new ByFieldValuesEqualityComparer<Organization>());
        }

        public static IEnumerable<object[]> ContactImportData
        {
            get
            {
                var expectedContact = (Contact)Contact.Clone();
                expectedContact.Id = null; // not patched
                expectedContact.AssociatedOrganizations = null; // not imported
                expectedContact.Groups = null; // not imported
                yield return new object[] { ContactCsvHeader, ContactCsvRecord, expectedContact, ContactOrganization };
                yield return new object[]
                {
                    "Contact First Name;Contact Last Name;Contact Full Name", "FirstName;LastName;FullName",
                    new Contact { FirstName = "FirstName", LastName = "LastName", FullName = "FullName", Addresses = new List<Address>(), DynamicProperties = new List<DynamicObjectProperty>() }, new Organization()
                };
            }
        }
    }
}
