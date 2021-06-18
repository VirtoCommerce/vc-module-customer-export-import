using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VirtoCommerce.CustomerExportImportModule.Core;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerExportImportModule.Core.Services;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.Platform.Core;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.Platform.Core.Exceptions;
using VirtoCommerce.Platform.Core.Extensions;

namespace VirtoCommerce.CustomerExportImportModule.Data.Services
{
    public sealed class CustomerDataExporter : ICustomerDataExporter
    {
        private readonly ICustomerExportPagedDataSourceFactory _customerExportPagedDataSourceFactory;
        private readonly IExportWriterFactory _exportWriterFactory;
        private readonly PlatformOptions _platformOptions;
        private readonly IBlobStorageProvider _blobStorageProvider;
        private readonly IDynamicPropertySearchService _dynamicPropertySearchService;




        public CustomerDataExporter(ICustomerExportPagedDataSourceFactory customerExportPagedDataSourceFactory, IExportWriterFactory exportWriterFactory, IOptions<PlatformOptions> platformOptions,
            IDynamicPropertySearchService dynamicPropertySearchService, IBlobStorageProvider blobStorageProvider)
        {
            _customerExportPagedDataSourceFactory = customerExportPagedDataSourceFactory;
            _exportWriterFactory = exportWriterFactory;
            _platformOptions = platformOptions.Value;
            _blobStorageProvider = blobStorageProvider;
            _dynamicPropertySearchService = dynamicPropertySearchService;
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

            var dynamicProperties = await _dynamicPropertySearchService.SearchDynamicPropertiesAsync(new DynamicPropertySearchCriteria()
            {
                ObjectTypes = new List<string> { typeof(Contact).FullName, typeof(Organization).FullName },
                Skip = 0,
                Take = int.MaxValue
            });

            var contactsDynamicProperties =
                dynamicProperties.Results.Where(x => x.ObjectType == typeof(Contact).FullName).ToArray();

            var organizationsDynamicProperties =
                dynamicProperties.Results.Where(x => x.ObjectType == typeof(Organization).FullName).ToArray();

            var contactsFilePath = GetExportFilePath("Contacts");
            var contactExportWriter = _exportWriterFactory.Create<CsvContact>(contactsFilePath, new ExportConfiguration(), contactsDynamicProperties);

            var organizationFilePath = GetExportFilePath("Organizations");
            var organizationExportWriter = _exportWriterFactory.Create<CsvOrganization>(organizationFilePath, new ExportConfiguration(), organizationsDynamicProperties);

            try
            {
                while (await dataSource.FetchAsync())
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var contacts = dataSource.Items.OfType<CsvContact>().ToArray();

                    if (!contacts.IsNullOrEmpty())
                    {
                        contactExportWriter.WriteRecords(contacts);
                    }                    

                    var organizations = dataSource.Items.OfType<CsvOrganization>().ToArray();

                    if (!organizations.IsNullOrEmpty())
                    {
                        organizationExportWriter.WriteRecords(organizations);
                    }

                    exportProgress.ProcessedCount += dataSource.Items.Length;
                    exportProgress.Description = string.Format(exportDescription, exportProgress.ProcessedCount,
                        exportProgress.TotalCount);
                    progressCallback(exportProgress);
                }

                exportProgress.Description = "Export completed";

            }
            finally
            {
                contactExportWriter.Dispose();
                organizationExportWriter.Dispose();
            }

            try
            {
                var contactsFileInfo = await _blobStorageProvider.GetBlobInfoAsync(contactsFilePath);
                var organizationsFileInfo = await _blobStorageProvider.GetBlobInfoAsync(organizationFilePath);

                if (contactsFileInfo.Size > 0)
                {
                    exportProgress.ContactsFileUrl = contactsFilePath;
                }

                if (organizationsFileInfo.Size > 0)
                {
                    exportProgress.OrganizationsFileUrl = organizationFilePath;
                }
            }
            finally
            {
                progressCallback(exportProgress);
            }
        }

        private string GetExportFilePath(string entityName)
        {
            if (string.IsNullOrEmpty(_platformOptions.DefaultExportFolder))
            {
                throw new PlatformException($"{nameof(_platformOptions.DefaultExportFolder)} must be set.");
            }

            const string template = "{0}_{1:yyyyMMddHHmmss}.csv";

            var result = string.Format(template, entityName, DateTime.UtcNow);

            return UrlHelperExtensions.Combine(_platformOptions.DefaultExportFolder, result);
        }
    }
}
