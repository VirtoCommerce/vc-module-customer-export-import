// Call this to register your module to main application
var moduleName = "virtoCommerce.customerExportImportModule";

if (AppDependencies !== undefined) {
    AppDependencies.push(moduleName);
}

angular.module(moduleName, []).run(["virtoCommerce.featureManagerSubscriber", "platformWebApp.dialogService", "platformWebApp.toolbarService", "platformWebApp.authService",
    function (featureManagerSubscriber, dialogService, toolbarService, authService) {
        featureManagerSubscriber.onLoginStatusChanged(
            "CustomerExportImport",
            () => {
                toolbarService.register({
                    name: "platform.commands.import",
                    icon: "fa fa-download",
                    executeMethod: function (blade) {
                        console.log(
                            `test: ${this.name} ${this.icon} ${blade.title}`
                        );
                    },
                    canExecuteMethod: function () {
                        return true;
                    },
                    // permission: '',
                    index: 4,
                },
                "virtoCommerce.customerModule.memberListController"
                );

                toolbarService.register({
                    name: "platform.commands.export",
                    icon: "fa fa-upload",
                    executeMethod: function (blade) {
                        const selection = blade.$scope.gridApi.selection;
                        if (!selection.getSelectedRows().length) {
                            selection.selectAllRows();
                        }
                        const isAllSelected = selection.getSelectAllState();
                        const selectedRows = selection.getSelectedRows();

                        const selectedOrganizationsList = _.filter(selectedRows, {memberType: "Organization"});
                        const selectedContactsList = _.filter(selectedRows, {memberType: "Contact"});

                        const selectedOrganizationsCounter = selectedOrganizationsList.length;
                        const selectedContactsCounter = selectedContactsList.length;
                        const totalItemsCount = selectedOrganizationsCounter + selectedContactsCounter;
                        const exportIsEmpty = !totalItemsCount;

                        const dialog = {
                            id: "customerExportDialog",
                            exportAll: isAllSelected,
                            selectedOrganizationsCounter,
                            selectedContactsCounter,
                            totalItemsCount,
                            exportIsEmpty,
                            callback: function (confirm) {
                                if (confirm) {
                                    console.log('Confirmed');
                                }
                            }
                        }
                        dialogService.showDialog(dialog, 'Modules/$(VirtoCommerce.CustomerExportImport)/Scripts/dialogs/customerExport-dialog.tpl.html', 'platformWebApp.confirmDialogController');

                                },
                    canExecuteMethod: function () {
                        return true;
                    },
                    // permission: '',
                    index: 5,
                },
                "virtoCommerce.customerModule.memberListController"
                );
            }
        );
    },
]);
