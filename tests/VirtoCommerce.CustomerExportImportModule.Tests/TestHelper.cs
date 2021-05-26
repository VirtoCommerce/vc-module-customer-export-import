using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Moq;
using VirtoCommerce.CustomerExportImportModule.Core;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerExportImportModule.Data.Services;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.CustomerExportImportModule.Tests
{
    public static class TestHelper
    {
        public static IBlobStorageProvider GetBlobStorageProvider(string csv, MemoryStream errorReporterMemoryStream = null)
        {
            errorReporterMemoryStream ??= new MemoryStream();
            var blobStorageProviderMock = new Mock<IBlobStorageProvider>();
            var stream = GetStream(csv);
            blobStorageProviderMock.Setup(x => x.OpenRead(It.IsAny<string>())).Returns(() => stream);
            blobStorageProviderMock.Setup(x => x.OpenWrite(It.IsAny<string>())).Returns(() => errorReporterMemoryStream);
            blobStorageProviderMock.Setup(x => x.GetBlobInfoAsync(It.IsAny<string>()))
                .Returns(() => Task.FromResult(new BlobInfo { Size = stream.Length }));
            return blobStorageProviderMock.Object;
        }

        public static CustomerImportPagedDataSourceFactory GetCustomerImportPagedDataSourceFactory(IBlobStorageProvider blobStorageProvider)
        {
            return new CustomerImportPagedDataSourceFactory(blobStorageProvider);
        }

        public static Stream GetStream(string csv)
        {
            var stream = new MemoryStream();
            using var writer = new StreamWriter(stream, leaveOpen: true);
            writer.Write(csv);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static string GetCsv(IEnumerable<string> records, string header = null)
        {
            var csv = new StringBuilder();

            if (header != null)
            {
                csv.AppendLine(header);
            }

            foreach (var record in records)
            {
                csv.AppendLine(record);
            }

            return csv.ToString();
        }

        public static string[] GetArrayOfSameRecords(string recordValue, long number)
        {
            var result = new List<string>();

            for (long i = 0; i < number; i++)
            {
                result.Add(recordValue);
            }

            return result.ToArray();
        }

        public static Mock<ISettingsManager> GetSettingsManagerMoq()
        {
            var settingsManagerMoq = new Mock<ISettingsManager>();

            settingsManagerMoq.Setup(x =>
                    x.GetObjectSettingAsync(
                        It.Is<string>(x => x == ModuleConstants.Settings.General.ImportFileMaxSize.Name),
                        null, null))
                .ReturnsAsync(new ObjectSettingEntry()
                { Value = ModuleConstants.Settings.General.ImportFileMaxSize.DefaultValue });

            settingsManagerMoq.Setup(x =>
                    x.GetObjectSettingAsync(
                        It.Is<string>(x => x == ModuleConstants.Settings.General.ImportLimitOfLines.Name),
                        null, null))
                .ReturnsAsync(new ObjectSettingEntry()
                { Value = ModuleConstants.Settings.General.ImportLimitOfLines.DefaultValue });
            return settingsManagerMoq;
        }

        public static ImportDataRequest CreateImportDataRequest()
        {
            return new ImportDataRequest { FilePath = "https://localhost/test_url.csv" };
        }

    }
}
