angular.module('virtoCommerce.customerExportImportModule')
.controller('virtoCommerce.customerExportImportModule.exportProcessingController', ['$scope', 'virtoCommerce.customerExportImportModule.export', 'platformWebApp.bladeNavigationService',
    function ($scope, exportResources, bladeNavigationService) {
        var blade = $scope.blade;
        blade.isLoading = false;

        $scope.$on("new-notification-event", function (event, notification) {
            if (blade.notification && notification.id === blade.notification.id) {
                angular.copy(notification, blade.notification);
            }
        });

        blade.toolbarCommands = [{
            name: 'platform.commands.cancel',
            icon: 'fa fa-times',
            canExecuteMethod: function() {
                return blade.notification && !blade.notification.finished;
            },
            executeMethod: function() {
                exportResources.cancel({ jobId: blade.notification.jobId });
            }
        }];

        $scope.bladeClose = () => {
            bladeNavigationService.closeBlade(blade);
        }

    }]);
