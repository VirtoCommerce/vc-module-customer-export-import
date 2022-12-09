using System.IO;
using System.Threading.Tasks;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerExportImportModule.Core.Services;
using VirtoCommerce.AssetsModule.Core.Assets;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CustomerExportImportModule.Data.Services
{
    public sealed class CsvCustomerImportReporter : ICsvCustomerImportReporter
    {
        private readonly IBlobStorageProvider _blobStorageProvider;
        private readonly string _delimiter;
        private readonly StreamWriter _streamWriter;
        private const string ErrorsColumnName = "Error description";

        public bool ReportIsNotEmpty { get; private set; }

        public string FilePath { get; }

        public CsvCustomerImportReporter(string filePath, IBlobStorageProvider blobStorageProvider, string delimiter)
        {
            FilePath = filePath;
            _delimiter = delimiter;
            _blobStorageProvider = blobStorageProvider;
            var stream = _blobStorageProvider.OpenWrite(filePath);
            _streamWriter = new StreamWriter(stream);
        }

        public async Task WriteAsync(ImportError error)
        {
            using (await AsyncLock.GetLockByKey(FilePath).GetReleaserAsync())
            {
                ReportIsNotEmpty = true;
                await _streamWriter.WriteLineAsync(GetLine(error));
            }
        }

        public async Task WriteHeaderAsync(string header)
        {
            using (await AsyncLock.GetLockByKey(FilePath).GetReleaserAsync())
            {
                await _streamWriter.WriteLineAsync($"{ErrorsColumnName}{_delimiter}{header}");
            }
        }

        public async ValueTask DisposeAsync()
        {
            using (await AsyncLock.GetLockByKey(FilePath).GetReleaserAsync())
            {
                await _streamWriter.DisposeAsync();

                if (!ReportIsNotEmpty)
                {
                    await _blobStorageProvider.RemoveAsync(new[] { FilePath });
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
