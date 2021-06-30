using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CsvHelper.Configuration.Attributes;
using Newtonsoft.Json;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DynamicProperties;
using Address = VirtoCommerce.CustomerModule.Core.Model.Address;

namespace VirtoCommerce.CustomerExportImportModule.Core.Models
{
    public sealed class CsvOrganization : CsvMember
    {
        private string _id;

        [Optional]
        [JsonProperty("organizationId")]
        [Name("Organization Id")]
        public override string Id
        {
            get => _id;
            set => _id = value?.Trim();
        }

        [Optional]
        [JsonProperty("organizationOuterId")]
        [Name("Organization Outer Id")]
        public override string OuterId { get; set; }

        [Required]
        [Name("Organization Name")]
        public string OrganizationName { get; set; }

        [Optional]
        [Name("Parent Organization Name")]
        public string ParentOrganizationName { get; set; }

        [Optional]
        [Name("Parent Organization Id")]
        public string ParentOrganizationId { get; set; }

        [Optional]
        [Name("Parent Organization Outer Id")]
        public string ParentOrganizationOuterId { get; set; }

        [Optional]
        [Name("Business category")]
        public string BusinessCategory { get; set; }

        [Optional]
        [Name("Description")]
        public string Description { get; set; }

        [Optional]
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

        public void PatchOrganization(Organization target)
        {
            target.OuterId = OuterId;
            target.Name = OrganizationName;
            target.Phones = string.IsNullOrEmpty(Phones) ? null : Phones.Split(',').Select(phone => phone.Trim()).ToList();
            target.BusinessCategory = BusinessCategory;
            target.Description = Description;
            target.Groups = string.IsNullOrEmpty(OrganizationGroups) ? null : OrganizationGroups.Split(',').Select(organizationGroups => organizationGroups.Trim()).ToList();
            target.DynamicProperties = DynamicProperties;

            target.Addresses ??= new List<Address>();
            var isAddressSpecified = new[] { AddressCountry, AddressCountryCode, AddressRegion, AddressCity, AddressLine1, AddressLine2, AddressZipCode }.Any(addressField => !string.IsNullOrEmpty(addressField));

            if (isAddressSpecified)
            {
                target.Addresses.Add(new Address
                {
                    AddressType = !string.IsNullOrEmpty(AddressType) ? Enum.Parse<AddressType>(AddressType) : CoreModule.Core.Common.AddressType.BillingAndShipping,
                    FirstName = AddressFirstName,
                    LastName = AddressLastName,
                    CountryName = AddressCountry,
                    CountryCode = AddressCountryCode,
                    RegionName = AddressRegion,
                    City = AddressCity,
                    Line1 = AddressLine1,
                    Line2 = AddressLine2,
                    PostalCode = AddressZipCode,
                    Email = AddressEmail,
                    Phone = AddressPhone,
                });
            }
        }
    }
}
