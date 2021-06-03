using System.Collections.Generic;
using CsvHelper.Configuration.Attributes;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DynamicProperties;

namespace VirtoCommerce.CustomerExportImportModule.Core.Models
{
    public abstract class CsvMember : IExportable, IHasOuterId, IHasDynamicProperties
    {
        public abstract string Id { get; set; }

        public abstract string OuterId { get; set; }

        [Optional]
        [Name("Address Type")]
        public string AddressType { get; set; }

        [Optional]
        [Name("Address First Name")]
        public string AddressFirstName { get; set; }

        [Optional]
        [Name("Address Last Name")]
        public string AddressLastName { get; set; }
        
        [Optional]
        [Name("Address Country")]
        public string AddressCountry { get; set; }

        [Optional]
        [Name("Address Country Code")]
        public string AddressCountryCode { get; set; }

        [Optional]
        [Name("Address Region")]
        public string AddressRegion { get; set; }

        [Optional]
        [Name("Address City")]
        public string AddressCity { get; set; }

        [Optional]
        [Name("Address Line1")]
        public string AddressLine1 { get; set; }

        [Optional]
        [Name("Address Line2")]
        public string AddressLine2 { get; set; }

        [Optional]
        [Name("Address Zip Code")]
        public string AddressZipCode { get; set; }

        [Optional]
        [Name("Address Email")]
        public string AddressEmail { get; set; }

        [Optional]
        [Name("Address Phone")]
        public string AddressPhone { get; set; }

        [Ignore]
        public string ObjectType { get; set; }

        public ICollection<DynamicObjectProperty> DynamicProperties { get; set; }
    }
}
