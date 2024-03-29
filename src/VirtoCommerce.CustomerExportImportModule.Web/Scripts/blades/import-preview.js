angular.module('virtoCommerce.customerExportImportModule')
    .controller('virtoCommerce.customerExportImportModule.importPreviewController', ['$scope', 'virtoCommerce.customerExportImportModule.import', 'platformWebApp.bladeNavigationService', 'uiGridConstants', 'platformWebApp.bladeUtils', 'platformWebApp.dialogService', function ($scope, importResources, bladeNavigationService, uiGridConstants, bladeUtils, dialogService) {
        $scope.uiGridConstants = uiGridConstants;

        var blade = $scope.blade;

        function initialize() {
            blade.isLoading = true;
            $scope.showUnparsedRowsWarning = false;

            importResources.preview({ filePath: blade.csvFilePath, dataType: blade.dataType.value }, (response) => {
                const records = response.results;

                _.each(records, record => {
                    _.each(record.dynamicProperties, prop => {
                        _.extend(record, { [prop.name]: prop.values.map(value => value.value).join(', ') });
                    });
                    _.omit(record, 'dynamicProperties');
                });

                const columnNames = _.keys(records[0]);
                const idColumnNames = _.filter(columnNames, key => key.includes('Id'));

                $scope.originalRecords = _.map(records, record => ({...record}));

                _.each(records, record => {
                    _.each(idColumnNames, columnName => {
                        record[columnName] = truncateId(record[columnName]);
                    });
                });

                blade.currentEntities = records;
                blade.totalCount = response.totalCount;
                $scope.pageSettings.totalItems = 10;
                getInvalidRowsCount();
                blade.isLoading = false;
            }, (error) => { bladeNavigationService.setError('Error ' + error.status, blade); });
        }

        blade.toolbarCommands = [
            {
                name: "platform.commands.import",
                icon: "fa fa-download",
                canExecuteMethod: () => true,
                executeMethod: () => {
                    const dialog = {
                        id: "customerImportDialog",
                        membersQty: blade.totalCount,
                        dataType: blade.dataType,
                        organizationName: blade.organizationName,
                        callback: (confirm) => {
                            if (confirm) {
                                const importDataRequest = {
                                    filePath: blade.csvFilePath,
                                    memberType: blade.dataType.value,
                                    organizationId: blade.organizationId
                                };

                                importResources.run(importDataRequest, (data) => {
                                    var newBlade = {
                                        id: "customerImportProcessing",
                                        notification: data,
                                        dataType: blade.dataType,
                                        headIcon: "fa fa-download",
                                        title: "customerExportImport.blades.import-processing.title",
                                        controller: "virtoCommerce.customerExportImportModule.importProcessingController",
                                        template: "Modules/$(VirtoCommerce.CustomerExportImport)/Scripts/blades/import-processing.tpl.html",
                                    };

                                    bladeNavigationService.showBlade(newBlade, blade);
                                });

                            }
                        }
                    };
                    dialogService.showDialog(dialog, "Modules/$(VirtoCommerce.CustomerExportImport)/Scripts/dialogs/customerImport-dialog.tpl.html", "platformWebApp.confirmDialogController");
                },
            },
            {
                name: "customerExportImport.blades.import-preview.close",
                icon: 'fa fa-close',
                canExecuteMethod: () => true,
                executeMethod: () => {
                    bladeNavigationService.closeBlade(blade);
                }
            }
        ];

        //ui-grid
        $scope.setGridOptions = (gridOptions) => {
            $scope.gridOptions = gridOptions;
            bladeUtils.initializePagination($scope);

            gridOptions.onRegisterApi = function (gridApi) {
                gridApi.grid.registerDataChangeCallback((grid) => {
                    grid.buildColumns();
                    _.each(gridApi.grid.options.columnDefs, column => {
                        column.cellTooltip = getCellTooltip;
                        column.headerCellClass = 'br-0 font-weight-500 fs-13';
                    });

                    if (blade.dataType.value === 'Contact') {
                        $scope.nameColumn = _.findWhere(gridApi.grid.options.columnDefs, {name: 'contactFullName'});
                        $scope.idColumn = _.findWhere(gridApi.grid.options.columnDefs, {name: 'contactId'});
                        const birthdayColumn = _.findWhere(gridApi.grid.options.columnDefs, { name: "birthday" });
                        if (birthdayColumn) {
                            birthdayColumn.cellTemplate = "birthday.col.html";
                        }
                    } else {
                        $scope.nameColumn = _.findWhere(gridApi.grid.options.columnDefs, {name: 'organizationName'});
                        $scope.idColumn = _.findWhere(gridApi.grid.options.columnDefs, {name: 'organizationId'});
                        const parentOrganizationName = _.findWhere(gridApi.grid.options.columnDefs, {name: 'parentOrganizationName'});
                        const parentOrganizationId = _.findWhere(gridApi.grid.options.columnDefs, {name: 'parentOrganizationId'});
                        const parentOrganizationOuterId = _.findWhere(gridApi.grid.options.columnDefs, {name: 'parentOrganizationOuterId'});
                        if (parentOrganizationName) {
                            parentOrganizationName.visible = false;
                        }
                        if (parentOrganizationId) {
                            parentOrganizationId.visible = false;
                        }
                        if (parentOrganizationOuterId) {
                            parentOrganizationOuterId.visible = false;
                        }
                    }

                    $scope.nameColumn.pinnedLeft = true;
                    $scope.nameColumn.cellClass = "pl-7 bl-0 font-weight-500 fs-12";
                    $scope.nameColumn.headerCellClass = "pl-7 font-weight-500 fs-13";
                    if ($scope.idColumn) {
                        $scope.idColumn.enablePinning = true;
                        $scope.idColumn.hidePinLeft = false;
                    }

                    grid.api.core.notifyDataChange(uiGridConstants.dataChange.COLUMN);
                },[uiGridConstants.dataChange.ROW])
            };
        };

        function getInvalidRowsCount() {
            $scope.previewCount = _.min([blade.totalCount, $scope.pageSettings.totalItems]);

            if (blade.currentEntities.length < $scope.previewCount) {
                $scope.unparsedRowsCount = $scope.previewCount - blade.currentEntities.length;
                $scope.showUnparsedRowsWarning = true;
            }
        }

        function truncateId(content) {
            if (content === null) return "";

            if (content.length > 9) {
                return content.substr(0, 3) + '...' + content.substr(content.length - 3, content.length);
            }

            return content;
        }

        function getCellTooltip(row, col) {
            const index = blade.currentEntities.indexOf(row.entity);
            return $scope.originalRecords[index][col.name];
        }

        initialize();

    }]);
