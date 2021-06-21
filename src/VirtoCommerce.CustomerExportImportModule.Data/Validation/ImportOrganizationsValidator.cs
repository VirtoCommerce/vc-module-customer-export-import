using FluentValidation;
using VirtoCommerce.CustomerExportImportModule.Core.Models;

namespace VirtoCommerce.CustomerExportImportModule.Data.Validation
{
    public sealed class ImportOrganizationsValidator : AbstractValidator<ImportRecord<CsvOrganization>[]>
    {
        public ImportOrganizationsValidator()
        {
            AttachValidators();
        }

        private void AttachValidators()
        {
            // It will be implemented at another US
        }
    }
}
