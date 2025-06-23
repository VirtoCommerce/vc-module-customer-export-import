using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Nager.Country;
using VirtoCommerce.AssetsModule.Core.Assets;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerExportImportModule.Core.Services;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;
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
            var updatedContacts = existedContacts
                .Where(ec => importContacts.Any(ic => ic.Record.IdsEquals(ec)))
                .ToArray();

            var updateImportContacts = importContacts
                .Where(ic => updatedContacts.Any(ec => ic.Record.IdsEquals(ec)))
                .ToArray();

            updatedContacts = GetReducedExistedByWrongOuterId(updateImportContacts, updatedContacts).ToArray();

            var createImportContacts = importContacts.Except(updateImportContacts).ToArray();

            var internalOrgIds = importContacts.Select(x => x.Record?.OrganizationId).Distinct()
                .Where(x => !string.IsNullOrEmpty(x)).ToArray();

            var outerOrgIds = importContacts.Select(x => x.Record?.OrganizationOuterId).Distinct()
                .Where(x => !string.IsNullOrEmpty(x)).ToArray();

            var existedOrganizations =
                (await SearchMembersByIdAndOuterIdAsync(internalOrgIds, outerOrgIds, new[] { nameof(Organization) }, true))
                .OfType<Organization>().ToArray();

            var newContacts = CreateNewContacts(createImportContacts, existedOrganizations, request.OrganizationId);

            PatchExistedContacts(updatedContacts, updateImportContacts, existedOrganizations, request.OrganizationId);

            var saveContacts = newContacts.Union(updatedContacts).ToArray();

            await _memberService.SaveChangesAsync(saveContacts);

            await CreateAccountsForContacts(saveContacts);

            importProgress.CreatedCount += newContacts.Length;
            importProgress.UpdatedCount += updatedContacts.Length;
            importProgress.AdditionalLineCount += createImportContacts.Length - newContacts.Length + updateImportContacts.Length - updatedContacts.Length;
        }

        private async Task CreateAccountsForContacts(Contact[] saveContacts)
        {
            foreach (var (contact, account) in saveContacts.SelectMany(x =>
                         x.SecurityAccounts.Where(a => string.IsNullOrEmpty(a.MemberId)), (contact, account) => (contact, account)))
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

        private static void PatchExistedContacts(Contact[] existedContacts, ImportRecord<ImportableContact>[] updateImportContacts, Organization[] existedOrganizations, string requestOrganizationId)
        {
            foreach (var importContact in updateImportContacts)
            {
                var existedContact = existedContacts.FirstOrDefault(x => importContact.Record.IdsEquals(x));
                if (existedContact == null)
                {
                    continue;
                }

                importContact.Record.PatchModel(existedContact);

                var existedOrg = existedOrganizations.FirstOrDefault(x => CsvMember.IdsEquals(importContact.Record.OrganizationId, importContact.Record.OrganizationOuterId, x));
                var orgIdForNewContact = existedOrg?.Id ?? requestOrganizationId;

                if (!string.IsNullOrEmpty(orgIdForNewContact) && existedContact.Organizations?.Contains(orgIdForNewContact) != true)
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
                        importRecord.Record.PatchModel(contact);

                        if (importRecord.Record.AdditionalLine != true)
                        {
                            var existedOrg = existedOrganizations.FirstOrDefault(x => CsvMember.IdsEquals(importRecord.Record.OrganizationId, importRecord.Record.OrganizationOuterId, x));
                            var orgIdForNewContact = existedOrg?.Id ?? requestOrganizationId;

                            contact.Organizations = orgIdForNewContact != null ? new[] { orgIdForNewContact }.ToList() : new List<string>();
                        }
                    }

                    return contact;
                }).ToArray();
    }
}
