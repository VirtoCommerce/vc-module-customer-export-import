using System;
using System.Collections.Generic;
using System.Linq;
using CsvHelper.Configuration.Attributes;
using Newtonsoft.Json;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.CustomerModule.Core.Model;
using Address = VirtoCommerce.CustomerModule.Core.Model.Address;

namespace VirtoCommerce.CustomerExportImportModule.Core.Models
{
    public sealed class ImportableOrganization : CsvOrganization, IImportable
    {
        [Ignore, JsonIgnore]
        public string RecordName
        {
            get => OrganizationName;
            set => OrganizationName = value;
        }

        [Optional]
        [Name("Parent Organization Id")]
        public string ParentOrganizationId { get; set; }

        [Optional]
        [Name("Parent Organization Outer Id")]
        public string ParentOrganizationOuterId { get; set; }

        [Optional]
        [Name("Parent Organization Name")]
        public string ParentOrganizationName { get; set; }

        [Optional]
        [Name("Organization Groups")]
        public string OrganizationGroups { get; set; }

        /// <summary>
        /// 'Address Country' from file is not used. It will be set at import process from ISO countries dictionary
        /// by 'Address Code' field. Therefore it is ignored.
        /// </summary>
        [Ignore, JsonIgnore]
        [Name("Address Country")]
        public override string AddressCountry { get; set; }

        public void PatchModel(Organization target)
        {
            if (AdditionalLine != true)
            {
                target.OuterId = OuterId;
                target.Name = OrganizationName;
                target.BusinessCategory = BusinessCategory;
                target.Description = Description;

                const StringSplitOptions splitOptions = StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries;
                target.Emails = string.IsNullOrEmpty(Emails) ? null : Emails.Split(',', splitOptions);
                target.Phones = string.IsNullOrEmpty(Phones) ? null : Phones.Split(',', splitOptions);
                target.Groups = string.IsNullOrEmpty(OrganizationGroups) ? null : OrganizationGroups.Split(',', splitOptions);

                PatchDynamicProperties(target);
            }

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
