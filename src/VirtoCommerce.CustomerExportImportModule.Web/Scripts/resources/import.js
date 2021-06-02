angular.module("virtoCommerce.customerExportImportModule")
    .factory("virtoCommerce.customerExportImportModule.import", ["$resource", function ($resource) {
        return $resource("api/customers/import", null, {
            validate: { method: "POST", url: "api/customers/import/validate" },
            preview: { method: "POST", url: "api/customers/import/preview" },
            run: {
                method: "POST",
                url: "api/customers/import/run",
                interceptor: {
                    response: async function (response) {
                        return await new Promise((resolve, reject) => {
                            setTimeout(() => {
                                const results = {
                                    id: 42,
                                    jobId: 542,
                                    description: "Import finished",
                                    finished: new Date(),
                                    createdLinesQty: 500,
                                    updatedLinesQty: 200,
                                    errorsQty: 100,
                                    reportUrl: "github.com/VirtoCommerce",
                                };
                                resolve(results);
                            }, 3000);
                        });
                    },
                },
            },
        });
    },
]);
