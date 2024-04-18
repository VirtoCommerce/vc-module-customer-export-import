using System.Linq;
using CsvHelper.Configuration.Attributes;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Swagger;

namespace VirtoCommerce.CustomerExportImportModule.Core.Models
{
    [SwaggerSchemaId("CustomerExportableOrganization")]
    public sealed class ExportableOrganization : CsvOrganization
    {
        [Index(0)]
        public override string Id { get; set; }

        [Index(1)]
        public override string OuterId { get; set; }

        [Index(2)]
        public override string OrganizationName { get; set; }

        [Index(3)]
        [Name("Parent Organization Name")]
        public string ParentOrganizationName { get; set; }

        [Index(4)]
        [Name("Parent Organization Id")]
        public string ParentOrganizationId { get; set; }

        [Index(5)]
        [Name("Parent Organization Outer Id")]
        public string ParentOrganizationOuterId { get; set; }

        [Index(6)]
        public override string BusinessCategory { get; set; }

        [Index(7)]
        public override string Description { get; set; }

        [Index(8)]
        public override string Status { get; set; }

        [Index(9)]
        [Optional]
        [Name("Organization Groups")]
        public string OrganizationGroups { get; set; }

        [Index(10)]
        public override string Emails { get; set; }

        [Index(11)]
        public override string Phones { get; set; }

        [Index(12)]
        public override string AddressType { get; set; }

        [Index(13)]
        public override string AddressFirstName { get; set; }

        [Index(14)]
        public override string AddressLastName { get; set; }

        [Index(15)]
        public override string AddressCountry { get; set; }

        [Index(16)]
        public override string AddressCountryCode { get; set; }

        [Index(17)]
        public override string AddressRegion { get; set; }

        [Index(18)]
        public override string AddressRegionCode { get; set; }

        [Index(19)]
        public override string AddressCity { get; set; }

        [Index(20)]
        public override string AddressLine1 { get; set; }

        [Index(21)]
        public override string AddressLine2 { get; set; }

        [Index(22)]
        public override string AddressZipCode { get; set; }

        [Index(23)]
        public override string AddressEmail { get; set; }

        [Index(24)]
        public override string AddressPhone { get; set; }


        public CsvOrganization FromModels(Organization organization, Organization parentOrganization)
        {
            var address = organization.Addresses?.FirstOrDefault();

            Id = organization.Id;
            OuterId = organization.OuterId;
            OrganizationName = organization.Name;
            ParentOrganizationName = parentOrganization?.Name;
            Emails = organization.Emails.IsNullOrEmpty() ? null : string.Join(", ", organization.Emails);
            ParentOrganizationId = organization.ParentId;
            ParentOrganizationOuterId = parentOrganization?.OuterId;
            AddressType = address?.AddressType.ToString();
            AddressFirstName = address?.FirstName;
            AddressLastName = address?.LastName;
            AddressCountry = address?.CountryName;
            AddressCountryCode = address?.CountryCode;
            AddressRegion = address?.RegionName;
            AddressRegionCode = address?.RegionId;
            AddressCity = address?.City;
            AddressLine1 = address?.Line1;
            AddressLine2 = address?.Line2;
            AddressZipCode = address?.PostalCode;
            AddressEmail = address?.Email;
            AddressPhone = address?.Phone;
            Phones = organization.Phones.IsNullOrEmpty() ? null : string.Join(", ", organization.Phones);
            BusinessCategory = organization.BusinessCategory;
            Description = organization.Description;
            Status = organization.Status;
            OrganizationGroups = organization.Groups.IsNullOrEmpty() ? null : string.Join(", ", organization.Groups);

            DynamicProperties = organization.DynamicProperties?.Select(x => x.CloneTyped()).ToArray();

            return this;
        }
    }
}
