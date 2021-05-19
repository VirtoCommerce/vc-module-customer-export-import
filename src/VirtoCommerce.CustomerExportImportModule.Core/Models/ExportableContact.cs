using System;
using System.Collections.Generic;
using System.Linq;
using CsvHelper.Configuration.Attributes;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.StoreModule.Core.Model;

namespace VirtoCommerce.CustomerExportImportModule.Core.Models
{
    public sealed class ExportableContact : IExportable, IHasDynamicProperties
    {
        [Name("Contact Id")]
        public string Id { get; set; }
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
        [Name("Address Type")]
        public string AddressType { get; set; }
        [Name("Address First Name")]
        public string AddressFirstName { get; set; }
        [Name("Address Last Name")]
        public string AddressLastName { get; set; }
        [Name("Address Country")]
        public string AddressCountry { get; set; }
        [Name("Address Region")]
        public string AddressRegion { get; set; }
        [Name("Address City")]
        public string AddressCity { get; set; }
        [Name("Address Address Line1")]
        public string AddressAddressLine1 { get; set; }
        [Name("Address Address Line2")]
        public string AddressAddressLine2 { get; set; }
        [Name("Address Zip Code")]
        public string AddressZipCode { get; set; }
        [Name("Address Email")]
        public string AddressEmail { get; set; }
        [Name("Address Phone")]
        public string AddressPhone { get; set; }

        [Ignore]
        public string ObjectType => typeof(ExportableContact).FullName;
        public ICollection<DynamicObjectProperty> DynamicProperties { get; set; }

        public ExportableContact FromModel(Contact contact, Organization organization, Store store)
        {
            var account = contact.SecurityAccounts.FirstOrDefault();
            var address = contact.Addresses.FirstOrDefault();

            var result = new ExportableContact
            {
                Id = contact.Id,
                FirstName = contact.FirstName,
                LastName = contact.LastName,
                FullName = contact.FullName,
                ContactOuterId = contact.OuterId,
                OrganizationId = organization.Id,
                OrganizationOuterId = organization.OuterId,
                OrganizationName = organization.Name,
                AccountId = account?.Id,
                StoreId = account?.StoreId,
                StoreName = store.Name,
                AccountLogin = account?.UserName,
                AccountEmail = account?.StoreId,
                AccountType = account?.UserType,
                AccountStatus = account?.Status,
                EmailVerified = account?.EmailConfirmed,
                ContactStatus = contact.Status,
                AssociatedOrganizationId = contact.AssociatedOrganizations.FirstOrDefault(),
                BirthDate = contact.BirthDate,
                TimeZone = contact.TimeZone,
                Phones = string.Join(",", contact.Phones),
                UserGroups = string.Join(", ", contact.Groups),
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

                DynamicProperties = contact.DynamicProperties?.Select(x => x.Clone() as DynamicObjectProperty)
                    .ToArray()
            };


            return result;
        }
    }
}
