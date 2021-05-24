using System;
using System.Collections.Generic;
using CsvHelper.Configuration.Attributes;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.Platform.Core.Security;
using Address = VirtoCommerce.CustomerModule.Core.Model.Address;

namespace VirtoCommerce.CustomerExportImportModule.Core.Models
{
    public sealed class ImportableContact: ImportableMember
    {
        [Name("Contact Id")]
        public override string Id { get; set; }

        [Name("Contact First Name")]
        public string FirstName { get; set; }

        [Name("Contact Last Name")]
        public string LastName { get; set; }

        [Name("Contact Full Name")]
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
        public bool? EmailVerified { get; set; }

        [Name("Contact Status")]
        public string ContactStatus { get; set; }

        [Name("Associated Organization Id")]
        public string AssociatedOrganizationId { get; set; }

        [Name("Birthday")]
        public DateTime? BirthDate { get; set; }

        [Name("TimeZone")]
        public string TimeZone { get; set; }

        [Name("Phones")]
        public string Phones { get; set; }

        [Name("User groups")]
        public string UserGroups { get; set; }

        [Name("Default language")]
        public string DefaultLanguage { get; set; }

        [Name("Taxpayer ID")]
        public string TaxPayerId { get; set; }

        [Name("Preferred communication")]
        public string PreferredCommunication { get; set; }

        [Name("Preferred delivery")]
        public string PreferredDelivery { get; set; }

        public Contact ToContact()
        {
            return new Contact()
            {
                Id = Id,
                FirstName = FirstName,
                LastName = LastName,
                FullName = FullName,
                OuterId = ContactOuterId,
                SecurityAccounts = new List<ApplicationUser>()
                {
                    new ApplicationUser()
                    {
                        Id = AccountId,
                        StoreId = StoreId,
                        UserName = AccountLogin,
                        Email = AccountEmail,
                        UserType = AccountType,
                        Status = AccountStatus,
                        EmailConfirmed = EmailVerified ?? false
                    }
                },
                Status = ContactStatus,
                AssociatedOrganizations = new List<string>() { AssociatedOrganizationId },
                BirthDate = BirthDate,
                TimeZone = TimeZone,
                Phones = Phones?.Split(","),
                Groups = UserGroups?.Split(","),
                Addresses = new List<Address>()
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
            var result = new Organization
            {
                Id = OrganizationId,
                OuterId = OrganizationOuterId,
                Name = OrganizationName,
            };

            return result;
        }
    }
}
