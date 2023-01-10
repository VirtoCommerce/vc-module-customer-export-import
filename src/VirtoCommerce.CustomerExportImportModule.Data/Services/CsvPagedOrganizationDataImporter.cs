using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Nager.Country;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerExportImportModule.Core.Services;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.AssetsModule.Core.Assets;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CustomerExportImportModule.Data.Services
{
    public sealed class CsvPagedOrganizationDataImporter : CsvPagedDataImporter<ImportableOrganization, Organization>
    {
        private readonly IMemberService _memberService;

        public override string MemberType => nameof(Organization);

        public CsvPagedOrganizationDataImporter(IMemberService memberService, IMemberSearchService memberSearchService, ICsvCustomerDataValidator dataValidator, IValidator<ImportRecord<ImportableOrganization>[]> importOrganizationValidator
            , ICustomerImportPagedDataSourceFactory dataSourceFactory, ICsvCustomerImportReporterFactory importReporterFactory, IBlobUrlResolver blobUrlResolver,
            ICountryProvider countryProvider, ICountriesService countriesService)
        : base(memberSearchService, dataValidator, dataSourceFactory, importOrganizationValidator, importReporterFactory, blobUrlResolver, countryProvider, countriesService)
        {
            _memberService = memberService;
        }

        protected override async Task ProcessChunkAsync(ImportDataRequest request, Action<ImportProgressInfo> progressCallback, ImportRecord<ImportableOrganization>[] importRecords,
            ImportErrorsContext errorsContext, ImportProgressInfo importProgress, ICsvCustomerImportReporter importReporter)
        {
            var importOrganizations = importRecords;

            var internalIds = importOrganizations.Select(x => x.Record?.Id).Distinct()
                .Where(x => !string.IsNullOrEmpty(x))
                .ToArray();

            var outerIds = importOrganizations.Select(x => x.Record?.OuterId).Distinct()
                .Where(x => !string.IsNullOrEmpty(x))
                .ToArray();

            var existedOrganizations =
                (await SearchMembersByIdAndOuterIdAsync(internalIds, outerIds, new[] { nameof(Organization) }, true))
                .OfType<Organization>().ToArray();

            SetIdToNullForNotExisted(importOrganizations, existedOrganizations);

            SetIdToRealForExistedOuterId(importOrganizations, existedOrganizations);

            var validationResult = await ValidateAsync(importOrganizations, importReporter);

            var invalidImportOrganizations = validationResult.Errors
                .Select(x => (x.CustomState as ImportValidationState<ImportableOrganization>)?.InvalidRecord).Distinct().ToArray();

            importOrganizations = importOrganizations.Except(invalidImportOrganizations).ToArray();

            //reduce existed set after validation
            existedOrganizations = existedOrganizations
                .Where(ec => importOrganizations.Any(ic =>
                    ec.Id.EqualsInvariant(ic.Record.Id) || !string.IsNullOrEmpty(ec.OuterId) && ec.OuterId.EqualsInvariant(ic.Record.OuterId)))
                .ToArray();

            var updateImportOrganizations = importOrganizations
                .Where(ic => existedOrganizations.Any(ec =>
                    ec.Id.EqualsInvariant(ic.Record.Id) || (!string.IsNullOrEmpty(ec.OuterId) && ec.OuterId.EqualsInvariant(ic.Record.OuterId))))
                .ToArray();

            existedOrganizations = GetReducedExistedByWrongOuterId(updateImportOrganizations, existedOrganizations).ToArray();

            var createImportOrganizations = importOrganizations.Except(updateImportOrganizations).ToArray();

            var newOrganizations = CreateNewOrganizations(createImportOrganizations);

            PatchExistedOrganizations(existedOrganizations, updateImportOrganizations);

            var organizationsForSave = newOrganizations.Union(existedOrganizations).ToArray();

            await _memberService.SaveChangesAsync(organizationsForSave);

            importProgress.CreatedCount += newOrganizations.Length;
            importProgress.AdditionalLineCount += createImportOrganizations.Length - newOrganizations.Length;
            importProgress.UpdatedCount += existedOrganizations.Length;
        }


        private static void PatchExistedOrganizations(IEnumerable<Organization> existedOrganizations, ImportRecord<ImportableOrganization>[] updateImportOrganizations)
        {
            foreach (var existedOrganization in existedOrganizations)
            {
                var importOrganization = updateImportOrganizations.LastOrDefault(x => existedOrganization.Id.EqualsInvariant(x.Record.Id)
                    || (!string.IsNullOrEmpty(existedOrganization.OuterId) && existedOrganization.OuterId.EqualsInvariant(x.Record.OuterId)));

                importOrganization?.Record.PatchModel(existedOrganization);
            }
        }

        private static Organization[] CreateNewOrganizations(ImportRecord<ImportableOrganization>[] createImportOrganizations) =>
            createImportOrganizations
                .GroupBy(x => (x.Record.Id, x.Record.OuterId, x.Record.RecordName))
                .Where(group => group.Any(x => x.Record.AdditionalLine != true))
                .Select(group =>
                {
                    var organization = AbstractTypeFactory<Organization>.TryCreateInstance();

                    foreach (var importRecord in group.OrderBy(x => x.Record.AdditionalLine == true).ThenBy(x => x.Row))
                    {
                        importRecord.Record.PatchModel(organization);
                    }

                    return organization;
                }).ToArray();
    }
}
