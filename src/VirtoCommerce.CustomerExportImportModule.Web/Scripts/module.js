// Call this to register your module to main application
var moduleName = "virtoCommerce.customerExportImportModule";

if (AppDependencies !== undefined) {
    AppDependencies.push(moduleName);
}

angular.module(moduleName, []).run([
    'virtoCommerce.featureManagerSubscriber', 'platformWebApp.dialogService', 'platformWebApp.toolbarService', 'virtoCommerce.customerModule.members', 'virtoCommerce.customerExportImportModule.export', 'platformWebApp.settings', '$q', 'platformWebApp.bladeNavigationService',
    function (featureManagerSubscriber, dialogService, toolbarService, members, exportResources, settings, $q, bladeNavigationService) {
        featureManagerSubscriber.onLoginStatusChanged('CustomerExportImport', () => {
            toolbarService.register(
                {
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
            toolbarService.register(
                {
                    name: 'platform.commands.export',
                    icon: 'fa fa-upload',
                    executeMethod: async function (blade) {
                        const scope = blade.$scope;

                        const contactMemberTypeName = 'Contact';
                        const organizationMemberTypeName = 'Organization';

                        const selection = scope.gridApi.selection;
                        const organizationId = blade.currentEntity.id;
                        const organizationName = blade.currentEntity.name;
                        const keyword = blade.filter.keyword;
                        const isAllSelected = !selection.getSelectedRows().length;
                        const exportDataRequest = {
                            keyword,
                            objectIds: [],
                            organizationId
                        };

                        const getExportLimits = () => settings.getValues({ id: 'CustomerExportImport.Export.LimitOfLines' }, (value) => value).$promise;

                        $q.when(getExportLimits()).then((value) => {
                            const maxMembersPerFile = value[0];

                            if (!isAllSelected) {
                                const selectedRows = selection.getSelectedRows();
                                const selectedContactsList = _.filter(selectedRows, { memberType: contactMemberTypeName });
                                const selectedOrganizationsList = _.filter(selectedRows, { memberType: organizationMemberTypeName });

                                let contactsCount = selectedContactsList.length;
                                let organizationsCount = selectedOrganizationsList.length;

                                const selectedMembersList = selectedContactsList.concat(selectedOrganizationsList);
                                exportDataRequest.objectIds = _.pluck(selectedMembersList, 'id');
                            }

                            const contactsSearchRequest = members.search(getSearchCriteria(contactMemberTypeName, organizationId, keyword, exportDataRequest.objectIds)).$promise;
                            const organizationsSearchRequest = members.search(getSearchCriteria(organizationMemberTypeName, organizationId, keyword, exportDataRequest.objectIds)).$promise;

                            $q.all([contactsSearchRequest, organizationsSearchRequest]).then(([contactsSearchResponse, organizationsSearchResponse]) => {
                                const contactsNumber = contactsSearchResponse.totalCount;
                                const organizationsNumber = organizationsSearchResponse.totalCount;
                                const membersTotalNumber = contactsNumber + organizationsNumber;
                                if (membersTotalNumber > maxMembersPerFile) {
                                    showWarningDialog(membersTotalNumber, maxMembersPerFile);
                                    return;
                                }
                                showExportDialog(contactsNumber, organizationsNumber);
                            });
                        });

                        function getSearchCriteria(memberType, memberId, searchKey, objectIds) {
                            return {
                                memberType,
                                memberId,
                                objectIds: objectIds,
                                keyword: searchKey,
                                deepSearch: true,
                                objectType: 'Member',
                                take: 0
                            };
                        }

                        function showWarningDialog(flattenMembersQty, limitQty) {
                            const dialog = {
                                id: 'customerWarningDialog',
                                flattenMembersQty,
                                limitQty
                            };
                            dialogService.showDialog(dialog, 'Modules/$(VirtoCommerce.CustomerExportImport)/Scripts/dialogs/customerWarning-dialog.tpl.html', 'platformWebApp.confirmDialogController');
                        }

                        function showExportDialog(contactsQty, organizationsQty) {
                            const totalQty = contactsQty + organizationsQty;
                            const exportIsEmpty = !totalQty;

                            const dialog = {
                                id: 'customerExportDialog',
                                contactsQty,
                                organizationsQty,
                                organizationName,
                                exportAll: isAllSelected,
                                exportIsEmpty,
                                callback: (success) => {
                                    if (success) {
                                        exportResources.run(exportDataRequest, (data) => {
                                            const newBlade = {
                                                id: 'customerExportProcessing',
                                                notification: data,
                                                headIcon: "fa fa-download",
                                                title: 'customerExportImport.blades.export-processing.title',
                                                controller: 'virtoCommerce.customerExportImportModule.exportProcessingController',
                                                template:
                                                    'Modules/$(VirtoCommerce.CustomerExportImport)/Scripts/blades/export-processing.tpl.html'
                                            };

                                            bladeNavigationService.showBlade(newBlade, blade);
                                        });
                                    }
                                }
                            };
                            dialogService.showDialog(dialog,
                                'Modules/$(VirtoCommerce.CustomerExportImport)/Scripts/dialogs/customerExport-dialog.tpl.html',
                                'platformWebApp.confirmDialogController');
                        }
                    },
                    canExecuteMethod: function () {
                        return true;
                    },
                    index: 5
                },
                'virtoCommerce.customerModule.memberListController'
            );
        });
    }
]);
