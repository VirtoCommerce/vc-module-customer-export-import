using System.Linq;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Nager.Country;
using VirtoCommerce.CustomerExportImportModule.Core;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerExportImportModule.Core.Services;
using VirtoCommerce.CustomerExportImportModule.Data.Services;
using VirtoCommerce.CustomerExportImportModule.Data.Validation;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.CustomerExportImportModule.Web
{
    public class Module : IModule, IHasConfiguration
    {
        public ManifestModuleInfo ModuleInfo { get; set; }

        public IConfiguration Configuration { get; set; }

        public void Initialize(IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<ICustomerExportPagedDataSourceFactory, CustomerExportPagedDataSourceFactory>();
            serviceCollection.AddTransient<IExportWriterFactory, ExportWriterFactory>();
            serviceCollection.AddTransient<ICustomerDataExporter, CustomerDataExporter>();

            serviceCollection.AddTransient<IMemberSearchService, ExportImportMemberSearchService>();
            serviceCollection.AddSingleton<ICustomerImportPagedDataSourceFactory, CustomerImportPagedDataSourceFactory>();

            serviceCollection.AddTransient<ICsvCustomerDataValidator, CsvCustomerDataValidator>();
            serviceCollection.AddTransient<IValidator<ImportRecord<ImportableContact>[]>, ImportContactsValidator>();
            serviceCollection.AddTransient<IValidator<ImportRecord<ImportableOrganization>[]>, ImportOrganizationsValidator>();
            serviceCollection.AddTransient<IImportMemberValidator<ImportableContact>, ImportMemberValidator<ImportableContact>>();
            serviceCollection.AddTransient<IImportMemberValidator<ImportableOrganization>, ImportMemberValidator<ImportableOrganization>>();
            serviceCollection.AddTransient<IImportAddressValidator<ImportableContact>, ImportAddressValidator<ImportableContact>>();
            serviceCollection.AddTransient<IImportAddressValidator<ImportableOrganization>, ImportAddressValidator<ImportableOrganization>>();

            serviceCollection.AddTransient<IPasswordGenerator, PasswordGenerator>();

            serviceCollection.AddSingleton<ICsvCustomerImportReporterFactory, CsvCustomerImportReporterFactory>();

            serviceCollection.AddTransient<ICsvPagedCustomerDataImporter, CsvPagedContactDataImporter>();
            serviceCollection.AddTransient<ICsvPagedCustomerDataImporter, CsvPagedOrganizationDataImporter>();

            serviceCollection.AddOptions<ExportOptions>().Bind(Configuration.GetSection("CustomerExportImport:Export")).ValidateDataAnnotations();
            serviceCollection.AddOptions<ImportOptions>().Bind(Configuration.GetSection("CustomerExportImport:Import")).ValidateDataAnnotations();

            serviceCollection.AddTransient<ICountryProvider, CountryProvider>();
        }

        public void PostInitialize(IApplicationBuilder appBuilder)
        {
            // register settings
            var settingsRegistrar = appBuilder.ApplicationServices.GetRequiredService<ISettingsRegistrar>();
            settingsRegistrar.RegisterSettings(ModuleConstants.Settings.General.AllSettings, ModuleInfo.Id);

            var settingsManager = appBuilder.ApplicationServices.GetService<ISettingsManager>();
            var priceExportOptions = appBuilder.ApplicationServices.GetService<IOptions<ExportOptions>>()?.Value;

            settingsManager.SetValue(ModuleConstants.Settings.General.ExportLimitOfLines.Name,
                priceExportOptions?.LimitOfLines ?? ModuleConstants.Settings.General.ExportLimitOfLines.DefaultValue);

            var priceImportOptions = appBuilder.ApplicationServices.GetService<IOptions<ImportOptions>>()?.Value;

            settingsManager.SetValue(ModuleConstants.Settings.General.ImportLimitOfLines.Name,
                priceImportOptions?.LimitOfLines ?? ModuleConstants.Settings.General.ImportLimitOfLines.DefaultValue);

            settingsManager.SetValue(ModuleConstants.Settings.General.ImportFileMaxSize.Name,
                priceImportOptions?.FileMaxSize ?? ModuleConstants.Settings.General.ImportFileMaxSize.DefaultValue);

            // register permissions
            var permissionsProvider = appBuilder.ApplicationServices.GetRequiredService<IPermissionsRegistrar>();
            permissionsProvider.RegisterPermissions(ModuleConstants.Security.Permissions.AllPermissions.Select(x =>
                new Permission
                {
                    GroupName = "CustomerExportImport",
                    ModuleId = ModuleInfo.Id,
                    Name = x
                }).ToArray());
        }

        public void Uninstall()
        {
            // do nothing in here
        }
    }
}
