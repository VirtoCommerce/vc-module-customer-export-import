using System.Linq;
using CsvHelper.Configuration.Attributes;
using Newtonsoft.Json;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DynamicProperties;

namespace VirtoCommerce.CustomerExportImportModule.Core.Models
{
    public sealed class CsvOrganization : CsvMember
    {
        [JsonProperty("organizationId")]
        [Name("Organization Id")]
        public override string Id { get; set; }

        [JsonProperty("organizationOuterId")]
        [Name("Organization Outer Id")]
        public override string OuterId { get; set; }

        [Name("Organization Name")]
        public string OrganizationName { get; set; }

        [Name("Parent Organization Name")]
        public string ParentOrganizationName { get; set; }

        [Name("Parent Organization Id")]
        public string ParentOrganizationId { get; set; }

        [Name("Parent Organization Outer Id")]
        public string ParentOrganizationOuterId { get; set; }

        [Name("Phones")]
        public string Phones { get; set; }

        [Name("Business category")]
        public string BusinessCategory { get; set; }

        [Name("Description")]
        public string Description { get; set; }

        [Name("Organization Groups")]
        public string OrganizationGroups { get; set; }

        public CsvOrganization FromModel(Organization organization, Organization parentOrganization)
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
            AddressCountryCode = address?.CountryCode;
            AddressCountry = address?.CountryName;
            AddressCountryCode = address?.CountryCode;
            AddressRegion = address?.RegionName;
            AddressCity = address?.City;
            AddressLine1 = address?.Line1;
            AddressLine2 = address?.Line2;
            AddressZipCode = address?.PostalCode;
            AddressEmail = address?.Email;
            AddressPhone = address?.Phone;
            Phones = organization.Phones.IsNullOrEmpty() ? null : string.Join(",", organization.Phones);
            BusinessCategory = organization.BusinessCategory;
            Description = organization.Description;
            OrganizationGroups = organization.Groups.IsNullOrEmpty() ? null : string.Join(", ", organization.Groups);

            DynamicProperties = organization.DynamicProperties?.Select(x => x.Clone() as DynamicObjectProperty)
                .ToArray();

            return this;
        }
    }
}
