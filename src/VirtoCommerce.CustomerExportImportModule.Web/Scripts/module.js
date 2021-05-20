// Call this to register your module to main application
var moduleName = 'virtoCommerce.customerExportImportModule';

if (AppDependencies !== undefined) {
    AppDependencies.push(moduleName);
}

angular.module(moduleName, []).run([
    'virtoCommerce.featureManagerSubscriber',
    'platformWebApp.dialogService',
    'platformWebApp.toolbarService',
    'virtoCommerce.customerModule.members',
    'virtoCommerce.customerExportImportModule.export',
    function (featureManagerSubscriber, dialogService, toolbarService, members, exportResources) {
        featureManagerSubscriber.onLoginStatusChanged('CustomerExportImport', () => {
            toolbarService.register(
                {
                    name: 'platform.commands.export',
                    icon: 'fa fa-upload',
                    executeMethod: async function (blade) {
                        const scope = blade.$scope;
                        const selection = scope.gridApi.selection;
                        const keyword = blade.filter.keyword;
                        const organizationId = blade.currentEntity.id;
                        const exportDataRequest = {
                            keyword,
                            memberIds: [],
                            organizationId
                        };

                        const getSearchCriteria = (memberType) => {
                            return {
                                memberType,
                                memberId: organizationId,
                                keyword: keyword || undefined,
                                deepSearch: !!keyword,
                                objectType: 'Member',
                                take: 0
                            };
                        };

                        const isAllSelected = !selection.getSelectedRows().length;

                        if (isAllSelected) {
                            members.search(getSearchCriteria('Contact'), (contactsSearchResponse) => {
                                members.search(getSearchCriteria('Organization'), (organizationsSearchResponse) => {
                                    const contactsQty = contactsSearchResponse.totalCount;
                                    const organizationsQty = organizationsSearchResponse.totalCount;

                                    const dialogData = {
                                        isAllSelected,
                                        contactsQty,
                                        organizationsQty
                                    };

                                    showExportDialog(dialogData);
                                });
                            });
                        } else {
                            const selectedRows = selection.getSelectedRows();

                            const selectedContactsList = _.filter(selectedRows, { memberType: 'Contact' });
                            const selectedOrganizationsList = _.filter(selectedRows, { memberType: 'Organization' });
                            const selectedMembersList = selectedContactsList.concat(selectedOrganizationsList);

                            exportDataRequest.memberIds = _.pluck(selectedMembersList, 'id');

                            const contactsQty = selectedContactsList.length;
                            const organizationsQty = selectedOrganizationsList.length;

                            const dialogData = {
                                isAllSelected,
                                contactsQty,
                                organizationsQty
                            };

                            showExportDialog(dialogData);
                        }

                        function showExportDialog({ isAllSelected, contactsQty, organizationsQty }) {
                            const totalQty = contactsQty + organizationsQty;
                            const exportIsEmpty = !totalQty;
                            const dialog = {
                                id: 'customerExportDialog',
                                exportAll: isAllSelected,
                                contactsQty,
                                organizationsQty,
                                totalQty,
                                exportIsEmpty,
                                callback: function (confirm) {
                                    if (confirm) {
                                        console.log('Confirmed');
                                        console.log(exportDataRequest);
                                        exportResources.run(exportDataRequest);

                                        // blade.isExporting = true;
                                        // var progressBlade = {
                                        //     id: 'exportProgress',
                                        //     title: 'export.blades.export-progress.title',
                                        //     controller: 'virtoCommerce.exportModule.exportProgressController',
                                        //     template: 'Modules/$(VirtoCommerce.Export)/Scripts/blades/export-progress.tpl.html',
                                        //     exportDataRequest: exportDataRequest,
                                        //     onCompleted: function () {
                                        //         blade.isExporting = false;
                                        //     }
                                        // };
                                        // bladeNavigationService.showBlade(progressBlade, blade);
                                    }
                                }
                            };
                            dialogService.showDialog(dialog, 'Modules/$(VirtoCommerce.CustomerExportImport)/Scripts/dialogs/customerExport-dialog.tpl.html', 'platformWebApp.confirmDialogController');
                        }
                    },
                    canExecuteMethod: function () {
                        return true;
                    },
                    index: 4
                },
                'virtoCommerce.customerModule.memberListController'
            );
        });
    }
]);
