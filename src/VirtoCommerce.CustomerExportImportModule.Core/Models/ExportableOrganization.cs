using System.Linq;
using CsvHelper.Configuration.Attributes;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DynamicProperties;

namespace VirtoCommerce.CustomerExportImportModule.Core.Models
{
    public sealed class ExportableOrganization: CsvOrganization
    {
        [Index(0)]
        public override string Id { get; set; }

        [Index(1)]
        public override string OuterId { get; set; }

        [Index(2)]
        public override string OrganizationName { get; set; }

        [Index(3)]
        public override string ParentOrganizationName { get; set; }

        [Index(4)]
        public override string ParentOrganizationId { get; set; }

        [Index(5)]
        public override string ParentOrganizationOuterId { get; set; }

        [Index(6)]
        public override string BusinessCategory { get; set; }

        [Index(7)]
        public override string Description { get; set; }

        [Index(8)]
        public override string OrganizationGroups { get; set; }

        public CsvOrganization FromModels(Organization organization, Organization parentOrganization)
        {
            var address = organization.Addresses?.FirstOrDefault();

            Id = organization.Id;
            OuterId = organization.OuterId;
            OrganizationName = organization.Name;
            ParentOrganizationName = parentOrganization?.Name;
            ParentOrganizationId = organization.ParentId;
            ParentOrganizationOuterId = parentOrganization?.OuterId;
            AddressType = address?.AddressType.ToString();
            AddressFirstName = address?.FirstName;
            AddressLastName = address?.LastName;
            AddressCountry = address?.CountryName;
            AddressCountryCode = address?.CountryCode;
            AddressRegion = address?.RegionName;
            AddressCity = address?.City;
            AddressLine1 = address?.Line1;
            AddressLine2 = address?.Line2;
            AddressZipCode = address?.PostalCode;
            AddressEmail = address?.Email;
            AddressPhone = address?.Phone;
            Phones = organization.Phones.IsNullOrEmpty() ? null : string.Join(", ", organization.Phones);
            BusinessCategory = organization.BusinessCategory;
            Description = organization.Description;
            OrganizationGroups = organization.Groups.IsNullOrEmpty() ? null : string.Join(", ", organization.Groups);

            DynamicProperties = organization.DynamicProperties?.Select(x => x.Clone() as DynamicObjectProperty)
                .ToArray();

            return this;
        }
    }
}
