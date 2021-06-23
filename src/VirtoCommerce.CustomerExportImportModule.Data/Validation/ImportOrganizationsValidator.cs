using FluentValidation;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CustomerExportImportModule.Data.Validation
{
    public sealed class ImportOrganizationsValidator : AbstractValidator<ImportRecord<CsvOrganization>[]>
    {
        private readonly ICountriesService _countriesService;

        public ImportOrganizationsValidator(ICountriesService countriesService)
        {
            _countriesService = countriesService;
            AttachValidators();
        }

        private void AttachValidators()
        {
            RuleFor(importRecords => importRecords).SetValidator(_ => new ImportEntitiesAreNotDuplicatesValidator<CsvOrganization>());
            RuleForEach(importRecords => importRecords).SetValidator(new ImportMemberValidator<CsvOrganization>(_countriesService));
            RuleForEach(importRecords => importRecords).SetValidator(new ImportOrganizationValidator());
        }
    }
}
