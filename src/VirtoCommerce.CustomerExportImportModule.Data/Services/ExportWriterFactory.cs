using CsvHelper.Configuration;
using VirtoCommerce.CustomerExportImportModule.Core.Services;
using VirtoCommerce.Platform.Core.Assets;

namespace VirtoCommerce.CustomerExportImportModule.Data.Services
{
    public sealed class ExportWriterFactory : IExportWriterFactory
    {
        private readonly IBlobStorageProvider _blobStorageProvider;

        public ExportWriterFactory(IBlobStorageProvider blobStorageProvider)
        {
            _blobStorageProvider = blobStorageProvider;
        }
        public IExportWriter<T> Create<T>(string filePath, Configuration csvConfiguration, string[] dynamicProperties = null)
        {
            return new ExportWriter<T>(filePath, _blobStorageProvider, csvConfiguration, dynamicProperties);
        }
    }
}
