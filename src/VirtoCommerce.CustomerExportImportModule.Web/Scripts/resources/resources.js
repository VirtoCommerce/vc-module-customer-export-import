angular.module('virtoCommerce.customerExportImportModule')
    .factory('virtoCommerce.customerExportImportModule.import', ['$resource', function ($resource) {
        return $resource('api/customers/import', null,
            {
                validate: { method: 'POST', url: 'api/customers/import/validate' }
            });

    }])
