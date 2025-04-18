using System;
using System.ComponentModel.DataAnnotations;
using CsvHelper.Configuration.Attributes;
using Newtonsoft.Json;

namespace VirtoCommerce.CustomerExportImportModule.Core.Models
{
    public abstract class CsvContact : CsvMember
    {
        private string _id;

        [Optional]
        [JsonProperty("contactId")]
        [Name("Contact Id")]
        public override string Id
        {
            get => _id;
            set => _id = value?.Trim();
        }

        [Name("Contact First Name")]
        [Required]
        public virtual string ContactFirstName { get; set; }

        [Name("Contact Last Name")]
        [Required]
        public virtual string ContactLastName { get; set; }

        [Name("Contact Full Name")]
        [Required]
        public virtual string ContactFullName { get; set; }

        [Optional]
        [JsonProperty("contactOuterId")]
        [Name("Contact Outer Id")]
        public override string OuterId { get; set; }

        [Optional]
        [Name("Contact Status")]
        public virtual string ContactStatus { get; set; }

        [Optional]
        [Name("Contact Phones")]
        public override string Phones { get; set; }

        [Optional]
        [Name("Contact Associated Organization Ids")]
        public virtual string AssociatedOrganizationIds { get; set; }

        [Optional]
        [Name("Contact User Groups")]
        public virtual string UserGroups { get; set; }

        [Optional]
        [Name("Contact Emails")]
        public override string Emails { get; set; }

        [Optional]
        [Name("Contact Birthday")]
        public virtual DateTime? Birthday { get; set; }

        [Optional]
        [Name("Contact TimeZone")]
        public virtual string TimeZone { get; set; }

        [Optional]
        [Name("Contact Salutation")]
        public virtual string Salutation { get; set; }

        [Optional]
        [Name("Contact Default language")]
        public virtual string DefaultLanguage { get; set; }

        [Optional]
        [Name("Contact Taxpayer ID")]
        public virtual string TaxPayerId { get; set; }

        [Optional]
        [Name("Contact Preferred communication")]
        public virtual string PreferredCommunication { get; set; }

        [Optional]
        [Name("Contact Preferred delivery")]
        public virtual string PreferredDelivery { get; set; }

        [Optional]
        [Name("Account Id")]
        public virtual string AccountId { get; set; }

        [Optional]
        [Name("Organization Id")]
        public virtual string OrganizationId { get; set; }

        [Optional]
        [Name("Organization Outer Id")]
        public virtual string OrganizationOuterId { get; set; }

        [Optional]
        [Name("Organization Name")]
        public virtual string OrganizationName { get; set; }

        [Optional]
        [Name("Account Login")]
        public virtual string AccountLogin { get; set; }

        [Optional]
        [Name("Account Store Id")]
        public virtual string StoreId { get; set; }

        [Optional]
        [Name("Account Store Name")]
        public virtual string StoreName { get; set; }

        [Optional]
        [Name("Account Email")]
        public virtual string AccountEmail { get; set; }

        [Optional]
        [Name("Account Type")]
        public virtual string AccountType { get; set; }

        [Optional]
        [Name("Account Status")]
        public virtual string AccountStatus { get; set; }

        [Optional]
        [Name("Account Email Verified")]
        [BooleanTrueValues("True", "Yes")]
        [BooleanFalseValues("False", "No")]
        public virtual bool? EmailVerified { get; set; }
    }
}
