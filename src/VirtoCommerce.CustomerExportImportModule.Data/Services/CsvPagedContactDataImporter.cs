using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Nager.Country;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerExportImportModule.Core.Services;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.AssetsModule.Core.Assets;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.CustomerExportImportModule.Data.Services
{
    public sealed class CsvPagedContactDataImporter : CsvPagedDataImporter<ImportableContact, Contact>
    {
        private readonly IMemberService _memberService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPasswordGenerator _passwordGenerator;

        public override string MemberType => nameof(Contact);

        public CsvPagedContactDataImporter(IMemberService memberService, IMemberSearchService memberSearchService, ICsvCustomerDataValidator dataValidator, IValidator<ImportRecord<ImportableContact>[]> importContactValidator
            , ICustomerImportPagedDataSourceFactory dataSourceFactory, ICsvCustomerImportReporterFactory importReporterFactory, IBlobUrlResolver blobUrlResolver, UserManager<ApplicationUser> userManager, IPasswordGenerator passwordGenerator,
            ICountryProvider countryProvider, ICountriesService countriesService)
        : base(memberSearchService, dataValidator, dataSourceFactory, importContactValidator, importReporterFactory, blobUrlResolver, countryProvider, countriesService)
        {
            _memberService = memberService;
            _userManager = userManager;
            _passwordGenerator = passwordGenerator;
        }

        protected override async Task ProcessChunkAsync(ImportDataRequest request, Action<ImportProgressInfo> progressCallback, ImportRecord<ImportableContact>[] importRecords,
            ImportErrorsContext errorsContext, ImportProgressInfo importProgress, ICsvCustomerImportReporter importReporter)
        {
            var importContacts = importRecords;

            var internalIds = importContacts.Select(x => x.Record?.Id).Distinct()
                .Where(x => !string.IsNullOrEmpty(x))
                .ToArray();

            var outerIds = importContacts.Select(x => x.Record?.OuterId).Distinct()
                .Where(x => !string.IsNullOrEmpty(x))
                .ToArray();

            var existedContacts =
                (await SearchMembersByIdAndOuterIdAsync(internalIds, outerIds, new[] { nameof(Contact) }, true))
                .OfType<Contact>().ToArray();

            SetIdToNullForNotExisted(importContacts, existedContacts);

            SetIdToRealForExistedOuterId(importContacts, existedContacts);

            var validationResult = await ValidateAsync(importContacts, importReporter);

            var invalidImportContacts = validationResult.Errors
                .Select(x => (x.CustomState as ImportValidationState<ImportableContact>)?.InvalidRecord).Distinct().ToArray();

            importContacts = importContacts.Except(invalidImportContacts).ToArray();

            //reduce existed set after validation
            existedContacts = existedContacts
                .Where(ec => importContacts.Any(ic =>
                    ec.Id.EqualsInvariant(ic.Record.Id) || !string.IsNullOrEmpty(ec.OuterId) && ec.OuterId.EqualsInvariant(ic.Record.OuterId)))
                .ToArray();

            var updateImportContacts = importContacts
                .Where(ic => existedContacts.Any(ec =>
                    ec.Id.EqualsInvariant(ic.Record.Id) || (!string.IsNullOrEmpty(ec.OuterId) && ec.OuterId.EqualsInvariant(ic.Record.OuterId))))
                .ToArray();

            existedContacts = GetReducedExistedByWrongOuterId(updateImportContacts, existedContacts).ToArray();

            var createImportContacts = importContacts.Except(updateImportContacts).ToArray();

            var internalOrgIds = importContacts.Select(x => x.Record?.OrganizationId).Distinct()
                .Where(x => !string.IsNullOrEmpty(x)).ToArray();

            var outerOrgIds = importContacts.Select(x => x.Record?.OrganizationOuterId).Distinct()
                .Where(x => !string.IsNullOrEmpty(x)).ToArray();

            var existedOrganizations =
                (await SearchMembersByIdAndOuterIdAsync(internalOrgIds, outerOrgIds, new[] { nameof(Organization) }))
                .OfType<Organization>().ToArray();

            var newContacts = CreateNewContacts(createImportContacts, existedOrganizations, request.OrganizationId);

            PatchExistedContacts(existedContacts, updateImportContacts, existedOrganizations, request.OrganizationId);

            var saveContacts = newContacts.Union(existedContacts).ToArray();

            await _memberService.SaveChangesAsync(saveContacts);

            await CreateAccountsForContacts(saveContacts);

            importProgress.CreatedCount += newContacts.Length;
            importProgress.AdditionalLineCount += createImportContacts.Length - newContacts.Length;
            importProgress.UpdatedCount += existedContacts.Length;
        }

        private async Task CreateAccountsForContacts(Contact[] contactsForSave)
        {
            foreach (var contact in contactsForSave)
                foreach (var account in contact.SecurityAccounts)
                {
                    account.MemberId = contact.Id;

                    if (string.IsNullOrEmpty(account.Password))
                    {
                        var generatedPassword = _passwordGenerator.GeneratePassword();
                        await _userManager.CreateAsync(account, generatedPassword);
                    }
                    else
                    {
                        await _userManager.CreateAsync(account, account.Password);
                    }
                }
        }

        private static void PatchExistedContacts(IEnumerable<Contact> existedContacts, ImportRecord<ImportableContact>[] updateImportContacts, Organization[] existedOrganizations, string requestOrganizationId)

        {
            foreach (var existedContact in existedContacts)
            {
                var importContact = updateImportContacts.LastOrDefault(x => existedContact.Id.EqualsInvariant(x.Record.Id)
                                                                            || (!existedContact.OuterId.IsNullOrEmpty() && existedContact.OuterId.EqualsInvariant(x.Record.OuterId)));

                var existedOrg = importContact == null
                    ? null
                    : existedOrganizations.FirstOrDefault(o => o.Id.EqualsInvariant(importContact.Record.OrganizationId))
                      ?? existedOrganizations.FirstOrDefault(o => !o.OuterId.IsNullOrEmpty() && o.OuterId.EqualsInvariant(importContact.Record.OrganizationOuterId));

                var orgIdForNewContact = existedOrg?.Id ?? requestOrganizationId;

                importContact?.Record.PatchModel(existedContact);

                if (!string.IsNullOrEmpty(orgIdForNewContact) && !existedContact.Organizations.Contains(orgIdForNewContact))
                {
                    existedContact.Organizations ??= new List<string>();
                    existedContact.Organizations.Add(orgIdForNewContact);
                }
            }
        }

        private static Contact[] CreateNewContacts(ImportRecord<ImportableContact>[] createImportContacts, Organization[] existedOrganizations, string requestOrganizationId) =>
            createImportContacts
                .GroupBy(x => (x.Record.Id, x.Record.OuterId, x.Record.RecordName))
                .Where(group => group.Any(x => x.Record.AdditionalLine != true))
                .Select(group =>
                {
                    var contact = AbstractTypeFactory<Contact>.TryCreateInstance<Contact>();

                    foreach (var importRecord in group.OrderBy(x => x.Record.AdditionalLine == true).ThenBy(x => x.Row))
                    {
                        if (importRecord.Record.AdditionalLine != true)
                        {
                            var existedOrg = existedOrganizations.FirstOrDefault(o => o.Id.EqualsInvariant(importRecord.Record.OrganizationId))
                                             ?? existedOrganizations.FirstOrDefault(o => !string.IsNullOrEmpty(o.OuterId) && o.OuterId.EqualsInvariant(importRecord.Record.OrganizationOuterId));

                            var orgIdForNewContact = existedOrg?.Id ?? requestOrganizationId;

                            contact.Organizations = orgIdForNewContact != null ? new[] { orgIdForNewContact }.ToList() : new List<string>();
                        }

                        importRecord.Record.PatchModel(contact);
                    }

                    return contact;
                }).ToArray();
    }
}
