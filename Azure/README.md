# Azure implementation

## Azure SQL Database

### Table and data

See `db/Facility.sql` for the script to set up the DB schema, the facility table and to add sample data.

## Function app for APIs

### Set up local developer environment

    brew tap azure/functions
    brew install azure-functions-core-tools@4
    mkdir funcapp
    cd funcapp
    func init --worker-runtime typescript --model V4

Add a starter function

    func new --name ServerTime --template "HTTP trigger" --authlevel "function"

Implement the logic in the function body and adjust the methods and authentication of the function registration call.

### Run locally

    npm start

The list of functions and local endpoints will be printed; point your browser to

    http://localhost:7071/api/ServerTime

You can also enable incremental compilation by running `npm run watch` in a new terminal window.

## Integrate function app with Azure SQL Database

### Use EntraID authentication locally

Create an externally identified user in the database if needed; the `<identity-name>` should be your EntraID username:

    CREATE USER [<identity-name>] FROM EXTERNAL PROVIDER;
    ALTER ROLE db_datareader ADD MEMBER [<identity-name>];
    ALTER ROLE db_datawriter ADD MEMBER [<identity-name>];

Go to the Azure SQL database in the portal, settings -> connection strings and copy the value displayed under
"ADO.NET (Microsoft Entra passwordless authentication)". It will look like this:

    Server=tcp:your_Azure_SQL_server.database.windows.net,1433;Initial Catalog=your_Azure_SQL_database;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Authentication=\"Active Directory Default\";

Add this setting to your `local.settings.json` as a new attribute of the `Value` object; the attribute name should be `SqlConnectionString`. At this point your local.settings.json will look like this:

    {
        "IsEncrypted": false,
        "Values": {
            "FUNCTIONS_WORKER_RUNTIME": "node",
            "AzureWebJobsStorage": "UseDevelopmentStorage=true",
            "SqlConnectionString":"Server=tcp:your_Azure_SQL_server.database.windows.net,1433;Initial Catalog=your_Azure_SQL_database;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Authentication=\"Active Directory Default\";"
        }
    }

### Adjust the Azure SQL Database server firewall settings if needed

Go to the SQL server (not the database) resource in the portal, check "Public access" under security -> networking. Enable the "Selected networks" option and add your client IP or create a firewall rule as needed.

### Add a function to query DB time

    func new --name DbTime --template "HTTP trigger" --authlevel "function"

Edit the implementation and leverage an sql input referencing the `SqlConnectionString` setting. See `DbTime.ts` for details.

### Support Retrieve and RetrieveMultiple operations

The `getFacility` and `getFacilities` function implement querying by facilityid and querying multiple records, respectively.
For the single record operation, the `id` part of the route is mandatory and needs to be a GUID - otherwise HTTP 500 is generated.
For the multiple record operation, currently all the query parameters must be present in the request. It seems to be a limitation
of the `@sqlparam={Query.myParameter}` syntax which results in an HTTP 500 if myParameter is not present.