using System.Collections.Generic;
using System.Linq;
using CsvHelper.Configuration.Attributes;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.Platform.Core.DynamicProperties;

namespace VirtoCommerce.CustomerExportImportModule.Core.Models
{
    public sealed class ExportableOrganization : ExportableMember
    {
        [Name("Organization Id")]
        public override string Id { get; set; }

        [Name("Organization Outer Id")]
        public string OrganizationOuterId { get; set; }

        [Name("Organization Name")]
        public string OrganizationName { get; set; }

        [Name("Parent Organization Name")]
        public string ParentOrganizationName { get; set; }

        [Name("Parent Organization Id")]
        public string ParentOrganizationId { get; set; }

        [Name("Parent Organization Outer Id")]
        public string ParentOrganizationOuterId { get; set; }

        [Name("Phones")]
        public string Phones { get; set; }

        [Name("Business category")]
        public string BusinessCategory { get; set; }

        [Name("Description")]
        public string Description { get; set; }

        [Name("Organization Groups")]
        public string OrganizationGroups { get; set; }

        public ExportableOrganization FromModel(Organization organization)
        {
            var result = new ExportableOrganization();

            result.Id = organization.Id;
            result.OrganizationName = organization.Name;

            result.DynamicProperties =
                organization.DynamicProperties?.Select(x => x.Clone() as DynamicObjectProperty).ToArray();

            return result;
        }
    }
}
