angular.module("virtoCommerce.customerExportImportModule")
    .factory("virtoCommerce.customerExportImportModule.import", ["$resource", function ($resource) {
        return $resource("api/customer/import", null, {
            validate: { method: "POST", url: "api/customer/import/validate" },
            preview: { method: "POST", url: "api/customer/import/preview" },
            run: { method: "POST", url: "api/customer/import/run" },
            cancel: { method: "POST", url: "api/customer/import/cancel" },
        });
    },
]);
