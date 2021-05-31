using System;
using System.Collections.Generic;
using System.IO;
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
using ExportableOrganization = VirtoCommerce.CustomerExportImportModule.Core.Models.ExportableOrganization;

namespace VirtoCommerce.CustomerExportImportModule.Tests
{

    [Trait("Category", "CI")]
    public class CsvClassMapsTest
    {
        [Fact]
        public void Map_ContactWithDynamicProperty_Success()
        {
            //Arrange
            var dynamicProperties = new List<DynamicObjectProperty>
            {
                new DynamicObjectProperty()
                {
                    Name = "Sex",
                    ValueType = DynamicPropertyValueType.ShortText,
                    Values = new[] {
                    new DynamicPropertyObjectValue
                    {
                        Value = "Male",
                        ValueType = DynamicPropertyValueType.ShortText
                    }}
                }
            };

            var contact = new Contact()
            {
                Id = "contact_id",
                FirstName = "Anton",
                LastName = "Boroda",
                FullName = "Anton Boroda",
                OuterId = "outer_id",
                SecurityAccounts = new List<ApplicationUser>()
                {
                    new ApplicationUser()
                    {
                        Id = "account_id",
                        UserName = "login",
                        StoreId = "b2b-store",
                        Email = "c@mail.com",
                        UserType = "customer",
                        Status = "new",
                        EmailConfirmed = true
                    }
                },
                Status = "new",
                AssociatedOrganizations = new[] { "org_id1", "org_id2" },
                BirthDate = new DateTime(1986, 04, 14),
                TimeZone = "MSK",
                Phones = new List<string>(new[] { "777", "555" }),
                Groups = new List<string>(new[] { "tag1", "tag2" }),
                Salutation = "mr",
                DefaultLanguage = "en_US",
                TaxPayerId = "TaxId",
                PreferredCommunication = "email",
                PreferredDelivery = "pickup",

                Addresses = new List<Address>(new[]{new Address()
                {
                    AddressType = AddressType.Pickup,
                    FirstName = "Anton",
                    LastName = "Boroda",
                    CountryName = "Russia",
                    RegionName = "Kirov region",
                    City = "Kirov",
                    Line1 = "1 st",
                    Line2 = "169",
                    Email = "c@mail.com",
                    PostalCode = "610033",
                    Phone = "777"
                }}),

                DynamicProperties = dynamicProperties
            };

            var organization = new Organization()
            {
                Id = "org_id",
                Name = "Boroda ltd",
                OuterId = "org_outer_id"

            };

            var store = new Store() { Id = "b2b-store", Name = "b2b-store" };
            var exportableContact = new ExportableContact();
            exportableContact.FromModel(contact, organization, store);

            var stream = new MemoryStream();
            var sw = new StreamWriter(stream, leaveOpen: true);
            var csvWriter = new CsvWriter(sw, new ExportConfiguration());

            //Act
            var selectedDynamicProperties = new[] { "Sex" };
            csvWriter.Configuration.RegisterClassMap(new GenericClassMap<ExportableContact>(selectedDynamicProperties));
            csvWriter.WriteRecords(new[] { exportableContact });

            sw.Dispose();
            csvWriter.Dispose();


            stream.Seek(0, SeekOrigin.Begin);

            //Assert
            var expected = "Contact Id;Contact First Name;Contact Last Name;Contact Full Name;Contact Outer Id;Organization Id;Organization Outer Id;Organization Name;Account Id;Account Login;Store Id;Store Name;Account Email;Account Type;Account Status;Email Verified;Contact Status;Associated Organization Ids;Birthday;TimeZone;Phones;User groups;Salutation;Default language;Taxpayer ID;Preferred communication;Preferred delivery;Address Type;Address First Name;Address Last Name;Address Country;Address Region;Address City;Address Line1;Address Line2;Address Zip Code;Address Email;Address Phone;Sex\r\n"
                           + "contact_id;Anton;Boroda;Anton Boroda;outer_id;org_id;org_outer_id;Boroda ltd;account_id;login;b2b-store;b2b-store;c@mail.com;customer;new;True;new;org_id1, org_id2;04/14/1986 00:00:00;MSK;777, 555;tag1, tag2;mr;en_US;TaxId;email;pickup;BillingAndShipping;Anton;Boroda;Russia;Kirov region;Kirov;1 st;169;610033;c@mail.com;777;Male\r\n";

            var sr = new StreamReader(stream);
            var csv = sr.ReadToEnd();

            Assert.Equal(expected, csv);
        }

        [Fact]
        public void Map_OrganizationWithDynamicProperty_Success()
        {
            //Arrange
            var dynamicProperties = new List<DynamicObjectProperty>
            {
                new DynamicObjectProperty()
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

            var organization = new Organization()
            {
                Id = "org_id1",
                Name = "Boroda ltd",
                OuterId = "OuterId1",
                ParentId = "parent_otg_id",
                Phones = new List<string>(new[] { "777", "555" }),
                BusinessCategory = "Market Place",
                Description = "org desc",
                Groups = new List<string>(new[] { "tag1", "tag2" }),

                Addresses = new List<Address>(new[]{new Address()
                {
                    AddressType = AddressType.Pickup,
                    FirstName = "Anton",
                    LastName = "Boroda",
                    CountryName = "Russia",
                    RegionName = "Kirov region",
                    City = "Kirov",
                    Line1 = "1 st",
                    Line2 = "169",
                    Email = "c@mail.com",
                    PostalCode = "610033",
                    Phone = "777"
                }}),

                DynamicProperties = dynamicProperties
            };

            var parent = new Organization() { Id = "parent_org_id", OuterId = "parent_outer_id", Name = "parent_outer_id" };

            var exportableOrganization = new ExportableOrganization();
            exportableOrganization.FromModel(organization, parent);

            var stream = new MemoryStream();
            var sw = new StreamWriter(stream, leaveOpen: true);
            var csvWriter = new CsvWriter(sw, new ExportConfiguration());

            //Act
            var selectedDynamicProperties = new[] { "Size" };
            csvWriter.Configuration.RegisterClassMap(new GenericClassMap<ExportableOrganization>(selectedDynamicProperties));

            csvWriter.WriteRecords(new[] { exportableOrganization });

            sw.Dispose();
            csvWriter.Dispose();

            stream.Seek(0, SeekOrigin.Begin);

            //Assert
            var expected = "Organization Id;Organization Outer Id;Organization Name;Parent Organization Name;Parent Organization Id;Parent Organization Outer Id;Phones;Business category;Description;Organization Groups;Address Type;Address First Name;Address Last Name;Address Country;Address Region;Address City;Address Line1;Address Line2;Address Zip Code;Address Email;Address Phone;Size\r\n"
                           + "org_id1;OuterId1;Boroda ltd;parent_outer_id;parent_otg_id;parent_outer_id;777,555;Market Place;org desc;tag1, tag2;BillingAndShipping;Anton;Boroda;Russia;Kirov region;Kirov;1 st;169;610033;c@mail.com;777;Huge\r\n";

            var sr = new StreamReader(stream);
            var csv = sr.ReadToEnd();

            Assert.Equal(expected, csv);
        }
    }
}
