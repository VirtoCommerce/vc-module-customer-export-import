using System.Collections.Generic;
using CsvHelper.Configuration;
using VirtoCommerce.CustomerExportImportModule.Core.Services;
using VirtoCommerce.AssetsModule.Core.Assets;
using VirtoCommerce.Platform.Core.DynamicProperties;

namespace VirtoCommerce.CustomerExportImportModule.Data.Services
{
    public sealed class ExportWriterFactory : IExportWriterFactory
    {
        private readonly IBlobStorageProvider _blobStorageProvider;

        public ExportWriterFactory(IBlobStorageProvider blobStorageProvider)
        {
            _blobStorageProvider = blobStorageProvider;
        }

        public IExportWriter<T> Create<T>(string filePath, CsvConfiguration csvConfiguration, IList<DynamicProperty> dynamicProperties = null)
            where T : IHasDynamicProperties
        {
            return new ExportWriter<T>(filePath, _blobStorageProvider, csvConfiguration, dynamicProperties);
        }
    }
}
