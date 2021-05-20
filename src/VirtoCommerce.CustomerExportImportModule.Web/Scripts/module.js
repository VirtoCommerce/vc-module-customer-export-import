// Call this to register your module to main application
var moduleName = "virtoCommerce.customerExportImportModule";

if (AppDependencies !== undefined) {
    AppDependencies.push(moduleName);
}

angular.module(moduleName, []).run(["virtoCommerce.featureManagerSubscriber", "platformWebApp.bladeNavigationService", "platformWebApp.toolbarService",
    function (featureManagerSubscriber, bladeNavigationService, toolbarService) {
        featureManagerSubscriber.onLoginStatusChanged(
            "CustomerExportImport",
            () => {

                toolbarService.register({
                    name: "platform.commands.import",
                    icon: "fa fa-download",
                    executeMethod: function (blade) {
                        const newBlade = {
                            id: 'customerImportFileUpload',
                            title: (blade.currentEntity && blade.currentEntity.name) ? 'customerExportImport.blades.file-upload.title-member' : 'customerExportImport.blades.file-upload.title-root',
                            titleValues: (blade.currentEntity && blade.currentEntity.name) && { member: blade.currentEntity.name },
                            subtitle: 'customerExportImport.blades.file-upload.subtitle',
                            controller: 'virtoCommerce.customerExportImportModule.fileUploadController',
                            template: 'Modules/$(VirtoCommerce.CustomerExportImport)/Scripts/blades/file-upload.tpl.html'
                        };
                        bladeNavigationService.showBlade(newBlade, blade);
                    },
                    canExecuteMethod: function () { return true; },
                    index: 4,
                }, "virtoCommerce.customerModule.memberListController");

            }
        );
    },
]);
