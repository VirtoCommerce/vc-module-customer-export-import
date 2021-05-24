// Call this to register your module to main application
var moduleName = 'virtoCommerce.customerExportImportModule';

if (AppDependencies !== undefined) {
    AppDependencies.push(moduleName);
}

angular.module(moduleName, []).run([
    'virtoCommerce.featureManagerSubscriber', 'platformWebApp.dialogService', 'platformWebApp.toolbarService', 'virtoCommerce.customerModule.members', 'virtoCommerce.customerExportImportModule.export', 'platformWebApp.settings', '$q',
    function (featureManagerSubscriber, dialogService, toolbarService, members, exportResources, settings, $q) {
        featureManagerSubscriber.onLoginStatusChanged('CustomerExportImport', () => {
            toolbarService.register(
                {
                    name: 'platform.commands.export',
                    icon: 'fa fa-upload',
                    executeMethod: async function (blade) {
                        const scope = blade.$scope;

                        const selection = scope.gridApi.selection;
                        const organizationId = blade.currentEntity.id;
                        const organizationName = blade.currentEntity.name;
                        const keyword = blade.filter.keyword;
                        const isAllSelected = !selection.getSelectedRows().length;
                        const exportDataRequest = {
                            keyword,
                            memberIds: [],
                            organizationId
                        };

                        const getExportLimits = () => settings.getValues({ id: 'CustomerExportImport.Export.LimitOfLines' }, (value) => value).$promise;

                        $q.when(getExportLimits()).then((value) => {
                            const maxMembersPerFile = value[0];

                            if (isAllSelected) {
                                const contactsSearchRequest = members.search(getSearchCriteria('Contact', organizationId, keyword)).$promise;
                                const organizationsSearchRequest = members.search(getSearchCriteria('Organization', organizationId, keyword)).$promise;

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
                            } else {
                                const selectedRows = selection.getSelectedRows();
                                const selectedContactsList = _.filter(selectedRows, { memberType: 'Contact' });
                                const selectedOrganizationsList = _.filter(selectedRows, { memberType: 'Organization' });

                                let contactsCount = selectedContactsList.length;
                                let organizationsCount = selectedOrganizationsList.length;

                                const selectedMembersList = selectedContactsList.concat(selectedOrganizationsList);
                                exportDataRequest.memberIds = _.pluck(selectedMembersList, 'id');

                                const organizationsSearchRequests = selectedOrganizationsList.map((item) => members.search(getSearchCriteria('Organization', item.id, keyword)).$promise);
                                const r1 = $q.all(organizationsSearchRequests);
                                const contactsSearchRequests = selectedOrganizationsList.map((item) => members.search(getSearchCriteria('Contact', item.id, keyword)).$promise);
                                const r2 = $q.all(contactsSearchRequests);

                                $q.all([r1, r2]).then((data) => {
                                    for (let i = 0; i < data[0].length; i++) {
                                        organizationsCount += data[0][i].totalCount;
                                    }
                                    for (let i = 0; i < data[1].length; i++) {
                                        contactsCount += data[1][i].totalCount;
                                    }

                                    const membersTotalNumber = contactsCount + organizationsCount;
                                    if (membersTotalNumber > maxMembersPerFile) {
                                        showWarningDialog(membersTotalNumber, maxMembersPerFile);
                                        return;
                                    }

                                    showExportDialog(contactsCount, organizationsCount);
                                });
                            }
                        });

                        function getSearchCriteria(memberType, memberId, keyword) {
                            return {
                                memberType,
                                memberId,
                                keyword,
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

                        function showExportDialog(contactsQty, organizationsQty, flattenMembersQty) {
                            const totalQty = contactsQty + organizationsQty;
                            const exportIsEmpty = !totalQty;

                            const dialog = {
                                id: 'customerExportDialog',
                                contactsQty,
                                organizationsQty,
                                totalQty,
                                organizationName,
                                exportAll: isAllSelected,
                                exportIsEmpty,
                                flattenMembersQty,
                                callback: () => {}
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
