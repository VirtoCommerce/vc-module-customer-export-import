using System.Linq;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VirtoCommerce.CustomerExportImportModule.Core;
using VirtoCommerce.CustomerExportImportModule.Core.Models;
using VirtoCommerce.CustomerExportImportModule.Core.Services;
using VirtoCommerce.CustomerExportImportModule.Data.Repositories;
using VirtoCommerce.CustomerExportImportModule.Data.Services;
using VirtoCommerce.CustomerExportImportModule.Data.Validation;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.FeatureManagementModule.Core.Services;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;
using featureManagementCore = VirtoCommerce.FeatureManagementModule.Core;

namespace VirtoCommerce.CustomerExportImportModule.Web
{
    public class Module : IModule, IHasConfiguration
    {
        public ManifestModuleInfo ModuleInfo { get; set; }

        public IConfiguration Configuration { get; set; }

        public void Initialize(IServiceCollection serviceCollection)
        {
            // database initialization
            var connectionString = Configuration.GetConnectionString("VirtoCommerce.CustomerExportImport") ?? Configuration.GetConnectionString("VirtoCommerce");
            serviceCollection.AddDbContext<VirtoCommerceCustomerExportImportDbContext>(options => options.UseSqlServer(connectionString));


            serviceCollection.AddTransient<ICustomerExportPagedDataSourceFactory, CustomerExportPagedDataSourceFactory>();
            serviceCollection.AddTransient<IExportWriterFactory, ExportWriterFactory>();
            serviceCollection.AddTransient<ICustomerDataExporter, CustomerDataExporter>();
            serviceCollection.AddTransient<IMemberSearchService, ExportImportMemberSearchService>();
            serviceCollection.AddSingleton<ICustomerImportPagedDataSourceFactory, CustomerImportPagedDataSourceFactory>();
            serviceCollection.AddTransient<ICsvCustomerDataValidator, CsvCustomerDataValidator>();
            serviceCollection.AddTransient<IValidator<ImportRecord<CsvContact>[]>, ImportContactsValidator>();
            serviceCollection.AddSingleton<ICsvCustomerImportReporterFactory, CsvCustomerImportReporterFactory>();
            serviceCollection.AddTransient<ICsvPagedCustomerDataImporter, CsvPagedContactDataImporter>();

            serviceCollection.AddOptions<ExportOptions>().Bind(Configuration.GetSection("CustomerExportImport:Export")).ValidateDataAnnotations();
            serviceCollection.AddOptions<ImportOptions>().Bind(Configuration.GetSection("CustomerExportImport:Import")).ValidateDataAnnotations();
        }

        public void PostInitialize(IApplicationBuilder appBuilder)
        {
            // register settings
            var settingsRegistrar = appBuilder.ApplicationServices.GetRequiredService<ISettingsRegistrar>();
            settingsRegistrar.RegisterSettings(ModuleConstants.Settings.General.AllSettings, ModuleInfo.Id);

            var settingsManager = appBuilder.ApplicationServices.GetService<ISettingsManager>();
            var priceExportOptions = appBuilder.ApplicationServices.GetService<IOptions<ExportOptions>>().Value;

            settingsManager.SetValue(ModuleConstants.Settings.General.ExportLimitOfLines.Name,
                priceExportOptions.LimitOfLines ?? ModuleConstants.Settings.General.ExportLimitOfLines.DefaultValue);

            var priceImportOptions = appBuilder.ApplicationServices.GetService<IOptions<ImportOptions>>().Value;

            settingsManager.SetValue(ModuleConstants.Settings.General.ImportLimitOfLines.Name,
                priceImportOptions.LimitOfLines ?? ModuleConstants.Settings.General.ImportLimitOfLines.DefaultValue);

            settingsManager.SetValue(ModuleConstants.Settings.General.ImportFileMaxSize.Name,
                priceImportOptions.FileMaxSize ?? ModuleConstants.Settings.General.ImportFileMaxSize.DefaultValue);

            // register permissions
            var permissionsProvider = appBuilder.ApplicationServices.GetRequiredService<IPermissionsRegistrar>();
            permissionsProvider.RegisterPermissions(ModuleConstants.Security.Permissions.AllPermissions.Select(x =>
                new Permission()
                {
                    GroupName = "CustomerExportImport",
                    ModuleId = ModuleInfo.Id,
                    Name = x
                }).ToArray());

            var featureStorage = appBuilder.ApplicationServices.GetService<IFeatureStorage>();
            featureStorage.TryAddFeatureDefinition(ModuleConstants.Features.CustomerExportImport, featureManagementCore.ModuleConstants.FeatureFilters.Developers);

            // ensure that all pending migrations are applied
            using var serviceScope = appBuilder.ApplicationServices.CreateScope();
            using var dbContext = serviceScope.ServiceProvider.GetRequiredService<VirtoCommerceCustomerExportImportDbContext>();
            dbContext.Database.EnsureCreated();
            dbContext.Database.Migrate();
        }

        public void Uninstall()
        {
            // do nothing in here
        }
    }
}
