# dataverse-virtual-table-poc

This project demonstrates how external data can be used in Dataverse through a virtual table. The virtual table can
participate in 1:N and N:M relationships.

## Schema and relationships

* Account:  standard Dataverse table
* Ticket: custom Dataverse table
* Facility: virtual Dataverse table

        Account -< Ticket (one-to-many)
        Account -< Facility (one-to-many)
        Ticket >-< Facility (many-to-many)

## Architecture

The facility underlying datastore is Azure SQL Database table. An Azure Function App serves APIs to query the table. The Function App is protected by EntraID. The custom Dataverse virtual table data provider implements the API calls for Dataverse. See the `Azure` and the `Power Platform` folders for details.