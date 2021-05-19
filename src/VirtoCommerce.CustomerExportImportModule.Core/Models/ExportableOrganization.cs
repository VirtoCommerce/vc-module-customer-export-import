using System.Linq;
using CsvHelper.Configuration.Attributes;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.Platform.Core.DynamicProperties;

namespace VirtoCommerce.CustomerExportImportModule.Core.Models
{
    public sealed class ExportableOrganization : ExportableMember
    {
        [Name("Organization Id")]
        public override string Id { get; set; }

        [Name("Organization Outer Id")]
        public string OrganizationOuterId { get; set; }

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

        public ExportableOrganization FromModel(Organization organization, Organization parentOrganization)
        {
            var address = organization.Addresses.FirstOrDefault();

            var result = new ExportableOrganization
            {
                Id = organization.Id,
                OrganizationName = organization.Name,
                OrganizationOuterId = organization.OuterId,
                ParentOrganizationName = parentOrganization.Name,
                ParentOrganizationId = organization.ParentId,
                ParentOrganizationOuterId = parentOrganization.OuterId,
                AddressType = address?.AddressType.ToString(),
                AddressFirstName = address?.FirstName,
                AddressLastName = address?.LastName,
                AddressCountry = address?.RegionName,
                AddressCity = address?.City,
                AddressAddressLine1 = address?.Line1,
                AddressAddressLine2 = address?.Line2,
                AddressZipCode = address?.Zip,
                AddressEmail = address?.Email,
                AddressPhone = address?.Phone,
                Phones = string.Join(",", organization.Phones),
                BusinessCategory = organization.BusinessCategory,
                Description = organization.Description,
                OrganizationGroups = string.Join(", ", organization.Groups),
                DynamicProperties = organization.DynamicProperties?.Select(x => x.Clone() as DynamicObjectProperty)
                    .ToArray()
            };

            return result;
        }
    }
}
