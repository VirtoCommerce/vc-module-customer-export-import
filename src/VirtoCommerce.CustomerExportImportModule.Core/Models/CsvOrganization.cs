using System.ComponentModel.DataAnnotations;
using CsvHelper.Configuration.Attributes;
using Newtonsoft.Json;

namespace VirtoCommerce.CustomerExportImportModule.Core.Models
{
    public abstract class CsvOrganization : CsvMember
    {
        private string _id;

        [Optional]
        [JsonProperty("organizationId")]
        [Name("Organization Id")]
        public override string Id
        {
            get => _id;
            set => _id = value?.Trim();
        }

        [Optional]
        [JsonProperty("organizationOuterId")]
        [Name("Organization Outer Id")]
        public override string OuterId { get; set; }

        [Required]
        [Name("Organization Name")]
        public virtual string OrganizationName { get; set; }

        [Optional]
        [Name("Business category")]
        public virtual string BusinessCategory { get; set; }

        [Optional]
        [Name("Description")]
        public virtual string Description { get; set; }

        [Optional]
        [Name("Status")]
        public virtual string Status { get; set; }
    }
}
