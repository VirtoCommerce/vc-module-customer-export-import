using System.Collections.Generic;
using CsvHelper.Configuration.Attributes;
using VirtoCommerce.Platform.Core.DynamicProperties;

namespace VirtoCommerce.CustomerExportImportModule.Core.Models
{
    public abstract class CsvMember: IExportable, IHasDynamicProperties
    {
        public abstract string Id { get; set; }

        [Name("Address Type")]
        public string AddressType { get; set; }

        [Name("Address First Name")]
        public string AddressFirstName { get; set; }

        [Name("Address Last Name")]
        public string AddressLastName { get; set; }

        [Name("Address Country")]
        public string AddressCountry { get; set; }

        [Name("Address Region")]
        public string AddressRegion { get; set; }

        [Name("Address City")]
        public string AddressCity { get; set; }

        [Name("Address Address Line1")]
        public string AddressAddressLine1 { get; set; }

        [Name("Address Address Line2")]
        public string AddressAddressLine2 { get; set; }

        [Name("Address Zip Code")]
        public string AddressZipCode { get; set; }

        [Name("Address Email")]
        public string AddressEmail { get; set; }

        [Name("Address Phone")]
        public string AddressPhone { get; set; }

        public string ObjectType { get; set; }

        public ICollection<DynamicObjectProperty> DynamicProperties { get; set; }
    }
}
