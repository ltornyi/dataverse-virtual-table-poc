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

## Steps to follow if implementing from scratch

### Azure

This is detailed in the `Azure/README.md` file.

1. Create `poc` schema and `facility` table in Azure SQL Database
2. Set up function app local development environment
3. Set up remote access for your database, adjust firewall
4. Check the function app code, run and test locally
5. Deploy function app to Azure, test remote calls using the function keys
6. Configure managed identity for the function app, set it up as an external user in the DB
7. Test all remote calls end-to-end using function keys

### Power Platform

This is detailed in the `Power Platform/README.md` file. There was a failed attemp to build the virtual data provider plug-in on a Mac - the main blocker is that Dataverse plugins require .NET Framework 4.6.2 which is Windows only. This attempt is documented in the `Power Platform/README_plugin_build_attempt_on_Mac.md` file.

to be continued...