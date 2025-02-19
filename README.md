# Virto Commerce Customer Export & Import Module

[![CI status](https://github.com/VirtoCommerce/vc-module-customer-export-import/workflows/Module%20CI/badge.svg?branch=dev)](https://github.com/VirtoCommerce/vc-module-customer-export-import/actions?query=workflow%3A"Module+CI")
[![Quality gate](https://sonarcloud.io/api/project_badges/measure?project=VirtoCommerce_vc-module-customer-export-import&metric=alert_status)](https://sonarcloud.io/dashboard?id=VirtoCommerce_vc-module-customer-export-import)
[![Reliability rating](https://sonarcloud.io/api/project_badges/measure?project=VirtoCommerce_vc-module-customer-export-import&metric=reliability_rating)](https://sonarcloud.io/dashboard?id=VirtoCommerce_vc-module-customer-export-import)
[![Security rating](https://sonarcloud.io/api/project_badges/measure?project=VirtoCommerce_vc-module-customer-export-import&metric=security_rating)](https://sonarcloud.io/dashboard?id=VirtoCommerce_vc-module-customer-export-import)
[![Maintainability rating](https://sonarcloud.io/api/project_badges/measure?project=VirtoCommerce_vc-module-customer-export-import&metric=sqale_rating)](https://sonarcloud.io/dashboard?id=VirtoCommerce_vc-module-customer-export-import)

If you want to transfer a large amount of customer information between Virto Commerce and another system, then you can use a specially-formatted spreadsheet to import or export that data. Virto Commerce uses CSV (semicolon-separated value) files to perform this kind of bulk task.

The business goal for the module is to provide to non-technical not high skilled business users (like "customer manager") who works with customers on a daily basis and do not understand the database structure to work comfortably with customers export and import functionality using it for customers management. 


![Main-Screen](docs/media/main-screen.png)

> **Note:**  
> To automate the transfer of information from third-party systems like ERP, refer to the API, Integration Middleware approach, and Azure Logic Apps connectors.

## Key features  

* Export and update contacts  
* Export and update organizations  
* Migrate contacts, organizations, and accounts from another system

## Documentation
* [Customer Export and Import module user documentation](https://docs.virtocommerce.org/platform/user-guide/customer-export-import/overview/)
* [REST API](https://virtostart-demo-admin.govirto.com/docs/index.html?urls.primaryName=VirtoCommerce.CustomerExportImport)
* [View on GitHub](https://github.com/VirtoCommerce/vc-module-customer-export-import/)

## Development
Abstractions and implementation including public API can be changed in next releases (breaking changes may be introduced).

## References

* [Deployment](https://docs.virtocommerce.org/platform/developer-guide/Tutorials-and-How-tos/Tutorials/deploy-module-from-source-code/)
* [Installation](https://docs.virtocommerce.org/platform/user-guide/modules-installation/)
* [Home](https://virtocommerce.com)
* [Community](https://www.virtocommerce.org)
* [Download latest release](https://github.com/VirtoCommerce/vc-module-customer-export-import/releases/latest)

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
