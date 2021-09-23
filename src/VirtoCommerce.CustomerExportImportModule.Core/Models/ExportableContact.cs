using System.Linq;
using CsvHelper.Configuration.Attributes;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.StoreModule.Core.Model;

namespace VirtoCommerce.CustomerExportImportModule.Core.Models
{
    public sealed class ExportableContact : CsvContact
    {
        [Index(0)]
        public override string Id { get; set; }

        [Index(1)]
        public override string ContactFirstName { get; set; }

        [Index(2)]
        public override string ContactLastName { get; set; }

        [Index(3)]
        public override string ContactFullName { get; set; }

        [Index(4)]
        public override string OuterId { get; set; }

        [Index(5)]
        public override string OrganizationId { get; set; }

        [Index(6)]
        public override string OrganizationOuterId { get; set; }

        [Index(7)]
        public override string OrganizationName { get; set; }

        [Index(8)]
        [Optional]
        [Name("Contact Associated Organization Ids")]
        public string AssociatedOrganizationIds { get; set; }

        [Index(9)]
        [Optional]
        [Name("Contact User groups")]
        public string UserGroups { get; set; }

        [Index(10)]
        [Optional]
        [Name("Account Id")]
        public string AccountId { get; set; }

        public ExportableContact FromModels(Contact contact, Organization organization, Store store)
        {
            var account = contact.SecurityAccounts?.FirstOrDefault();
            var address = contact.Addresses?.FirstOrDefault();

            Id = contact.Id;
            OuterId = contact.OuterId;
            ContactFirstName = contact.FirstName;
            ContactLastName = contact.LastName;
            ContactFullName = contact.FullName;
            Emails = contact.Emails.IsNullOrEmpty() ? null : string.Join(", ", contact.Emails);
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
            AddressCountryCode = address?.CountryCode;
            AddressRegion = address?.RegionName;
            AddressCity = address?.City;
            AddressLine1 = address?.Line1;
            AddressLine2 = address?.Line2;
            AddressZipCode = address?.PostalCode;
            AddressEmail = address?.Email;
            AddressPhone = address?.Phone;

            DynamicProperties = contact.DynamicProperties?.Select(x => x.Clone() as DynamicObjectProperty).ToArray();

            return this;
        }
    }
}
