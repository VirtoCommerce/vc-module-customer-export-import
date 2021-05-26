using CsvHelper.Configuration;

namespace VirtoCommerce.CustomerExportImportModule.Core.Services
{
    public interface IExportWriterFactory
    {
        IExportWriter<T> Create<T>(string filePath, Configuration csvConfiguration, string[] dynamicProperties = null);
    }
}
