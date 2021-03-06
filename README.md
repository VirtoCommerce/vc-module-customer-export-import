# Customer Export & Import module

[![CI status](https://github.com/VirtoCommerce/vc-module-customer-export-import/workflows/Module%20CI/badge.svg?branch=dev)](https://github.com/VirtoCommerce/vc-module-customer-export-import/actions?query=workflow%3A"Module+CI")
[![Deployment status](https://github.com/VirtoCommerce/vc-module-customer-export-import/workflows/Module%20deployment/badge.svg?branch=dev)](https://github.com/VirtoCommerce/vc-module-customer-export-import/actions?query=workflow%3A"Module+deployment")
[![Quality gate](https://sonarcloud.io/api/project_badges/measure?project=VirtoCommerce_vc-module-customer-export-import&metric=alert_status)](https://sonarcloud.io/dashboard?id=VirtoCommerce_vc-module-customer-export-import)
[![Reliability rating](https://sonarcloud.io/api/project_badges/measure?project=VirtoCommerce_vc-module-customer-export-import&metric=reliability_rating)](https://sonarcloud.io/dashboard?id=VirtoCommerce_vc-module-customer-export-import)
[![Security rating](https://sonarcloud.io/api/project_badges/measure?project=VirtoCommerce_vc-module-customer-export-import&metric=security_rating)](https://sonarcloud.io/dashboard?id=VirtoCommerce_vc-module-customer-export-import)
[![Maintainability rating](https://sonarcloud.io/api/project_badges/measure?project=VirtoCommerce_vc-module-customer-export-import&metric=sqale_rating)](https://sonarcloud.io/dashboard?id=VirtoCommerce_vc-module-customer-export-import)

If you want to transfer a large amount of customer information between Virto Commerce and another system, then you can use a specially-formatted spreadsheet to import or export that data. Virto Commerce uses CSV (semicolon-separated value) files to perform this kind of bulk task.

The business goal for the module is to provide to non-technical not high skilled business users (like "customer manager") who works with customers on a daily basis and do not understand the database structure to work comfortably with customers export and import functionality using it for customers management. 


![Main-Screen](docs/media/main-screen.png)

### Note
If you want to automated transferring information from 3rd party system, like ERP, then see API, Integration Middleware approach and Azure Logic Apps connectors.

## Business scenarios
* I need to export contacts to edit and to update.
* I need to export organizations to edit and to update.
* I need to migrate contacts from another system.
* I need to migrate organizations from another system.
* I need to migrate accounts from another system.



## Documentation
* [Module Documentation](https://virtocommerce.com/docs/latest/modules/customer-export-import/index/)
* [View on GitHub](docs/index.md)

## Development
    Abstractions and implementation including public API can be changed in next releases (breaking changes may be introduced).

## References

* Deploy: https://virtocommerce.com/docs/latest/developer-guide/deploy-module-from-source-code/
* Installation: https://www.virtocommerce.com/docs/latest/user-guide/modules/
* Home: https://virtocommerce.com
* Community: https://www.virtocommerce.org
* [Download Latest Release](https://github.com/VirtoCommerce/vc-module-customer-export-import/releases/latest)

## License

Copyright (c) Virto Solutions LTD.  All rights reserved.

Licensed under the Virto Commerce Open Software License (the "License"); you
may not use this file except in compliance with the License. You may
obtain a copy of the License at

http://virtocommerce.com/opensourcelicense

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
implied.
