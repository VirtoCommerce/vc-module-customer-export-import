using System.Collections.Generic;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using VirtoCommerce.CustomerExportImportModule.Core.Services;
using VirtoCommerce.CustomerExportImportModule.Data.ExportImport;
using VirtoCommerce.AssetsModule.Core.Assets;
using VirtoCommerce.Platform.Core.DynamicProperties;

namespace VirtoCommerce.CustomerExportImportModule.Data.Services
{
    public sealed class ExportWriter<T> : IExportWriter<T> where T : IHasDynamicProperties
    {
        private readonly StreamWriter _streamWriter;
        private readonly CsvWriter _csvWriter;

        public ExportWriter(string filePath, IBlobStorageProvider blobStorageProvider, CsvConfiguration csvConfiguration, IList<DynamicProperty> dynamicProperties = null)
        {
            var stream = blobStorageProvider.OpenWrite(filePath);
            _streamWriter = new StreamWriter(stream);
            _csvWriter = new CsvWriter(_streamWriter, csvConfiguration);
            _csvWriter.Context.RegisterClassMap(new GenericClassMap<T>(dynamicProperties));
        }

        public void WriteRecords(T[] records)
        {
            _csvWriter.WriteRecords(records);
        }

        public void Dispose()
        {
            _csvWriter.Dispose();
            _streamWriter.Dispose();
        }
    }
}
