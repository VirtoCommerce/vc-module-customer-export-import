using System;
using System.Collections.Generic;
using System.Linq;
using CsvHelper.Configuration.Attributes;
using Newtonsoft.Json;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.CustomerExportImportModule.Core.Models
{
    public sealed class ImportableContact : CsvContact, IImportable
    {
        [Ignore, JsonIgnore]
        public string RecordName
        {
            get => ContactFullName;
            set => ContactFullName = value;
        }

        [Optional]
        [Name("Account Password")]
        public string Password { get; set; }

        /// <summary>
        /// 'Address Country' from file is not used. It will be set at import process from ISO countries dictionary
        /// by 'Address Code' field. Therefore it is ignored.
        /// </summary>
        [Ignore, JsonIgnore]
        [Name("Address Country")]
        public override string AddressCountry { get; set; }

        public void PatchModel(Contact target)
        {
            if (AdditionalLine != true)
            {
                target.OuterId = OuterId;
                target.FirstName = ContactFirstName;
                target.LastName = ContactLastName;
                target.FullName = ContactFullName;
                target.Status = ContactStatus;
                target.BirthDate = Birthday;
                target.TimeZone = TimeZone;
                target.Salutation = Salutation;
                target.DefaultLanguage = DefaultLanguage;
                target.TaxPayerId = TaxPayerId;
                target.PreferredCommunication = PreferredCommunication;
                target.PreferredDelivery = PreferredDelivery;

                const StringSplitOptions splitOptions = StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries;
                target.Emails = string.IsNullOrEmpty(Emails) ? null : Emails.Split(',', splitOptions);
                target.Phones = string.IsNullOrEmpty(Phones) ? null : Phones.Split(',', splitOptions);

                PatchDynamicProperties(target);

                target.SecurityAccounts ??= new List<ApplicationUser>();
                if ((!string.IsNullOrEmpty(AccountLogin) || !string.IsNullOrEmpty(AccountEmail))
                    && !target.SecurityAccounts.Any(x => x.UserName.EqualsInvariant(AccountLogin) && x.Email.EqualsInvariant(AccountEmail)))
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

            PatchAddresses(target);
        }

        public Organization ToOrganization()
        {
            var result = new Organization { Id = OrganizationId, OuterId = OrganizationOuterId, Name = OrganizationName, };

            return result;
        }
    }
}
