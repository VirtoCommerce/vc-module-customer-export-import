using CsvHelper.Configuration;
using VirtoCommerce.Platform.Core.DynamicProperties;

namespace VirtoCommerce.CustomerExportImportModule.Core.Services
{
    public interface IExportWriterFactory
    {
        IExportWriter<T> Create<T>(string filePath, Configuration csvConfiguration, DynamicProperty[] dynamicProperties = null);
    }
}
