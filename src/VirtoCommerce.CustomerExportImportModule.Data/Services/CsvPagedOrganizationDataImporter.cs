using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerExportImportModule.Core.Services;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CustomerExportImportModule.Data.Services
{
    public sealed class CsvPagedOrganizationDataImporter : CsvPagedDataImporter<CsvOrganization, Organization>
    {
        private readonly IMemberService _memberService;

        public override string MemberType => nameof(Organization);

        public CsvPagedOrganizationDataImporter(IMemberService memberService, IMemberSearchService memberSearchService, ICsvCustomerDataValidator dataValidator, IValidator<ImportRecord<CsvOrganization>[]> importOrganizationValidator
            , ICustomerImportPagedDataSourceFactory dataSourceFactory, ICsvCustomerImportReporterFactory importReporterFactory, IBlobUrlResolver blobUrlResolver)
        : base(memberSearchService, dataValidator, dataSourceFactory, importOrganizationValidator, importReporterFactory, blobUrlResolver)
        {
            _memberService = memberService;
        }

        protected override async Task ProcessChunkAsync(ImportDataRequest request, Action<ImportProgressInfo> progressCallback, ICustomerImportPagedDataSource<CsvOrganization> dataSource,
            ImportErrorsContext errorsContext, ImportProgressInfo importProgress, ICsvCustomerImportReporter importReporter)
        {
            var importOrganizations = dataSource.Items
                // expect records that was parsed with errors
                .Where(importContact => !errorsContext.ErrorsRows.Contains(importContact.Row))
                .ToArray();

            try
            {
                var internalIds = importOrganizations.Select(x => x.Record?.Id).Distinct()
                    .Where(x => !x.IsNullOrEmpty())
                    .ToArray();

                var outerIds = importOrganizations.Select(x => x.Record?.OuterId).Distinct()
                    .Where(x => !x.IsNullOrEmpty())
                    .ToArray();

                var existedOrganizations =
                    (await SearchMembersByIdAndOuterIdAsync(internalIds, outerIds, new[] { nameof(Organization) }, true))
                    .OfType<Organization>().ToArray();

                SetIdToNullForNotExisted(importOrganizations, existedOrganizations);

                var validationResult = await ValidateAsync(importOrganizations, importReporter);

                var invalidImportOrganizations = validationResult.Errors
                    .Select(x => (x.CustomState as ImportValidationState<CsvOrganization>)?.InvalidRecord).Distinct().ToArray();

                importOrganizations = importOrganizations.Except(invalidImportOrganizations).ToArray();

                //reduce existed set after validation
                existedOrganizations = existedOrganizations.Where(ec => importOrganizations.Any(ic =>
                        ec.Id.EqualsInvariant(ic.Record.Id)
                        || !string.IsNullOrEmpty(ec.OuterId) && ec.OuterId.EqualsInvariant(ic.Record.OuterId)))
                    .ToArray();

                var updateImportOrganizations = importOrganizations.Where(x => existedOrganizations.Any(ec =>
                    ec.Id.EqualsInvariant(x.Record.Id)
                    || (!ec.OuterId.IsNullOrEmpty() && ec.OuterId.EqualsInvariant(x.Record.OuterId)))
                ).ToArray();

                var createImportOrganizations = importOrganizations.Except(updateImportOrganizations).ToArray();

                var newOrganizations = CreateNewOrganizations(createImportOrganizations);

                PatchExistedOrganizations(existedOrganizations, updateImportOrganizations);

                var organizationsForSave = newOrganizations.Union(existedOrganizations).ToArray();

                await _memberService.SaveChangesAsync(organizationsForSave);

                importProgress.ContactsCreated += newOrganizations.Length;
                importProgress.ContactsUpdated += existedOrganizations.Length;
            }
            catch (Exception e)
            {
                HandleError(progressCallback, importProgress, e.Message);
            }
            finally
            {
                importProgress.ProcessedCount = Math.Min(dataSource.CurrentPageNumber * dataSource.PageSize, importProgress.TotalCount);
                importProgress.ErrorCount = importProgress.ProcessedCount - importProgress.ContactsCreated - importProgress.ContactsUpdated;
            }
        }


        private static void PatchExistedOrganizations(IEnumerable<Organization> existedOrganizations, ImportRecord<CsvOrganization>[] updateImportOrganizations)
        {
            foreach (var existedOrganization in existedOrganizations)
            {
                var importOrganization = updateImportOrganizations.LastOrDefault(x => existedOrganization.Id.EqualsInvariant(x.Record.Id)
                                                                            || (!existedOrganization.OuterId.IsNullOrEmpty() && existedOrganization.OuterId.EqualsInvariant(x.Record.OuterId)));

                importOrganization?.Record.PatchOrganization(existedOrganization);
            }
        }

        private static Organization[] CreateNewOrganizations(ImportRecord<CsvOrganization>[] createImportOrganizations)
        {
            var newOrganizations = createImportOrganizations.Select(x =>
            {
                var organization = AbstractTypeFactory<Organization>.TryCreateInstance();

                x.Record.PatchOrganization(organization);

                return organization;
            }).ToArray();

            return newOrganizations;
        }
    }
}
