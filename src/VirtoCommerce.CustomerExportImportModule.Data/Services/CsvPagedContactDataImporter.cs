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
    public sealed class CsvPagedContactDataImporter : CsvPagedDataImporter<CsvContact, Contact>
    {
        private readonly IValidator<ImportRecord<CsvContact>[]> _importContactValidator;

        public CsvPagedContactDataImporter(IMemberService memberService, IMemberSearchService memberSearchService, ICsvCustomerDataValidator dataValidator, IValidator<ImportRecord<CsvContact>[]> importContactValidator
            , ICustomerImportPagedDataSourceFactory dataSourceFactory, ICsvCustomerImportReporterFactory importReporterFactory, IBlobUrlResolver blobUrlResolver)
        : base(memberService, memberSearchService, dataValidator, dataSourceFactory, importReporterFactory, blobUrlResolver)
        {
            _importContactValidator = importContactValidator;
        }


        protected override async Task HandleChunk(ImportDataRequest request, Action<ImportProgressInfo> progressCallback,
            ICustomerImportPagedDataSource<CsvContact> dataSource, ImportErrorsContext errorsContext, ImportProgressInfo importProgress,
            string importDescription)
        {
            var importContacts = dataSource.Items
                // expect records that was parsed with errors
                .Where(importContact => !errorsContext.ErrorsRows.Contains(importContact.Row))
                .ToArray();

            try
            {
                var internalIds = importContacts.Select(x => x.Record?.Id).Distinct()
                    .Where(x => !x.IsNullOrEmpty())
                    .ToArray();

                var outerIds = importContacts.Select(x => x.Record?.OuterId).Distinct()
                    .Where(x => !x.IsNullOrEmpty())
                    .ToArray();

                var existedContacts =
                    (await SearchMembersByIdAndOuterId(internalIds, outerIds, new[] { nameof(Contact) }, true))
                    .OfType<Contact>().ToArray();

                SetIdToNullForNotExisted(importContacts, existedContacts);

                var validationResult = await _importContactValidator.ValidateAsync(importContacts);

                var invalidImportContacts = validationResult.Errors
                    .Select(x => (x.CustomState as ImportValidationState<CsvContact>)?.InvalidRecord).Distinct().ToArray();

                importContacts = importContacts.Except(invalidImportContacts).ToArray();

                //reduce existed set after validation
                existedContacts = existedContacts.Where(ec => importContacts.Any(ic =>
                        ec.Id.EqualsInvariant(ic.Record.Id)
                        || !string.IsNullOrEmpty(ec.OuterId) && ec.OuterId.EqualsInvariant(ic.Record.OuterId)))
                    .ToArray();

                var updateImportContacts = importContacts.Where(x => existedContacts.Any(ec =>
                    ec.Id.EqualsInvariant(x.Record.Id)
                    || (!ec.OuterId.IsNullOrEmpty() && ec.OuterId.EqualsInvariant(x.Record.OuterId)))
                ).ToArray();

                var createImportContacts = importContacts.Except(updateImportContacts).ToArray();

                var internalOrgIds = importContacts.Select(x => x.Record?.OrganizationId).Distinct()
                    .Where(x => !x.IsNullOrEmpty()).ToArray();

                var outerOrgIds = importContacts.Select(x => x.Record?.OrganizationOuterId).Distinct()
                    .Where(x => !x.IsNullOrEmpty()).ToArray();

                var existedOrganizations =
                    (await SearchMembersByIdAndOuterId(internalOrgIds, outerOrgIds,
                        new[] { nameof(Organization) })).OfType<Organization>().ToArray();

                var newContacts = CreateNewContacts(createImportContacts, existedOrganizations, request);

                PatchExistedContacts(existedContacts, updateImportContacts, existedOrganizations, request);

                var contactsForSave = newContacts.Union(existedContacts).ToArray();

                await _memberService.SaveChangesAsync(contactsForSave);

                importProgress.ContactsCreated += newContacts.Length;
                importProgress.ContactsUpdated += existedContacts.Length;
            }
            catch (Exception e)
            {
                HandleError(progressCallback, importProgress, e.Message);
            }
            finally
            {
                importProgress.ProcessedCount =
                    Math.Min(dataSource.CurrentPageNumber * dataSource.PageSize, importProgress.TotalCount);
                importProgress.ErrorCount = importProgress.ProcessedCount - importProgress.ContactsCreated -
                                            importProgress.ContactsUpdated;
            }

            if (importProgress.ProcessedCount != importProgress.TotalCount)
            {
                importProgress.Description =
                    string.Format(importDescription, importProgress.ProcessedCount, importProgress.TotalCount);
                progressCallback(importProgress);
            }
        }


        private static void PatchExistedContacts(IEnumerable<Contact> existedContacts, ImportRecord<CsvContact>[] updateImportContacts, Organization[] existedOrganizations, ImportDataRequest request)
        {
            foreach (var existedContact in existedContacts)
            {
                var importContact = updateImportContacts.LastOrDefault(x => existedContact.Id.EqualsInvariant(x.Record.Id)
                                                                            || (!existedContact.OuterId.IsNullOrEmpty() && existedContact.OuterId.EqualsInvariant(x.Record.OuterId)));

                var existedOrg = existedOrganizations.FirstOrDefault(o => o.Id.EqualsInvariant(importContact.Record.OrganizationId))
                                 ?? existedOrganizations.FirstOrDefault(o =>
                                     !o.OuterId.IsNullOrEmpty() && o.OuterId.EqualsInvariant(importContact.Record.OrganizationOuterId));

                var orgIdForNewContact = existedOrg?.Id ?? request.OrganizationId;

                importContact?.Record.PatchContact(existedContact);

                if (!orgIdForNewContact.IsNullOrEmpty() && !existedContact.Organizations.Contains(orgIdForNewContact))
                {
                    existedContact.Organizations ??= new List<string>();
                    existedContact.Organizations.Add(orgIdForNewContact);
                }
            }
        }

        private static Contact[] CreateNewContacts(ImportRecord<CsvContact>[] createImportContacts, Organization[] existedOrganizations, ImportDataRequest request)
        {
            var newContacts = createImportContacts.Select(x =>
            {
                var contact = AbstractTypeFactory<Contact>.TryCreateInstance<Contact>();

                x.Record.PatchContact(contact);

                var existedOrg = existedOrganizations.FirstOrDefault(o => o.Id == x.Record.OrganizationId)
                                 ?? existedOrganizations.FirstOrDefault(o =>
                                     o.OuterId == x.Record.OrganizationOuterId);

                var orgIdForNewContact = existedOrg?.Id ?? request.OrganizationId;

                contact.Organizations =
                    orgIdForNewContact != null ? new[] { orgIdForNewContact }.ToList() : new List<string>();

                return contact;
            }).ToArray();

            return newContacts;
        }
    }
}
