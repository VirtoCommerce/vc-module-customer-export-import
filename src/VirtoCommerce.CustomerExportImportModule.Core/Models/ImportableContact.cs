using System.Collections.Generic;
using System.Linq;
using CsvHelper.Configuration.Attributes;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Security;
using Address = VirtoCommerce.CustomerModule.Core.Model.Address;

namespace VirtoCommerce.CustomerExportImportModule.Core.Models
{
    public sealed class ImportableContact : CsvContact
    {
        [Optional]
        [Name("Account Password")]
        public string Password { get; set; }

        /// <summary>
        /// 'Address Country' from file is not used. It will be set at import process from ISO countries dictionary
        /// by 'Address Code' field. Therefore it is ignored.
        /// </summary>
        [Ignore]
        [Name("Address Country")]
        public override string AddressCountry { get; set; }

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
                    AddressType = EnumUtility.SafeParse(AddressType, CoreModule.Core.Common.AddressType.BillingAndShipping),
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

            target.SecurityAccounts = new List<ApplicationUser>();
            var accountSpecified = new[] { AccountLogin, AccountEmail }.Any(accountField => !string.IsNullOrEmpty(accountField));

            if (accountSpecified)
            {
                target.SecurityAccounts.Add(
                    new ApplicationUser
                    {
                        StoreId = StoreId,
                        UserName = AccountLogin,
                        Email = AccountEmail,
                        UserType = AccountType,
                        Status = AccountStatus,
                        EmailConfirmed = EmailVerified ?? false,
                        Password = Password,
                        PasswordExpired = true,
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
