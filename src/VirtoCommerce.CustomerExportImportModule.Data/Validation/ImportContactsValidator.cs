using System.IO;
using FluentValidation;
using Newtonsoft.Json;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerExportImportModule.Data.Models;

namespace VirtoCommerce.CustomerExportImportModule.Data.Validation
{
    public sealed class ImportContactsValidator: AbstractValidator<ImportRecord<CsvContact>[]>
    {
        public ImportContactsValidator()
        {
            AttachValidators();
        }

        private void AttachValidators()
        {
            var filePath = Path.GetFullPath("app_data/countries.json");
            var countries = JsonConvert.DeserializeObject<Country[]>(File.ReadAllText(filePath));

            RuleFor(importRecords => importRecords).SetValidator(_ => new ImportEntitiesAreNotDuplicatesValidator<CsvContact>());
            RuleForEach(importRecords => importRecords).SetValidator(new ImportMemberValidator<CsvContact>());
            RuleForEach(importRecords => importRecords).SetValidator(new ImportAddressValidator<CsvContact>(countries));
            RuleForEach(importRecords => importRecords).SetValidator(new ImportDynamicPropertiesValidator());
            RuleForEach(importRecords => importRecords).SetValidator(new ImportContactValidator());
        }
    }
}
