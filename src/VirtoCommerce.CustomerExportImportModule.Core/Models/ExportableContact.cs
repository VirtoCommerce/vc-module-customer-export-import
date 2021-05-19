using System;
using System.Linq;
using CsvHelper.Configuration.Attributes;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.Platform.Core.DynamicProperties;

namespace VirtoCommerce.CustomerExportImportModule.Core.Models
{
    public sealed class ExportableContact : ExportableMember
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

        public ExportableContact FromModel(Contact contact, Organization organization)
        {
            var result = new ExportableContact();

            result.Id = contact.Id;
            result.FirstName = contact.FirstName;
            result.LastName = contact.LastName;
            result.FullName = contact.FullName;

            result.OrganizationId = organization.Id;
            result.OrganizationName = organization.Name;

            result.DynamicProperties =
                contact.DynamicProperties?.Select(x => x.Clone() as DynamicObjectProperty).ToArray();

            return result;
        }
    }
}
