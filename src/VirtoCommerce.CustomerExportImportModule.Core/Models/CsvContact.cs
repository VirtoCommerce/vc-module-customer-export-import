using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CsvHelper.Configuration.Attributes;
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
        [Name("Contact Id")]
        public override string Id { get; set; }

        [Name("Contact First Name")]
        [Required]
        public string FirstName { get; set; }

        [Name("Contact Last Name")]
        [Required]
        public string LastName { get; set; }

        [Name("Contact Full Name")]
        [Required]
        public string FullName { get; set; }

        [Name("Contact Outer Id")]
        public string ContactOuterId { get; set; }

        [Name("Organization Id")]
        public string OrganizationId { get; set; }

        [Name("Organization Outer Id")]
        public string OrganizationOuterId { get; set; }

        [Name("Organization Name")]
        public string OrganizationName { get; set; }

        [Name("Account Id")]
        public string AccountId { get; set; }

        [Name("Account Login")]
        public string AccountLogin { get; set; }

        [Name("Store Id")]
        public string StoreId { get; set; }

        [Name("Store Name")]
        public string StoreName { get; set; }

        [Name("Account Email")]
        public string AccountEmail { get; set; }

        [Name("Account Type")]
        public string AccountType { get; set; }

        [Name("Account Status")]
        public string AccountStatus { get; set; }

        [Name("Email Verified")]
        [BooleanTrueValues("yes", "true")]
        [BooleanFalseValues("no", "false")]
        public bool? EmailVerified { get; set; }

        [Name("Contact Status")]
        public string ContactStatus { get; set; }

        [Name("Associated Organization Ids")]
        public string AssociatedOrganizationIds { get; set; }

        [Name("Birthday")]
        public DateTime? BirthDate { get; set; }

        [Name("TimeZone")]
        public string TimeZone { get; set; }

        [Name("Phones")]
        public string Phones { get; set; }

        [Name("User groups")]
        public string UserGroups { get; set; }

        [Name("Salutation")]
        public string Salutation { get; set; }

        [Name("Default language")]
        public string DefaultLanguage { get; set; }

        [Name("Taxpayer ID")]
        public string TaxPayerId { get; set; }

        [Name("Preferred communication")]
        public string PreferredCommunication { get; set; }

        [Name("Preferred delivery")]
        public string PreferredDelivery { get; set; }

        public CsvContact ToExportableImportableContact(Contact contact, Organization organization, Store store)
        {
            var account = contact.SecurityAccounts?.FirstOrDefault();
            var address = contact.Addresses?.FirstOrDefault();

            Id = contact.Id;
            FirstName = contact.FirstName;
            LastName = contact.LastName;
            FullName = contact.FullName;
            ContactOuterId = contact.OuterId;
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
            BirthDate = contact.BirthDate;
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
            AddressAddressLine1 = address?.Line1;
            AddressAddressLine2 = address?.Line2;
            AddressZipCode = address?.PostalCode;
            AddressEmail = address?.Email;
            AddressPhone = address?.Phone;

            DynamicProperties = contact.DynamicProperties?.Select(x => x.Clone() as DynamicObjectProperty)
                .ToArray();

            return this;
        }
        
        public Contact ToContact()
        {
            return new Contact
            {
                Id = Id,
                FirstName = FirstName,
                LastName = LastName,
                FullName = FullName,
                OuterId = ContactOuterId,
                SecurityAccounts = AccountId != null
                    ? new List<ApplicationUser>
                    {
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
                    }
                    : null,
                Status = ContactStatus,
                AssociatedOrganizations = AssociatedOrganizationIds?.Split(", ").ToList(),
                BirthDate = BirthDate,
                TimeZone = TimeZone,
                Phones = Phones?.Split(", "),
                Groups = UserGroups?.Split(", "),
                Addresses = new List<Address>
                {
                    new Address
                    {
                        AddressType = Enum.Parse<AddressType>(AddressType),
                        FirstName = AddressFirstName,
                        LastName = AddressLastName,
                        RegionName = AddressCountry,
                        City = AddressCity,
                        Line1 = AddressAddressLine1,
                        Line2 = AddressAddressLine2,
                        Zip = AddressZipCode,
                        Email = AddressEmail,
                        Phone = AddressPhone,
                    }
                },
                DynamicProperties = DynamicProperties
            };
        }

        public Organization ToOrganization()
        {
            var result = new Organization { Id = OrganizationId, OuterId = OrganizationOuterId, Name = OrganizationName, };

            return result;
        }
    }
}