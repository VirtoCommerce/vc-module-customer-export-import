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
        private const int MaxHierarchyLevel = 10;

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

            var internalIds = importOrganizations.Select(x => x.Record?.Id)
                .Concat(importOrganizations.Select(x => x.Record?.ParentOrganizationId))
                .Where(x => !string.IsNullOrEmpty(x))
                .Distinct()
                .ToArray();

            var outerIds = importOrganizations.Select(x => x.Record?.OuterId)
                .Concat(importOrganizations.Select(x => x.Record?.ParentOrganizationOuterId))
                .Where(x => !string.IsNullOrEmpty(x))
                .Distinct()
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
            var updatedOrganizations = existedOrganizations
                .Where(ec => importOrganizations.Any(ic =>
                    ec.Id.EqualsInvariant(ic.Record.Id) || (!string.IsNullOrEmpty(ec.OuterId) && ec.OuterId.EqualsInvariant(ic.Record.OuterId))))
                .ToArray();

            var updateImportOrganizations = importOrganizations
                .Where(ic => updatedOrganizations.Any(ec =>
                    ec.Id.EqualsInvariant(ic.Record.Id) || (!string.IsNullOrEmpty(ec.OuterId) && ec.OuterId.EqualsInvariant(ic.Record.OuterId))))
                .ToArray();

            updatedOrganizations = GetReducedExistedByWrongOuterId(updateImportOrganizations, updatedOrganizations).ToArray();

            var createImportOrganizations = importOrganizations.Except(updateImportOrganizations).ToArray();

            var newOrganizations = CreateNewOrganizations(createImportOrganizations);

            PatchExistedOrganizations(updatedOrganizations, updateImportOrganizations);

            var saveOrganizations = newOrganizations.Union(updatedOrganizations).ToArray();

            await SaveChangesWithHierarchy(saveOrganizations, existedOrganizations, importOrganizations);

            importProgress.CreatedCount += newOrganizations.Length;
            importProgress.AdditionalLineCount += createImportOrganizations.Length - newOrganizations.Length;
            importProgress.UpdatedCount += updatedOrganizations.Length;
        }

        protected override void SetIdToNullForNotExisted(ImportRecord<ImportableOrganization>[] importMembers, Organization[] existedMembers)
        {
            base.SetIdToNullForNotExisted(importMembers, existedMembers);

            foreach (var importMember in importMembers.Where(x => !string.IsNullOrEmpty(x.Record.ParentOrganizationId)))
            {
                var existedMember = existedMembers.FirstOrDefault(x => x.Id.EqualsInvariant(importMember.Record.ParentOrganizationId));
                if (existedMember == null)
                {
                    importMember.Record.ParentOrganizationId = null;
                }
            }
        }

        protected override void SetIdToRealForExistedOuterId(ImportRecord<ImportableOrganization>[] importMembers, Organization[] existedMembers)
        {
            base.SetIdToRealForExistedOuterId(importMembers, existedMembers);

            foreach (var importMember in importMembers.Where(x => string.IsNullOrEmpty(x.Record.ParentOrganizationId) && !string.IsNullOrEmpty(x.Record.ParentOrganizationOuterId)))
            {
                var existedMember = existedMembers.FirstOrDefault(x => !string.IsNullOrEmpty(x.OuterId) && x.OuterId.EqualsInvariant(importMember.Record.ParentOrganizationOuterId));
                if (existedMember != null)
                {
                    importMember.Record.ParentOrganizationId = existedMember.Id;
                }
            }
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

        private async Task SaveChangesWithHierarchy(Organization[] saveOrganizations, Organization[] existedOrganizations, ImportRecord<ImportableOrganization>[] importOrganizations)
        {
            var existedList = existedOrganizations.ToList();
            var saveTuples = saveOrganizations.Select(x => (Organization: x, Saved: false)).ToArray();
            foreach (var item in saveTuples)
            {
                await SetParent(item.Organization, saveTuples, existedList, importOrganizations);
            }

            saveOrganizations = saveTuples.Where(x => !x.Saved).Select(x => x.Organization).ToArray();
            await _memberService.SaveChangesAsync(saveOrganizations);
        }

        private async Task SetParent(Organization organization, (Organization Organization, bool Saved)[] saveOrganizations, ICollection<Organization> existedOrganizations, ImportRecord<ImportableOrganization>[] importOrganizations, int level = default)
        {
            bool IdsPredicate(Organization first, ImportableOrganization second) =>
                (!string.IsNullOrEmpty(first.Id) && first.Id.EqualsInvariant(second.Id))
                || (!string.IsNullOrEmpty(first.OuterId) && first.OuterId.EqualsInvariant(second.OuterId));

            var importRecord = importOrganizations
                .Where(x => x.Record.AdditionalLine != true)
                .FirstOrDefault(x => IdsPredicate(organization, x.Record));
            if (!string.IsNullOrEmpty(importRecord?.Record.ParentOrganizationId) || !string.IsNullOrEmpty(importRecord?.Record.ParentOrganizationOuterId))
            {
                var parentOrganization = existedOrganizations.FirstOrDefault(x => IdsPredicate(x, importRecord.Record));
                if (parentOrganization != null)
                {
                    organization.ParentId = parentOrganization.Id;
                }
                else
                {
                    var saveTuple = saveOrganizations.FirstOrDefault(x => IdsPredicate(x.Organization, importRecord.Record));
                    if (saveTuple.Organization != null)
                    {
                        parentOrganization = saveTuple.Organization;
                        if (level < MaxHierarchyLevel)
                        {
                            await SetParent(parentOrganization, saveOrganizations, existedOrganizations, importOrganizations, ++level);
                        }
                        await _memberService.SaveChangesAsync(new[] { parentOrganization });
                        existedOrganizations.Add(parentOrganization);
                        saveTuple.Saved = true;

                        organization.ParentId = parentOrganization.Id;
                    }
                }
            }
        }
    }
}
