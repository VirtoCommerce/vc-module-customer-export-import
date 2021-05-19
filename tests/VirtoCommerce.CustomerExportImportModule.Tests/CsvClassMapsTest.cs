using System;
using System.Collections.Generic;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using VirtoCommerce.CustomerExportImportModule.Data.ExportImport.ClassMaps;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.Platform.Core.DynamicProperties;
using Xunit;

namespace VirtoCommerce.CustomerExportImportModule.Tests
{

    [Trait("Category", "CI")]
    public class CsvClassMapsTest
    {
        [Fact]
        public void Map_Contact_Success()
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
                Id = Guid.NewGuid().ToString(),
                FirstName = "Anton",
                LastName = "Boroda",

                DynamicProperties = dynamicProperties



            };

            var stream = new MemoryStream();

            var sw = new StreamWriter(stream, leaveOpen: true);
            var csvWriter = new CsvWriter(sw, new Configuration() { Delimiter = ";" });


            //Act
            var selectedDynamicProperties = new[] { "Sex" };
            csvWriter.Configuration.RegisterClassMap(new ContactClassMap(selectedDynamicProperties));

            csvWriter.WriteRecord(contact);

            sw.Flush();
            sw.Close();

            csvWriter.Dispose();
            sw.Dispose();

            stream.Seek(0, SeekOrigin.Begin);
            //Assert

            var sr = new StreamReader(stream);

            var csv = sr.ReadToEnd();

            Assert.NotEmpty(csv);

        }
    }
}
