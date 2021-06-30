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
        [Name("Organization Id")]
        public virtual string OrganizationId { get; set; }
        
        [Optional]
        [Name("Organization Outer Id")]
        public virtual string OrganizationOuterId { get; set; }
        
        [Optional]
        [Name("Organization Name")]
        public virtual string OrganizationName { get; set; }

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
        [Name("Birthday")]
        public DateTime? Birthday { get; set; }

        [Optional]
        [Name("TimeZone")]
        public string TimeZone { get; set; }

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
    }
}
