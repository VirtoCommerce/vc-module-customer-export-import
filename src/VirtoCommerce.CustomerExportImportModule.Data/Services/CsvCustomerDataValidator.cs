using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using VirtoCommerce.CustomerExportImportModule.Core;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerExportImportModule.Core.Services;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.CustomerExportImportModule.Data.Services
{
    public sealed class CsvCustomerDataValidator : ICsvCustomerDataValidator
    {
        private readonly IBlobStorageProvider _blobStorageProvider;
        private readonly ISettingsManager _settingsManager;

        public CsvCustomerDataValidator(IBlobStorageProvider blobStorageProvider, ISettingsManager settingsManager)
        {
            _blobStorageProvider = blobStorageProvider;
            _settingsManager = settingsManager;
        }

        public async Task<ImportDataValidationResult> ValidateAsync(string dataType, string filePath)
        {
            return dataType switch
            {
                nameof(Contact) => await ValidateAsync<CsvContact>(filePath),
                nameof(Organization) => await ValidateAsync<CsvOrganization>(filePath),
                _ => await ValidateAsync<CsvContact>(filePath),
            };
        }

        public async Task<ImportDataValidationResult> ValidateAsync<T>(string filePath)
        {
            var errorsList = new List<ImportDataValidationError>();

            var fileMaxSize = _settingsManager.GetValue(ModuleConstants.Settings.General.ImportFileMaxSize.Name,
                (int)ModuleConstants.Settings.General.ImportFileMaxSize.DefaultValue) * ModuleConstants.MByte;

            var blobInfo = await _blobStorageProvider.GetBlobInfoAsync(filePath);

            if (blobInfo == null)
            {
                var error = new ImportDataValidationError() { ErrorCode = ModuleConstants.ValidationErrors.FileNotExisted };
                errorsList.Add(error);
            }
            else if (blobInfo.Size > fileMaxSize)
            {
                var error = new ImportDataValidationError() { ErrorCode = ModuleConstants.ValidationErrors.ExceedingFileMaxSize };
                error.Properties.Add(nameof(fileMaxSize), fileMaxSize.ToString());
                error.Properties.Add(nameof(blobInfo.Size), blobInfo.Size.ToString());
                errorsList.Add(error);
            }
            else
            {
                var stream = _blobStorageProvider.OpenRead(filePath);
                var csvConfiguration = new ImportConfiguration();

                await ValidateDelimiterAndDataExists(stream, csvConfiguration, errorsList);

                ValidateRequiredColumns<T>(stream, csvConfiguration, errorsList);

                ValidateLineLimit(stream, csvConfiguration, errorsList);

                await stream.DisposeAsync();
            }

            var result = new ImportDataValidationResult { Errors = errorsList.ToArray() };

            return result;
        }

        private void ValidateLineLimit(Stream stream, Configuration csvConfiguration, List<ImportDataValidationError> errorsList)
        {
            var notCompatibleErrors = new[]
            {
                ModuleConstants.ValidationErrors.FileNotExisted,
                ModuleConstants.ValidationErrors.ExceedingFileMaxSize,
                ModuleConstants.ValidationErrors.NoData,
            };

            if (errorsList.Any(x => notCompatibleErrors.Contains(x.ErrorCode)))
            {
                return;
            }

            var importLimitOfLines = _settingsManager.GetValue(ModuleConstants.Settings.General.ImportLimitOfLines.Name,
                (int)ModuleConstants.Settings.General.ImportLimitOfLines.DefaultValue);

            stream.Seek(0, SeekOrigin.Begin);

            var streamReader = new StreamReader(stream);
            var csvReader = new CsvReader(streamReader, csvConfiguration);

            var totalCount = 0;

            csvReader.Read();
            csvReader.ReadHeader();

            while (csvReader.Read())
            {
                totalCount++;
            }

            if (totalCount > importLimitOfLines)
            {
                var error = new ImportDataValidationError() { ErrorCode = ModuleConstants.ValidationErrors.ExceedingLineLimits };
                error.Properties.Add(nameof(importLimitOfLines), importLimitOfLines.ToString());
                error.Properties.Add("LinesCount", totalCount.ToString());
                errorsList.Add(error);
            }
        }

        private static void ValidateRequiredColumns<T>(Stream stream, Configuration csvConfiguration, List<ImportDataValidationError> errorsList)
        {
            var notCompatibleErrors = new[]
            {
                ModuleConstants.ValidationErrors.FileNotExisted,
                ModuleConstants.ValidationErrors.ExceedingFileMaxSize,
                ModuleConstants.ValidationErrors.WrongDelimiter,
                ModuleConstants.ValidationErrors.NoData,
            };

            if (errorsList.Any(x => notCompatibleErrors.Contains(x.ErrorCode)))
            {
                return;
            }

            stream.Seek(0, SeekOrigin.Begin);
            var streamReader = new StreamReader(stream);
            var csvReader = new CsvReader(streamReader, csvConfiguration);

            csvReader.Read();
            csvReader.ReadHeader();

            var existedColumns = csvReader.Context.HeaderRecord;

            var requiredColumns = CsvCustomerImportHelper.GetImportCustomerRequiredColumns<T>();

            var missedColumns = requiredColumns.Except(existedColumns, StringComparer.InvariantCultureIgnoreCase).ToArray();

            if (missedColumns.Length > 0)
            {
                var error = new ImportDataValidationError() { ErrorCode = ModuleConstants.ValidationErrors.MissingRequiredColumns };
                error.Properties.Add(nameof(missedColumns), string.Join(", ", missedColumns));
                errorsList.Add(error);
            }
        }

        private static async Task ValidateDelimiterAndDataExists(Stream stream, Configuration csvConfiguration, List<ImportDataValidationError> errorsList)
        {

            var notCompatibleErrors = new[]
            {
                ModuleConstants.ValidationErrors.FileNotExisted,
                ModuleConstants.ValidationErrors.ExceedingFileMaxSize,
            };

            if (errorsList.Any(x => notCompatibleErrors.Contains(x.ErrorCode)))
            {
                return;
            }

            stream.Seek(0, SeekOrigin.Begin);
            var streamReader = new StreamReader(stream);

            var headerLine = await streamReader.ReadLineAsync();

            if (string.IsNullOrWhiteSpace(headerLine))
            {
                errorsList.Add(new ImportDataValidationError { ErrorCode = ModuleConstants.ValidationErrors.NoData });
            }
            else
            {
                if (!headerLine.Contains(csvConfiguration.Delimiter))
                {
                    errorsList.Add(new ImportDataValidationError { ErrorCode = ModuleConstants.ValidationErrors.WrongDelimiter });
                }

                var fistDataLine = await streamReader.ReadLineAsync();

                if (string.IsNullOrWhiteSpace(fistDataLine))
                {
                    errorsList.Add(new ImportDataValidationError { ErrorCode = ModuleConstants.ValidationErrors.NoData });
                }
            }
        }
    }
}
