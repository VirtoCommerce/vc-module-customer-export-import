using System.Linq;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DynamicProperties;

namespace VirtoCommerce.CustomerExportImportModule.Core.Models
{
    public sealed class ExportableOrganization: CsvOrganization
    {
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
