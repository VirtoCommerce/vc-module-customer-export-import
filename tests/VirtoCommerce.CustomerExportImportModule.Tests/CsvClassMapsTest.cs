using System;
using System.Collections.Generic;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerExportImportModule.Data.ExportImport.ClassMaps;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.StoreModule.Core.Model;
using Xunit;

namespace VirtoCommerce.CustomerExportImportModule.Tests
{

    [Trait("Category", "CI")]
    public class CsvClassMapsTest
    {
        [Fact]
        public void Map_ContactWithDynamicProperty_Success()
        {
            //Arrange
            var expected = @"Contact Id;Contact First Name;Contact Last Name;Contact Full Name;Contact Outer Id;Organization Id;Organization Outer Id;Organization Name;Account Id;Account Login;Store Id;Store Name;Account Email;Account Type;Account Status;Email Verified;Contact Status;Associated Organization Ids;Birthday;TimeZone;Phones;User groups;Default language;Taxpayer ID;Preferred communication;Preferred delivery;Address Type;Address First Name;Address Last Name;Address Country;Address Region;Address City;Address Address Line1;Address Address Line2;Address Zip Code;Address Email;Address Phone;ObjectType;Sex
id_c1;Anton;Boroda;;;id_org1;;Boroda ltd;;;;b2b-store;;;;;;;;;;;;;;;;;;;;;;;;;;;Male
";

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
                Id = "id_c1",
                FirstName = "Anton",
                LastName = "Boroda",

                DynamicProperties = dynamicProperties
            };

            var organization = new Organization()
            {
                Id = "id_org1",
                Name = "Boroda ltd"
            };

            var store = new Store() { Id = "b2b-store", Name = "b2b-store" };
            var exportableContact = new ExportableContact();
            exportableContact.FromModel(contact, organization, store);

            var stream = new MemoryStream();
            var sw = new StreamWriter(stream, leaveOpen: true)
            {
                NewLine = Environment.NewLine
            };

            var csvWriter = new CsvWriter(sw, new Configuration() { Delimiter = ";" });

            //Act
            var selectedDynamicProperties = new[] { "Sex" };
            csvWriter.Configuration.RegisterClassMap(new ContactClassMap(selectedDynamicProperties));

            csvWriter.WriteRecords(new[] { exportableContact });

            sw.Dispose();
            csvWriter.Dispose();


            stream.Seek(0, SeekOrigin.Begin);

            //Assert
            var sr = new StreamReader(stream);
            var csv = sr.ReadToEnd();

            Assert.Equal(expected, csv);
        }

        [Fact]
        public void Map_OrganizationWithDynamicProperty_Success()
        {
            //Arrange
            var expected = @"Organization Id;Organization Outer Id;Organization Name;Parent Organization Name;Parent Organization Id;Parent Organization Outer Id;Phones;Business category;Description;Organization Groups;Address Type;Address First Name;Address Last Name;Address Country;Address Region;Address City;Address Address Line1;Address Address Line2;Address Zip Code;Address Email;Address Phone;ObjectType;Size
org_id1;OuterId1;Boroda ltd;;;;;;;;;;;;;;;;;;;;Huge
";

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

                DynamicProperties = dynamicProperties
            };

            var exportableOrganization = new ExportableOrganization();
            exportableOrganization.FromModel(organization, null);

            var stream = new MemoryStream();
            var sw = new StreamWriter(stream, leaveOpen: true)
            {
                NewLine = Environment.NewLine
            };

            var csvWriter = new CsvWriter(sw, new Configuration() { Delimiter = ";", });

            //Act
            var selectedDynamicProperties = new[] { "Size" };
            csvWriter.Configuration.RegisterClassMap(new OrganizationClassMap(selectedDynamicProperties));

            csvWriter.WriteRecords(new[] { exportableOrganization });

            sw.Dispose();
            csvWriter.Dispose();

            stream.Seek(0, SeekOrigin.Begin);

            //Assert
            var sr = new StreamReader(stream);
            var csv = sr.ReadToEnd();

            Assert.Equal(expected, csv);
        }
    }
}
