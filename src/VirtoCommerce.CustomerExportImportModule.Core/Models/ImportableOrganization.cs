using System;
using System.Collections.Generic;
using System.Linq;
using CsvHelper.Configuration.Attributes;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.CustomerModule.Core.Model;
using Address = VirtoCommerce.CustomerModule.Core.Model.Address;

namespace VirtoCommerce.CustomerExportImportModule.Core.Models
{
    public sealed class ImportableOrganization : CsvOrganization
    {
        /// <summary>
        /// 'Address Country' from file is not used. It will be set at import process from ISO countries dictionary
        /// by 'Address Code' field. Therefore it is ignored.
        /// </summary>
        [Ignore]
        [Name("Address Country")]
        public override string AddressCountry { get; set; }

        public void PatchModel(Organization target)
        {
            target.OuterId = OuterId;
            target.Name = OrganizationName;
            target.Emails = string.IsNullOrEmpty(Emails) ? null : Emails.Split(',').Select(email => email.Trim()).ToList();
            target.Phones = string.IsNullOrEmpty(Phones) ? null : Phones.Split(',').Select(phone => phone.Trim()).ToList();
            target.BusinessCategory = BusinessCategory;
            target.Description = Description;
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
