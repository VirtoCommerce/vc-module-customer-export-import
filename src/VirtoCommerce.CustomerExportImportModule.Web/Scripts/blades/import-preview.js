angular.module('virtoCommerce.customerExportImportModule')
    .controller('virtoCommerce.customerExportImportModule.importPreviewController', ['$scope', 'virtoCommerce.customerExportImportModule.import', 'platformWebApp.bladeNavigationService', 'uiGridConstants', 'platformWebApp.bladeUtils', 'platformWebApp.dialogService', function ($scope, importResources, bladeNavigationService, uiGridConstants, bladeUtils, dialogService) {
        $scope.uiGridConstants = uiGridConstants;

        var blade = $scope.blade;

        blade.importPermission = "import:access";

        function initialize() {
            blade.isLoading = true;
            $scope.showUnparsedRowsWarning = false;

            importResources.preview({ filePath: blade.csvFilePath }, (response) => {
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
                        organizationName: blade.organizationName,
                        callback: (confirm) => {
                            if (confirm) {
                                // const importDataRequest = {
                                //     filePath: blade.csvFilePath,
                                //     organizationId: blade.organizationId
                                // };

                                // importResources.run(importDataRequest, (data) => {
                                //     var newBlade = {
                                //         id: "customerImportProcessing",
                                //         notification: data,
                                //         headIcon: "fa fa-download",
                                //         title: "customerExportImport.blades.import-processing.title",
                                //         controller: "virtoCommerce.customerExportImportModule.importProcessingController",
                                //         template: "Modules/$(VirtoCommerce.CustomerExportImport)/Scripts/blades/import-processing.tpl.html",
                                //     };

                                //     bladeNavigationService.showBlade(newBlade, blade);
                                // });

                                //temporary solution for mock
                                var newBlade = {
                                    id: "customerImportProcessing",
                                    headIcon: "fa fa-download",
                                    title: "customerExportImport.blades.import-processing.title",
                                    controller: "virtoCommerce.customerExportImportModule.importProcessingController",
                                    template: "Modules/$(VirtoCommerce.CustomerExportImport)/Scripts/blades/import-processing.tpl.html"
                                };

                                bladeNavigationService.showBlade(newBlade, blade);
                            }
                        }
                    };
                    dialogService.showDialog(dialog, "Modules/$(VirtoCommerce.CustomerExportImport)/Scripts/dialogs/customerImport-dialog.tpl.html", "platformWebApp.confirmDialogController");
                },
                permission: blade.importPermission
            },
            {
                name: "priceExportImport.blades.import-preview.upload-new",
                icon: 'fa fa-download',
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
                    const fullNameColumn = _.findWhere(gridApi.grid.options.columnDefs, {name: 'contactFullName'});
                    const idColumn = _.findWhere(gridApi.grid.options.columnDefs, {name: 'contactId'});
                    fullNameColumn.pinnedLeft = true;
                    fullNameColumn.cellClass = 'pl-7 bl-0 font-weight-500 fs-12';
                    fullNameColumn.headerCellClass = 'pl-7 font-weight-500 fs-13';
                    idColumn.enablePinning = true;
                    idColumn.hidePinLeft = false;
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
