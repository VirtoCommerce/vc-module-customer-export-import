using System;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CustomerExportImportModule.Core;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerExportImportModule.Core.Services;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CustomerExportImportModule.Data.Services
{
    public sealed class CustomerDataExporter : ICustomerDataExporter
    {
        private readonly ICustomerExportPagedDataSourceFactory _customerExportPagedDataSourceFactory;
        private readonly IBlobStorageProvider _blobStorageProvider;
        private readonly IExportWriterFactory _exportWriterFactory;

        public CustomerDataExporter(ICustomerExportPagedDataSourceFactory customerExportPagedDataSourceFactory, IBlobStorageProvider blobStorageProvider, IExportWriterFactory exportWriterFactory)
        {
            _customerExportPagedDataSourceFactory = customerExportPagedDataSourceFactory;
            _blobStorageProvider = blobStorageProvider;
            _exportWriterFactory = exportWriterFactory;
        }

        public async Task ExportAsync(ExportDataRequest request, Action<ExportProgressInfo> progressCallback, ICancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var exportProgress = new ExportProgressInfo { ProcessedCount = 0, Description = "Export has started" };

            var dataSource = _customerExportPagedDataSourceFactory.Create(ModuleConstants.Settings.PageSize, request);

            exportProgress.TotalCount = await dataSource.GetTotalCountAsync();
            progressCallback(exportProgress);

            const string exportDescription = "{0} out of {1} have been exported.";

            exportProgress.Description = "Fetching...";
            progressCallback(exportProgress);

            var contactExportWriter = _exportWriterFactory.Create<ExportableContact>("contacts.csv", new ExportConfiguration());

            var organizationExportWriter = _exportWriterFactory.Create<ExportableOrganization>("organization.csv", new ExportConfiguration());

            while (await dataSource.FetchAsync())
            {
                cancellationToken.ThrowIfCancellationRequested();

                var contacts = dataSource.Items.Select(x => x as ExportableContact).Where(x => x != null).ToArray();

                contactExportWriter.WriteRecords(contacts);

                var organizations = dataSource.Items.Select(x => x as ExportableOrganization).Where(x => x != null).ToArray();

                organizationExportWriter.WriteRecords(organizations);

                exportProgress.ProcessedCount += dataSource.Items.Length;
                exportProgress.Description = string.Format(exportDescription, exportProgress.ProcessedCount,
                    exportProgress.TotalCount);
                progressCallback(exportProgress);
            }

            exportProgress.Description = "Export completed";

            contactExportWriter.Dispose();
            organizationExportWriter.Dispose();

        }
    }
}
