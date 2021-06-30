using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.Platform.Core.Security;
using Address = VirtoCommerce.CustomerModule.Core.Model.Address;

namespace VirtoCommerce.CustomerExportImportModule.Core.Models
{
    public sealed class ImportableContact: CsvContact
    {
        public void PatchModel(Contact target)
        {
            target.OuterId = OuterId;
            target.FirstName = ContactFirstName;
            target.LastName = ContactLastName;
            target.FullName = ContactFullName;
            target.Status = ContactStatus;
            target.BirthDate = Birthday;
            target.TimeZone = TimeZone;
            target.Emails = string.IsNullOrEmpty(Emails) ? null : Emails.Split(',').Select(email => email.Trim()).ToList();
            target.Phones = string.IsNullOrEmpty(Phones) ? null : Phones.Split(',').Select(phone => phone.Trim()).ToList();
            target.Groups = string.IsNullOrEmpty(UserGroups) ? null : UserGroups.Split(',').Select(userGroups => userGroups.Trim()).ToList();
            target.Salutation = Salutation;
            target.DefaultLanguage = DefaultLanguage;
            target.TaxPayerId = TaxPayerId;
            target.PreferredCommunication = PreferredCommunication;
            target.PreferredDelivery = PreferredDelivery;
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

            target.SecurityAccounts ??= new List<ApplicationUser>();
            var accountSpecified = new[] { AccountId, AccountLogin, AccountEmail }.Any(accountField => !string.IsNullOrEmpty(accountField));

            if (accountSpecified)
            {
                target.SecurityAccounts.Add(
                    new ApplicationUser
                    {
                        Id = AccountId,
                        StoreId = StoreId,
                        UserName = AccountLogin,
                        Email = AccountEmail,
                        UserType = AccountType,
                        Status = AccountStatus,
                        EmailConfirmed = EmailVerified ?? false
                    }
                );
            }
        }

        public Organization ToOrganization()
        {
            var result = new Organization { Id = OrganizationId, OuterId = OrganizationOuterId, Name = OrganizationName, };

            return result;
        }
    }
}