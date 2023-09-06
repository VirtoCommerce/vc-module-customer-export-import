using System;
using CsvHelper.Configuration.Attributes;
using Newtonsoft.Json;
using VirtoCommerce.CustomerModule.Core.Model;

namespace VirtoCommerce.CustomerExportImportModule.Core.Models
{
    public sealed class ImportableOrganization : CsvOrganization, IImportable
    {
        [Ignore, JsonIgnore]
        public string RecordName
        {
            get => OrganizationName;
            set => OrganizationName = value;
        }

        [Optional]
        [Name("Parent Organization Id")]
        public string ParentOrganizationId { get; set; }

        [Optional]
        [Name("Parent Organization Outer Id")]
        public string ParentOrganizationOuterId { get; set; }

        [Optional]
        [Name("Parent Organization Name")]
        public string ParentOrganizationName { get; set; }

        [Optional]
        [Name("Organization Groups")]
        public string OrganizationGroups { get; set; }

        /// <summary>
        /// 'Address Country' from file is not used. It will be set at import process from ISO countries dictionary
        /// by 'Address Code' field. Therefore it is ignored.
        /// </summary>
        [Ignore, JsonIgnore]
        [Name("Address Country")]
        public override string AddressCountry { get; set; }

        public void PatchModel(Organization target)
        {
            if (AdditionalLine != true)
            {
                target.OuterId = OuterId;
                target.Name = OrganizationName;
                target.BusinessCategory = BusinessCategory;
                target.Description = Description;
                target.Status = Status;

                const StringSplitOptions splitOptions = StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries;
                target.Emails = string.IsNullOrEmpty(Emails) ? null : Emails.Split(',', splitOptions);
                target.Phones = string.IsNullOrEmpty(Phones) ? null : Phones.Split(',', splitOptions);
                target.Groups = string.IsNullOrEmpty(OrganizationGroups) ? null : OrganizationGroups.Split(',', splitOptions);

                PatchDynamicProperties(target);
            }

            PatchAddresses(target);
        }
    }
}
