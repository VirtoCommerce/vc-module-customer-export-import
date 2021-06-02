using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using VirtoCommerce.CustomerExportImportModule.Core;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerExportImportModule.Core.Services;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CustomerExportImportModule.Data.Services
{
    public sealed class CsvPagedCustomerDataImporter
    {
        private readonly IMemberService _memberService;
        private readonly IMemberSearchService _memberSearchService;
        private readonly ICsvCustomerDataValidator _dataValidator;
        private readonly ICsvCustomerImportReporterFactory _importReporterFactory;
        private readonly ICustomerImportPagedDataSourceFactory _dataSourceFactory;
        private readonly IBlobUrlResolver _blobUrlResolver;

        public CsvPagedCustomerDataImporter(IMemberService memberService, IMemberSearchService memberSearchService, ICsvCustomerDataValidator dataValidator
            , ICustomerImportPagedDataSourceFactory dataSourceFactory, ICsvCustomerImportReporterFactory importReporterFactory, IBlobUrlResolver blobUrlResolver)
        {
            _memberService = memberService;
            _memberSearchService = memberSearchService;
            _dataValidator = dataValidator;
            _importReporterFactory = importReporterFactory;
            _dataSourceFactory = dataSourceFactory;
            _blobUrlResolver = blobUrlResolver;
        }

        public async Task ImportAsync(ImportDataRequest request, Action<ImportProgressInfo> progressCallback,
            ICancellationToken cancellationToken)
        {
            ValidateParameters(request, progressCallback, cancellationToken);

            var errorsContext = new ImportErrorsContext();

            var csvPriceDataValidationResult = await _dataValidator.ValidateAsync(request.FilePath);

            if (csvPriceDataValidationResult.Errors.Any())
            {
                throw new InvalidDataException();
            }

            var reportFilePath = GetReportFilePath(request.FilePath);

            var configuration = new ImportConfiguration();

            await using var importReporter = await _importReporterFactory.CreateAsync(reportFilePath, configuration.Delimiter);

            cancellationToken.ThrowIfCancellationRequested();

            var importProgress = new ImportProgressInfo { Description = "Import has started" };

            using var dataSource = await _dataSourceFactory.CreateAsync(request.FilePath, ModuleConstants.Settings.PageSize, configuration);

            var headerRaw = dataSource.GetHeaderRaw();

            if (!headerRaw.IsNullOrEmpty())
            {
                importReporter.WriteHeader(headerRaw);
            }

            importProgress.TotalCount = dataSource.GetTotalCount();
            progressCallback(importProgress);

            const string importDescription = "{0} out of {1} have been imported.";

            configuration.ReadingExceptionOccurred = exception =>
            {
                var context = exception.ReadingContext;

                if (!errorsContext.ErrorsRows.Contains(context.Row))
                {
                    var fieldSourceValue = context.Record[context.CurrentIndex];

                    if (context.HeaderRecord.Length != context.Record.Length)
                    {
                        HandleNotClosedQuoteError(progressCallback, importProgress, importReporter, context, errorsContext);
                    }
                    else if (fieldSourceValue == "")
                    {
                        HandleRequiredValueError(progressCallback, importProgress, importReporter, context, errorsContext);
                    }
                    else
                    {
                        HandleWrongValueError(progressCallback, importProgress, importReporter, context, errorsContext);
                    }
                }

                return false;
            };

            configuration.BadDataFound = async context =>
            {
                await HandleBadDataError(progressCallback, importProgress, importReporter, context, errorsContext);
            };

            configuration.MissingFieldFound = async (headerNames, index, context) =>
                await HandleMissedColumnError(progressCallback, importProgress, importReporter, context, errorsContext);

            try
            {
                importProgress.Description = "Fetching...";
                progressCallback(importProgress);


                while (await dataSource.FetchAsync())
                {
                    var importContacts = dataSource.Items
                        // expect records that was parsed with errors
                        .Where(importContact => !errorsContext.ErrorsRows.Contains(importContact.Row))
                        .ToArray();

                    try
                    {
                        // todo: зарезать дубликаты. Вопрос: что делать если в одних дупликатах есть связанные сущности а в других нет...

                        var internalIds = importContacts.Select(x => x.Record.Id).Distinct().Where(x => x != null)
                            .ToArray();
                        var outerIds = importContacts.Select(x => x.Record.OuterId).Distinct().Where(x => x != null)
                            .ToArray();

                        var existedContacts =
                            (await SearchMembersByIdAndOuterId(internalIds, outerIds, new[] { nameof(Contact) }))
                            .OfType<Contact>();

                        var existedContactsIds = existedContacts.Select(x => x.Id);

                        var createImportContacts = importContacts.Where(x => existedContactsIds.Contains(x.Record.Id))
                            .ToArray();

                        var internalOrgIds = importContacts.Select(x => x.Record.OrganizationId).Distinct()
                            .Where(x => x != null).ToArray();
                        var outerOrgIds = importContacts.Select(x => x.Record.OrganizationOuterId).Distinct()
                            .Where(x => x != null).ToArray();

                        var existedOrganizations =
                            (await SearchMembersByIdAndOuterId(internalOrgIds, outerOrgIds,
                                new[] { nameof(Organization) })).OfType<Organization>();

                        var newContacts = createImportContacts.Select(x =>
                        {
                            var contact = x.Record.ToContact(true);

                            var existedOrg = existedOrganizations.FirstOrDefault(o => o.Id == x.Record.OrganizationId)
                                             ?? existedOrganizations.FirstOrDefault(o =>
                                                 o.OuterId == x.Record.OrganizationOuterId);

                            var orgIdForNewContact = existedOrg?.Id ?? request.OrganizationId;

                            contact.Organizations =
                                orgIdForNewContact != null ? new[] { orgIdForNewContact }.ToList() : new List<string>();

                            return contact;
                        }).ToArray();


                        await _memberService.SaveChangesAsync(newContacts);
                    }
                    catch (Exception e)
                    {
                        HandleError(progressCallback, importProgress, e.Message);
                    }
                    finally
                    {
                        importProgress.ProcessedCount = Math.Min(dataSource.CurrentPageNumber * dataSource.PageSize, importProgress.TotalCount);
                    }

                    if (importProgress.ProcessedCount != importProgress.TotalCount)
                    {
                        importProgress.Description = string.Format(importDescription, importProgress.ProcessedCount, importProgress.TotalCount);
                        progressCallback(importProgress);
                    }
                }
            }
            catch (Exception e)
            {
                HandleError(progressCallback, importProgress, e.Message);
            }
            finally
            {
                var completedMessage = importProgress.ErrorCount > 0 ? "Import completed with errors" : "Import completed";
                importProgress.Description = $"{completedMessage}: {string.Format(importDescription, importProgress.ProcessedCount, importProgress.TotalCount)}";

                if (importReporter.ReportIsNotEmpty)
                {
                    importProgress.ReportUrl = _blobUrlResolver.GetAbsoluteUrl(reportFilePath);
                }

                progressCallback(importProgress);

            }
        }

        private async Task<Member[]> SearchMembersByIdAndOuterId(string[] internalIds, string[] outerIds, string[] memberTypes)
        {
            var searchResultById = await _memberSearchService.SearchMembersAsync(new ExtendedMemberSearchCiteria()
            {
                ObjectIds = internalIds,
                MemberTypes = memberTypes,
                Take = ModuleConstants.Settings.PageSize
            });

            var searchResultByOuterId = await _memberSearchService.SearchMembersAsync(new ExtendedMemberSearchCiteria()
            {
                OuterIds = outerIds,
                MemberTypes = memberTypes,
                Take = ModuleConstants.Settings.PageSize
            });

            var existedMembers = searchResultById.Results.Union(searchResultByOuterId.Results,
                EqualityComparer<Member>.Default).ToArray();
            return existedMembers;
        }

        private static void HandleError(Action<ImportProgressInfo> progressCallback, ImportProgressInfo importProgress, string error = null)
        {
            if (error != null)
            {
                importProgress.Errors.Add(error);
            }

            importProgress.ErrorCount++;
            progressCallback(importProgress);
        }

        private static async Task HandleBadDataError(Action<ImportProgressInfo> progressCallback, ImportProgressInfo importProgress, ICsvCustomerImportReporter reporter, ReadingContext context, ImportErrorsContext errorsContext)
        {
            var importError = new ImportError { Error = "This row has invalid data. The data after field with not escaped quote was lost.", RawRow = context.RawRecord };

            await reporter.WriteAsync(importError);

            errorsContext.ErrorsRows.Add(context.Row);
            HandleError(progressCallback, importProgress);
        }

        private static void HandleNotClosedQuoteError(Action<ImportProgressInfo> progressCallback, ImportProgressInfo importProgress, ICsvCustomerImportReporter reporter, ReadingContext context, ImportErrorsContext errorsContext)
        {
            var importError = new ImportError { Error = "This row has invalid data. Quotes should be closed.", RawRow = context.RawRecord };

            reporter.Write(importError);

            errorsContext.ErrorsRows.Add(context.Row);
            HandleError(progressCallback, importProgress);
        }

        private static void HandleWrongValueError(Action<ImportProgressInfo> progressCallback, ImportProgressInfo importProgress, ICsvCustomerImportReporter reporter, ReadingContext context, ImportErrorsContext errorsContext)
        {
            var invalidFieldName = context.HeaderRecord[context.CurrentIndex];
            var importError = new ImportError { Error = $"This row has invalid value in the column {invalidFieldName}.", RawRow = context.RawRecord };

            reporter.Write(importError);

            errorsContext.ErrorsRows.Add(context.Row);
            HandleError(progressCallback, importProgress);
        }

        private static void HandleRequiredValueError(Action<ImportProgressInfo> progressCallback, ImportProgressInfo importProgress, ICsvCustomerImportReporter reporter, ReadingContext context, ImportErrorsContext errorsContext)
        {
            var fieldName = context.HeaderRecord[context.CurrentIndex];
            var requiredFields = CsvCustomerImportHelper.GetImportCustomerRequiredColumns();
            var missedValueColumns = new List<string>();

            for (var i = 0; i < context.HeaderRecord.Length; i++)
            {
                if (requiredFields.Contains(context.HeaderRecord[i], StringComparer.InvariantCultureIgnoreCase) && context.Record[i].IsNullOrEmpty())
                {
                    missedValueColumns.Add(context.HeaderRecord[i]);
                }
            }

            var importError = new ImportError { Error = $"The required value in column {fieldName} is missing.", RawRow = context.RawRecord };

            if (missedValueColumns.Count > 1)
            {
                importError.Error = $"The required values in columns: {string.Join(", ", missedValueColumns)} - are missing.";
            }

            reporter.Write(importError);

            errorsContext.ErrorsRows.Add(context.Row);
            HandleError(progressCallback, importProgress);
        }

        private static async Task HandleMissedColumnError(Action<ImportProgressInfo> progressCallback, ImportProgressInfo importProgress, ICsvCustomerImportReporter reporter, ReadingContext context, ImportErrorsContext errorsContext)
        {
            var headerColumns = context.HeaderRecord;
            var recordFields = context.Record;
            var missedColumns = headerColumns.Skip(recordFields.Length).ToArray();
            var error = $"This row has next missing columns: {string.Join(", ", missedColumns)}.";
            var importError = new ImportError { Error = error, RawRow = context.RawRecord };

            await reporter.WriteAsync(importError);

            errorsContext.ErrorsRows.Add(context.Row);
            HandleError(progressCallback, importProgress);
        }

        private static void ValidateParameters(ImportDataRequest request, Action<ImportProgressInfo> progressCallback, ICancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (progressCallback == null)
            {
                throw new ArgumentNullException(nameof(progressCallback));
            }

            if (cancellationToken == null)
            {
                throw new ArgumentNullException(nameof(cancellationToken));
            }
        }

        private static string GetReportFilePath(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            var fileExtension = Path.GetExtension(fileName);
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            var reportFileName = $"{fileNameWithoutExtension}_report{fileExtension}";
            var result = filePath.Replace(fileName, reportFileName);

            return result;
        }

    }
}
