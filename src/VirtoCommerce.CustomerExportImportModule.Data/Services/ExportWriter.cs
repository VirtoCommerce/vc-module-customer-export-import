using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using VirtoCommerce.CustomerExportImportModule.Core.Services;
using VirtoCommerce.CustomerExportImportModule.Data.ExportImport;
using VirtoCommerce.Platform.Core.Assets;

namespace VirtoCommerce.CustomerExportImportModule.Data.Services
{
    public sealed class ExportWriter<T> : IExportWriter<T>
    {

        private readonly IBlobStorageProvider _blobStorageProvider;
        private readonly StreamWriter _streamWriter;
        private readonly CsvWriter _csvWriter;

        public ExportWriter(string filePath, IBlobStorageProvider blobStorageProvider, Configuration csvConfiguration)
        {
            _blobStorageProvider = blobStorageProvider;
            var stream = _blobStorageProvider.OpenWrite(filePath);
            _streamWriter = new StreamWriter(stream);
            _csvWriter = new CsvWriter(_streamWriter, csvConfiguration);
            _csvWriter.Configuration.RegisterClassMap(new GenericClassMap<T>());

        }

        public void WriteRecords(T[] records)
        {
            _csvWriter.WriteRecords(records);
        }

        public void Dispose()
        {
            _streamWriter.Flush();
            _streamWriter.Close();
            _csvWriter.Dispose();
        }
    }
}
