using System.IO;
using System.Threading.Tasks;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerExportImportModule.Core.Services;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CustomerExportImportModule.Data.Services
{
    public sealed class CsvCustomerImportReporter : ICsvCustomerImportReporter
    {
        private readonly IBlobStorageProvider _blobStorageProvider;
        private readonly string _reportFilePath;
        private readonly string _delimiter;
        private readonly StreamWriter _streamWriter;
        private const string ErrorsColumnName = "Error description";

        public bool ReportIsNotEmpty { get; private set; } = false;

        public CsvCustomerImportReporter(string reportFilePath, IBlobStorageProvider blobStorageProvider, string delimiter)
        {
            _reportFilePath = reportFilePath;
            _delimiter = delimiter;
            _blobStorageProvider = blobStorageProvider;
            var stream = _blobStorageProvider.OpenWrite(reportFilePath);
            _streamWriter = new StreamWriter(stream);
        }

        public async Task WriteAsync(ImportError error)
        {
            using (await AsyncLock.GetLockByKey(_reportFilePath).LockAsync())
            {
                ReportIsNotEmpty = true;
                await _streamWriter.WriteLineAsync(GetLine(error));
            }
        }

        public async Task WriteHeaderAsync(string header)
        {
            using (await AsyncLock.GetLockByKey(_reportFilePath).LockAsync())
            {
                await _streamWriter.WriteLineAsync($"{ErrorsColumnName}{_delimiter}{header}");
            }
        }

        public async ValueTask DisposeAsync()
        {
            using (await AsyncLock.GetLockByKey(_reportFilePath).LockAsync())
            {
                await _streamWriter.FlushAsync();
                _streamWriter.Close();

                if (!ReportIsNotEmpty)
                {
                    await _blobStorageProvider.RemoveAsync(new[] { _reportFilePath });
                }
            }
        }


        private string GetLine(ImportError importError)
        {
            var result = $"{importError.Error}{_delimiter}{importError.RawRow.TrimEnd()}";

            return result;
        }
    }
}
