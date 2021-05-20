angular.module('virtoCommerce.customerExportImportModule')
    .factory('virtoCommerce.customerExportImportModule.export', ['$resource', function ($resource) {
        return $resource('api/customer/export', null,
            {
                run: { method: 'POST', url: 'api/customer/export/run'},
                cancel: { method: 'POST', url: 'api/customer/export/cancel'}
            });

    }])
