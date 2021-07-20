# Overview

If you want to transfer a large amount of customer information between Virto Commerce and another system, then you can use a specially-formatted spreadsheet to import or export that data. Virto Commerce uses CSV (semicolon-separated value) files to perform this kind of bulk task.

The business goal for the module is to provide to non-technical not high skilled business users (like customer manager) who works with customers on a daily basis and do not understand the database structure to work comfortably with customers export and import functionality using it for customers management.

![Screenshot_4](docs\media\Screenshot4.png)

> Note If you want to automated transferring information from 3rd party system, like ERP, then see API, Integration Middleware approach and Azure Logic Apps connectors.

## Business scenarios

- I need to create a new organization with customers.
- I need to make bulk changes for multiple customers in the organization of few hundreds of records.
- I need to make a bulk update (change user group according to customer segment) for a users in organization.
- I need to add new organization for a batch of customers added to the Customer module.

## Using CSV

Get a sample CSV file.
You can export and view a sample contacts or organizations CSV file and use it as a template.

## Contact CSV file format for import

The first line of CSV file should be Header:

| --- | --- |
| Contact First Name |  \*required |
| Contact Last Name |   \*required |
| Contact Full Name |   \*required |
| Contact Id |   |
| Contact Outer Id |   |
| Organization Id |   |
| Organization Outer Id |   |
| Organization Name |   |
| Email | |
| Account Login |  \*required for account |
| Store Id |  \*required for account |
| Store Name |   |
| Account Email |  \*required for account |
| Account Type |   |
| Account Status |   |
| Email Verified |   |
| Contact Status |   |
| Associated Organization Id |   |
| Birthday |   |
| TimeZone |   |
| Phones |   |
| Salutation |   |
| Default language |   |
| Taxpayer ID |   |
| Preferred communication |   |
| Preferred delivery |   |
| Address Type |  \*required for address |
| Address First Name |   |
| Address Last Name |   |
| Address Country |   |
| Address Region |   |
| Address City |   |
| Address Line1 |   |
| Address Line2 |   |
| Address Zip Code |   |
| Address Email |   |
| Address Phone |   |
| All Dynamic Properties | |


Each column must be separated by a semicolon. Only Contact First Name, Contact Last Name, Contact Full Name values are required for Contact creation.

All Address values are required for creation/updating address. If you don't need to create/update address leave it empty.

You can create new account in relation to contact. Account name, Account email, Store id values are required for Account creating. Please notice, you can only create account, but not update.

Example: [..\Downloads\Contacts\_example.csv](/C:%5CUsers%5C79787%5CDownloads%5CContacts_example.csv)

**Organisations CSV file format for import**

The first line should be Header:

| --- | --- |
| Organization Name |  required |
| Organization Id |   |
| Organization Outer Id |   |
| Address Type |  Required for address |
| Address First Name |   |
| Address Last Name |   |
| Address Country |   |
| Address Region |   |
| Address City |   |
| Address Address Line1 |   |
| Address Address Line2 |   |
| Address Zip Code |   |
| Address Email |   |
| Address Phone |   |
| Phones |   |
| Business category |   |
| Description |   |
| All Dynamic Properties | |

Each column must be separated by a semicolon. Only Organisation name value is required for creation organisation.

All Address values are required for creation/updating address. If you don't need to create/update address leave it empty.


Example: [../Downloads/Organizations\_sample.csv](/C:%5CUsers%5C79787%5CDownloads%5COrganizations_sample.csv)

## Export Contacts and Organizations

### Export selected organizations

1. The user opens root of Contact Module >
2. Select few organizations > click export
3. The system opens the > Simple export dialog screen with the text "NUM contacts NUM organizations will be exported to csv file."
4. User confirms export
5. The system opens the processing screen where the links appear when the processing is finished
6. Links to download organizations.csv file and contacts.csv file are displayed.

### Export all from Customer module

1. The user opens root of Contact Module >
2. Click export
3. The system opens the Simple export dialog screen with the text "NUM contacts NUM organizations will be exported to csv file."
4. User confirms export
5. The system opens the processing screen where the links appear when the processing is finished
6. Links to download organizations.csv file and contacts.csv file are displayed

## Contacts import

### Create new contacts and new organizations

1. The user opens root of Contact Module > click import
2. Upload file >
3. The system shows progress in the Upload CSV blade
4. The user opens uploaded file to preview > Click Import
5. The system shows a popup with the text: "NUM contacts will be added according to linked organizations"
6. The system creates new organizations if their OuterID not exist in the system

### Create contacts into organization

1. The user opens Contact Module > Open Subcategory Sturbacks > click Import
2. Upload file >
3. The system shows progress in the Upload CSV blade
4. The user opens uploaded file to preview > Click Import
5. The system shows a popup with the text: "NUM contacts will be added into Sturbacks organization"
6. The system creates new contacts into the Sturbacks organization

### Create accounts throught contact.csv import

1. The user opens root of Contact Module > click import
2. Upload file >
3. The system shows progress in the Upload CSV blade
4. The user opens uploaded file to preview > Click Import
5. The system shows a popup with text: "NUM contacts will be imported"
6. The system creates accounts in relation to contact in the system

### Update existing contacts

1. The user opens root of Customer management module > click import
2. Upload file >
3. The system shows progress in the Upload CSV blade
4. The user opens uploaded file to preview > Click Import
5. The system shows a popup with text: "NUM contacts will be imported"
6. The system finds contacts and organization in the system by Internal Id or Outer Id and updates them

## Import Organizations

### Create new organizations

1. The user opens root of Customer management module > click import
2. Upload file >
3. The system shows progress in the Upload CSV blade
4. The user opens uploaded file to preview > Click Import
5. The system shows a popup with the text: "NN organizations will be imported"
6. The system creates a new organization1 and places it in the root.

> It is important to know that organizations can be imported into root only

### Update existing organizations

1. The user opens root of Customer management module > click import
2. Browse and upload file >
3. The system shows progress in the Upload CSV blade
4. The user opens to preview uploaded file > Click Import
5. The system shows a popup with the text: "NN organizations will be imported"
6. The system finds Organizations in the system by Internal Id or Outer Id and updates them. Organization places into the Root

## Advanced settings

Limit for number of lines to export = 10.000 by default Ask system administrator to change it throught an environment variable for

<CustomerExportImport__Export__LimitOfLines >

Limit for number of lines to import = 10.000 by default Ask system administrator to change it throught an environment variable for

<CustomerExportImport__Import__LimitOfLines >

Limit for size of csv file = 1mb by default Ask system administrator to change it throught an environment variable for

<CustomerExportImport__Import__FileMaxSize