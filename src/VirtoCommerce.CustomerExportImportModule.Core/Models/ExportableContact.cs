using System;
using System.Collections.Generic;
using System.Linq;
using CsvHelper.Configuration.Attributes;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.Platform.Core.DynamicProperties;

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
        public bool EmailVerified { get; set; }
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

        public ExportableContact FromModel(Contact contact, Organization organization)
        {
            var result = new ExportableContact();

            result.Id = contact.Id;
            result.FirstName = contact.FirstName;
            result.LastName = contact.LastName;
            result.FullName = contact.FullName;

            result.DynamicProperties =
                contact.DynamicProperties?.Select(x => x.Clone() as DynamicObjectProperty).ToArray();

            return result;
        }
    }
}
