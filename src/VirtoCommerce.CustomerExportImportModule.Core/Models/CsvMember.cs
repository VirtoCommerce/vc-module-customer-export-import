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
        [Name("Phones")]
        public virtual string Phones { get; set; }

        [Optional]
        [Name("Emails")]
        public virtual string Emails { get; set; }

        [Optional]
        [Name("Address Type")]
        public virtual string AddressType { get; set; }

        [Optional]
        [Name("Address First Name")]
        public virtual string AddressFirstName { get; set; }

        [Optional]
        [Name("Address Last Name")]
        public virtual string AddressLastName { get; set; }

        [Optional]
        [Name("Address Country")]
        public virtual string AddressCountry { get; set; }

        [Optional]
        [Name("Address Country Code")]
        public virtual string AddressCountryCode { get; set; }

        [Optional]
        [Name("Address Region")]
        public virtual string AddressRegion { get; set; }

        [Optional]
        [Name("Address City")]
        public virtual string AddressCity { get; set; }

        [Optional]
        [Name("Address Line1")]
        public virtual string AddressLine1 { get; set; }

        [Optional]
        [Name("Address Line2")]
        public virtual string AddressLine2 { get; set; }

        [Optional]
        [Name("Address Zip Code")]
        public virtual string AddressZipCode { get; set; }

        [Optional]
        [Name("Address Email")]
        public virtual string AddressEmail { get; set; }

        [Optional]
        [Name("Address Phone")]
        public virtual string AddressPhone { get; set; }

        [Ignore]
        public string ObjectType { get; set; }

        public ICollection<DynamicObjectProperty> DynamicProperties { get; set; }
    }
}
