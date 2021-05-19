// Call this to register your module to main application
var moduleName = "virtoCommerce.customerExportImportModule";

if (AppDependencies !== undefined) {
    AppDependencies.push(moduleName);
}

angular.module(moduleName, []).run(["virtoCommerce.featureManagerSubscriber", "platformWebApp.dialogService", "platformWebApp.toolbarService", "platformWebApp.authService", "virtoCommerce.customerModule.members", "virtoCommerce.customerModule.organizations",
    function (featureManagerSubscriber, dialogService, toolbarService, authService, members, organizations) {
        featureManagerSubscriber.onLoginStatusChanged(
            "CustomerExportImport",
            () => {

                toolbarService.register({
                    name: "platform.commands.import",
                    icon: "fa fa-download",
                    executeMethod: function (blade) {
                        const newBlade = {
                            id: 'customerImportFileUpload',
                            title: 'customerExportImport.blades.file-upload.title',
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
