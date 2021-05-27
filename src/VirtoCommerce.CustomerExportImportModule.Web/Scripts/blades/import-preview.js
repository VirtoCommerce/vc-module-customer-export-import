angular.module('virtoCommerce.customerExportImportModule')
    .controller('virtoCommerce.customerExportImportModule.importPreviewController', ['$scope', 'virtoCommerce.customerExportImportModule.import', '$filter', 'platformWebApp.bladeNavigationService', 'uiGridConstants', 'platformWebApp.uiGridHelper', 'platformWebApp.bladeUtils', 'platformWebApp.dialogService', '$translate', 'platformWebApp.settings', function ($scope, importResources, $filter, bladeNavigationService, uiGridConstants, uiGridHelper, bladeUtils, dialogService, $translate, settings) {
        $scope.uiGridConstants = uiGridConstants;

        var blade = $scope.blade;

        blade.importPermission = "import:access";

        function initialize() {
            blade.isLoading = true;
            $scope.showUnparsedRowsWarning = false;

            importResources.preview({ filePath: blade.csvFilePath }, (data) => {
                _.each(data.results, record => {
                    _.each(record.dynamicProperties, prop => {
                        _.extend(record, { [prop.name]: prop.values.map(value => value.value).join(', ') });
                    });
                    _.omit(record, 'dynamicProperties');
                });
                blade.currentEntities = data.results;
                blade.totalCount = data.totalCount;
                $scope.pageSettings.totalItems = 10;
                getInvalidRowsCount();
                blade.isLoading = false;
            }, (error) => { bladeNavigationService.setError('Error ' + error.status, blade); });
        }

        blade.toolbarCommands = [
            {
                name: "platform.commands.import",
                icon: 'fa fa-download',
                canExecuteMethod: () => true ,
                executeMethod: () => {},
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
                        column.cellTooltip = true;
                        column.headerCellClass = 'br-0 font-weight-500 fs-13';
                    });
                    const fullNameColumn = _.findWhere(gridApi.grid.options.columnDefs, {name: 'fullName'});
                    const idColumn = _.findWhere(gridApi.grid.options.columnDefs, {name: 'id'});
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

        initialize();

    }]);
