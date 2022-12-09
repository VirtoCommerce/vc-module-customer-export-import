using System.Collections.Generic;
using CsvHelper.Configuration;
using VirtoCommerce.Platform.Core.DynamicProperties;

namespace VirtoCommerce.CustomerExportImportModule.Core.Services
{
    public interface IExportWriterFactory
    {
        IExportWriter<T> Create<T>(string filePath, CsvConfiguration csvConfiguration, IList<DynamicProperty> dynamicProperties = null)
            where T : IHasDynamicProperties;
    }
}
