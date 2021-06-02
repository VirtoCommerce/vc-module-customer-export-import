using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CsvHelper.Configuration.Attributes;
using Newtonsoft.Json;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.StoreModule.Core.Model;
using Address = VirtoCommerce.CustomerModule.Core.Model.Address;

namespace VirtoCommerce.CustomerExportImportModule.Core.Models
{
    public sealed class CsvContact : CsvMember
    {
        [Optional]
        [JsonProperty("contactId")]
        [Name("Contact Id")]
        public override string Id { get; set; }

        [Name("Contact First Name")]
        [Required]
        public string ContactFirstName { get; set; }

        [Name("Contact Last Name")]
        [Required]
        public string ContactLastName { get; set; }

        [Name("Contact Full Name")]
        [Required]
        public string ContactFullName { get; set; }

        [Optional]
        [JsonProperty("contactOuterId")]
        [Name("Contact Outer Id")]
        public override string OuterId { get; set; }

        [Optional]
        [Name("Organization Id")]
        public string OrganizationId { get; set; }

        [Optional]
        [Name("Organization Outer Id")]
        public string OrganizationOuterId { get; set; }

        [Optional]
        [Name("Organization Name")]
        public string OrganizationName { get; set; }

        [Optional]
        [Name("Account Id")]
        public string AccountId { get; set; }

        [Optional]
        [Name("Account Login")]
        public string AccountLogin { get; set; }

        [Optional]
        [Name("Store Id")]
        public string StoreId { get; set; }

        [Optional]
        [Name("Store Name")]
        public string StoreName { get; set; }

        [Optional]
        [Name("Account Email")]
        public string AccountEmail { get; set; }

        [Optional]
        [Name("Account Type")]
        public string AccountType { get; set; }

        [Optional]
        [Name("Account Status")]
        public string AccountStatus { get; set; }

        [Optional]
        [Name("Email Verified")]
        [BooleanTrueValues("yes", "true")]
        [BooleanFalseValues("no", "false")]
        public bool? EmailVerified { get; set; }

        [Optional]
        [Name("Contact Status")]
        public string ContactStatus { get; set; }

        [Optional]
        [Name("Associated Organization Ids")]
        public string AssociatedOrganizationIds { get; set; }

        [Optional]
        [Name("Birthday")]
        public DateTime? Birthday { get; set; }

        [Optional]
        [Name("TimeZone")]
        public string TimeZone { get; set; }

        [Optional]
        [Name("Phones")]
        public string Phones { get; set; }

        [Optional]
        [Name("User groups")]
        public string UserGroups { get; set; }

        [Optional]
        [Name("Salutation")]
        public string Salutation { get; set; }

        [Optional]
        [Name("Default language")]
        public string DefaultLanguage { get; set; }

        [Optional]
        [Name("Taxpayer ID")]
        public string TaxPayerId { get; set; }

        [Optional]
        [Name("Preferred communication")]
        public string PreferredCommunication { get; set; }

        [Optional]
        [Name("Preferred delivery")]
        public string PreferredDelivery { get; set; }

        public CsvContact ToExportableImportableContact(Contact contact, Organization organization, Store store)
        {
            var account = contact.SecurityAccounts?.FirstOrDefault();
            var address = contact.Addresses?.FirstOrDefault();

            Id = contact.Id;
            OuterId = contact.OuterId;
            ContactFirstName = contact.FirstName;
            ContactLastName = contact.LastName;
            ContactFullName = contact.FullName;
            OrganizationId = organization?.Id;
            OrganizationOuterId = organization?.OuterId;
            OrganizationName = organization?.Name;
            AccountId = account?.Id;
            StoreId = account?.StoreId;
            StoreName = store?.Name;
            AccountLogin = account?.UserName;
            AccountEmail = account?.Email;
            AccountType = account?.UserType;
            AccountStatus = account?.Status;
            EmailVerified = account?.EmailConfirmed;
            ContactStatus = contact.Status;
            AssociatedOrganizationIds = contact.AssociatedOrganizations.IsNullOrEmpty() ? null : string.Join(", ", contact.AssociatedOrganizations);
            Birthday = contact.BirthDate;
            TimeZone = contact.TimeZone;
            Phones = contact.Phones.IsNullOrEmpty() ? null : string.Join(", ", contact.Phones);
            UserGroups = contact.Groups.IsNullOrEmpty() ? null : string.Join(", ", contact.Groups);
            Salutation = contact.Salutation;
            DefaultLanguage = contact.DefaultLanguage;
            TaxPayerId = contact.TaxPayerId;
            PreferredCommunication = contact.PreferredCommunication;
            PreferredDelivery = contact.PreferredDelivery;
            AddressType = address?.AddressType.ToString();
            AddressFirstName = address?.FirstName;
            AddressLastName = address?.LastName;
            AddressCountry = address?.CountryName;
            AddressRegion = address?.RegionName;
            AddressCity = address?.City;
            AddressLine1 = address?.Line1;
            AddressLine2 = address?.Line2;
            AddressZipCode = address?.PostalCode;
            AddressEmail = address?.Email;
            AddressPhone = address?.Phone;

            DynamicProperties = contact.DynamicProperties?.Select(x => x.Clone() as DynamicObjectProperty)
                .ToArray();

            return this;
        }

        public Contact PatchContact(Contact target)
        {
            target.OuterId = OuterId;
            target.FirstName = ContactFirstName;
            target.LastName = ContactLastName;
            target.FullName = ContactFullName;
            target.Status = ContactStatus;
            target.AssociatedOrganizations = AssociatedOrganizationIds?.Split(", ").ToList();
            target.BirthDate = Birthday;
            target.TimeZone = TimeZone;
            target.Phones = Phones?.Split(", ");
            target.Groups = UserGroups?.Split(", ");
            target.Salutation = Salutation;
            target.DefaultLanguage = DefaultLanguage;
            target.TaxPayerId = TaxPayerId;
            target.PreferredCommunication = PreferredCommunication;
            target.PreferredDelivery = PreferredDelivery;
            target.DynamicProperties = DynamicProperties;

            var isAddressSpecified = new[] { AddressCountry, AddressRegion, AddressCity, AddressLine1, AddressLine2, AddressZipCode }.Any(addressField => addressField != null);
            if (isAddressSpecified)
            {
                target.Addresses.Add(new Address
                {
                    AddressType = AddressType != null ? Enum.Parse<AddressType>(AddressType) : CoreModule.Core.Common.AddressType.BillingAndShipping,
                    FirstName = AddressFirstName,
                    LastName = AddressLastName,
                    CountryName = AddressCountry,
                    RegionName = AddressRegion,
                    City = AddressCity,
                    Line1 = AddressLine1,
                    Line2 = AddressLine2,
                    PostalCode = AddressZipCode,
                    Email = AddressEmail,
                    Phone = AddressPhone,
                });
            }


            var accountSpecified = new[] { AccountId, AccountLogin, AccountEmail }.Any(accountField => accountField != null);
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

            return target;
        }

        public Organization ToOrganization()
        {
            var result = new Organization { Id = OrganizationId, OuterId = OrganizationOuterId, Name = OrganizationName, };

            return result;
        }
    }
}
