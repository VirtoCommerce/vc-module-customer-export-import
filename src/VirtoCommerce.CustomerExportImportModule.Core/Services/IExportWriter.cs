using System;

namespace VirtoCommerce.CustomerExportImportModule.Core.Services
{
    public interface IExportWriter<in T> : IDisposable
    {
        public void WriteRecords(T[] records);
    }
}
