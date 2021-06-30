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
        public string OrganizationName { get; set; }

        [Optional]
        [Name("Parent Organization Name")]
        public string ParentOrganizationName { get; set; }

        [Optional]
        [Name("Parent Organization Id")]
        public string ParentOrganizationId { get; set; }

        [Optional]
        [Name("Parent Organization Outer Id")]
        public string ParentOrganizationOuterId { get; set; }

        [Optional]
        [Name("Business category")]
        public string BusinessCategory { get; set; }

        [Optional]
        [Name("Description")]
        public string Description { get; set; }

        [Optional]
        [Name("Organization Groups")]
        public string OrganizationGroups { get; set; }
    }
}
