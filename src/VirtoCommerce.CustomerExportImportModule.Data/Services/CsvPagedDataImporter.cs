using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using FluentValidation;
using FluentValidation.Results;
using VirtoCommerce.CustomerExportImportModule.Core;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerExportImportModule.Core.Services;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CustomerExportImportModule.Data.Services
{
    public abstract class CsvPagedDataImporter<TCsvMember, TMember> : ICsvPagedCustomerDataImporter
        where TCsvMember : CsvMember
        where TMember : Member
    {
        private readonly IMemberSearchService _memberSearchService;
        private readonly ICsvCustomerDataValidator _dataValidator;
        private readonly ICsvCustomerImportReporterFactory _importReporterFactory;
        private readonly ICustomerImportPagedDataSourceFactory _dataSourceFactory;
        private readonly IValidator<ImportRecord<TCsvMember>[]> _importRecordsValidator;
        private readonly IBlobUrlResolver _blobUrlResolver;

        public abstract string MemberType { get; }

        protected CsvPagedDataImporter(IMemberSearchService memberSearchService, ICsvCustomerDataValidator dataValidator
            , ICustomerImportPagedDataSourceFactory dataSourceFactory, IValidator<ImportRecord<TCsvMember>[]> importRecordsValidator,
            ICsvCustomerImportReporterFactory importReporterFactory, IBlobUrlResolver blobUrlResolver)
        {
            _memberSearchService = memberSearchService;
            _dataValidator = dataValidator;
            _importReporterFactory = importReporterFactory;
            _dataSourceFactory = dataSourceFactory;
            _importRecordsValidator = importRecordsValidator;
            _blobUrlResolver = blobUrlResolver;
        }

        public virtual async Task ImportAsync(ImportDataRequest request, Action<ImportProgressInfo> progressCallback, ICancellationToken cancellationToken)
        {
            ValidateParameters(request, progressCallback, cancellationToken);

            var errorsContext = new ImportErrorsContext();

            var csvPriceDataValidationResult = await _dataValidator.ValidateAsync<TCsvMember>(request.FilePath);
            if (csvPriceDataValidationResult.Errors.Any())
            {
                throw new InvalidDataException();
            }

            var configuration = new ImportConfiguration();

            var reportFilePath = GetReportFilePath(request.FilePath);
            await using var importReporter = await _importReporterFactory.CreateAsync(reportFilePath, configuration.Delimiter);

            cancellationToken.ThrowIfCancellationRequested();

            var importProgress = new ImportProgressInfo { Description = "Import has started" };

            using var dataSource = await _dataSourceFactory.CreateAsync<TCsvMember, TMember>(request.FilePath, ModuleConstants.Settings.PageSize, configuration);

            var headerRaw = dataSource.GetHeaderRaw();
            if (!headerRaw.IsNullOrEmpty())
            {
                await importReporter.WriteHeaderAsync(headerRaw);
            }

            importProgress.TotalCount = dataSource.GetTotalCount();
            progressCallback(importProgress);

            const string importDescription = "{0} out of {1} have been imported.";

            SetupErrorHandlers(progressCallback, configuration, errorsContext, importProgress, importReporter);

            try
            {
                importProgress.Description = "Fetching...";
                progressCallback(importProgress);

                while (await dataSource.FetchAsync())
                {
                    await ProcessChunkAsync(request, progressCallback, dataSource, errorsContext, importProgress, importReporter);

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


        protected abstract Task ProcessChunkAsync(ImportDataRequest request, Action<ImportProgressInfo> progressCallback, ICustomerImportPagedDataSource<TCsvMember> dataSource,
            ImportErrorsContext errorsContext, ImportProgressInfo importProgress, ICsvCustomerImportReporter importReporter);

        protected async Task<ValidationResult> ValidateAsync(ImportRecord<TCsvMember>[] importRecords, ICsvCustomerImportReporter importReporter)
        {
            var validationResult = await _importRecordsValidator.ValidateAsync(importRecords);

            var errorsInfos = validationResult.Errors.Select(x => new { Message = x.ErrorMessage, (x.CustomState as ImportValidationState<TCsvMember>)?.InvalidRecord }).ToArray();

            var errorsGroups = errorsInfos.GroupBy(x => x.InvalidRecord);

            foreach (var group in errorsGroups)
            {
                var importPrice = group.Key;

                var errorMessages = string.Join(" ", group.Select(x => x.Message).ToArray());

                var importError = new ImportError { Error = errorMessages, RawRow = importPrice.RawRecord };

                await importReporter.WriteAsync(importError);
            }

            return validationResult;
        }

        protected static void HandleError(Action<ImportProgressInfo> progressCallback, ImportProgressInfo importProgress, string error = null)
        {
            if (error != null)
            {
                importProgress.Errors.Add(error);
            }

            progressCallback(importProgress);
        }

        protected async Task<Member[]> SearchMembersByIdAndOuterIdAsync(string[] internalIds, string[] outerIds, string[] memberTypes, bool deepSearch = false)
        {
            var criteriaById = new ExtendedMembersSearchCriteria()
            {
                ObjectIds = internalIds,
                MemberTypes = memberTypes,
                DeepSearch = deepSearch,
                Skip = 0,
                Take = ModuleConstants.Settings.PageSize
            };

            var membersById = internalIds.IsNullOrEmpty() ? Array.Empty<Member>() : (await _memberSearchService.SearchMembersAsync(criteriaById)).Results;

            var criteriaByOuterId = new ExtendedMembersSearchCriteria()
            {
                OuterIds = outerIds,
                MemberTypes = memberTypes,
                DeepSearch = deepSearch,
                Skip = 0,
                Take = ModuleConstants.Settings.PageSize
            };

            var membersByOuterId = outerIds.IsNullOrEmpty() ? Array.Empty<Member>() : (await _memberSearchService.SearchMembersAsync(criteriaByOuterId)).Results;

            var existedMembers = membersById.Union(membersByOuterId
                , AnonymousComparer.Create<Member>((x, y) => x.Id == y.Id, x => x.Id.GetHashCode())).ToArray();

            return existedMembers;
        }

        protected static void SetIdToNullForNotExisted(ImportRecord<TCsvMember>[] importContacts, TMember[] existedContacts)
        {
            foreach (var importContact in importContacts)
            {
                var existedContact =
                    existedContacts.FirstOrDefault(x => x.Id.EqualsInvariant(importContact.Record.Id));

                if (existedContact == null)
                {
                    importContact.Record.Id = null;
                }
            }
        }

        private static async Task HandleBadDataErrorAsync(Action<ImportProgressInfo> progressCallback, ImportProgressInfo importProgress, ICsvCustomerImportReporter reporter, ReadingContext context, ImportErrorsContext errorsContext)
        {
            var importError = new ImportError { Error = "This row has invalid data. The data after field with not escaped quote was lost.", RawRow = context.RawRecord };

            await reporter.WriteAsync(importError);

            errorsContext.ErrorsRows.Add(context.Row);
            HandleError(progressCallback, importProgress);
        }

        private static void HandleNotClosedQuoteError(Action<ImportProgressInfo> progressCallback, ImportProgressInfo importProgress, ICsvCustomerImportReporter reporter, ReadingContext context, ImportErrorsContext errorsContext)
        {
            var importError = new ImportError { Error = "This row has invalid data. Quotes should be closed.", RawRow = context.RawRecord };

            reporter.WriteAsync(importError).GetAwaiter().GetResult();

            errorsContext.ErrorsRows.Add(context.Row);
            HandleError(progressCallback, importProgress);
        }

        private static void HandleWrongValueError(Action<ImportProgressInfo> progressCallback, ImportProgressInfo importProgress, ICsvCustomerImportReporter reporter, ReadingContext context, ImportErrorsContext errorsContext)
        {
            var invalidFieldName = context.HeaderRecord[context.CurrentIndex];
            var importError = new ImportError { Error = $"This row has invalid value in the column {invalidFieldName}.", RawRow = context.RawRecord };

            reporter.WriteAsync(importError).GetAwaiter().GetResult();

            errorsContext.ErrorsRows.Add(context.Row);
            HandleError(progressCallback, importProgress);
        }

        private static void HandleRequiredValueError(Action<ImportProgressInfo> progressCallback, ImportProgressInfo importProgress, ICsvCustomerImportReporter reporter, ReadingContext context, ImportErrorsContext errorsContext)
        {
            var fieldName = context.HeaderRecord[context.CurrentIndex];
            var requiredFields = CsvCustomerImportHelper.GetImportCustomerRequiredColumns<CsvContact>();
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

            reporter.WriteAsync(importError).GetAwaiter().GetResult();

            errorsContext.ErrorsRows.Add(context.Row);
            HandleError(progressCallback, importProgress);
        }

        private static void SetupErrorHandlers(Action<ImportProgressInfo> progressCallback, ImportConfiguration configuration,
            ImportErrorsContext errorsContext, ImportProgressInfo importProgress, ICsvCustomerImportReporter importReporter)
        {
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
                await HandleBadDataErrorAsync(progressCallback, importProgress, importReporter, context, errorsContext);
            };

            configuration.MissingFieldFound = null;
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
    }
}
