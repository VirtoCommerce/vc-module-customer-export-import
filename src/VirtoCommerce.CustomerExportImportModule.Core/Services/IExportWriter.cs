using System;

namespace VirtoCommerce.CustomerExportImportModule.Core.Services
{
    public interface IExportWriter<in T> : IDisposable
    {
        void WriteRecords(T[] records);
    }
}
