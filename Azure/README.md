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

### Deploy to Azure

#### Create resource group, storage account and function app

    LOCATION="your region"
    RG_NAME="your resource group"
    SA_NAME="your storage account name"
    APP_NAME="your function app name"

    az group create --name $RG_NAME --location $LOCATION

    az storage account create --name $SA_NAME \
    --location $LOCATION \
    --resource-group $RG_NAME \
    --sku Standard_LRS \
    --allow-blob-public-access false

    az functionapp create \
    --resource-group $RG_NAME \
    --consumption-plan-location $LOCATION \
    --runtime node --runtime-version 20 \
    --functions-version 4 \
    --name $APP_NAME \
    --storage-account $SA_NAME \
    --os-type Linux

#### Build and deploy the function app

    npm run build
    func azure functionapp publish $APP_NAME

#### Call a function

Note the URLs printed by the deployment; for authLevel=function endpoints you will need to pass a function key i.e. `https://<your function app>.azurewebsites.net/api/<your function route>?code=<your function key>`. Example CLI command to list function key for the ServerTime function:

    az functionapp function keys list \
        --function-name ServerTime \
        --name $APP_NAME \
        --resource-group $RG_NAME

Try calling it with curl:

    curl -v "https://<your function app>.azurewebsites.net/api/ServerTime?code=<your function key>"

This should work but if you try calling the dbtime function, it will fail with an HTTP 500 because the database connection is not set up yet.

### Connect the function app to Azure SQL with managed identity

This is based on [MS Documentation](https://learn.microsoft.com/en-gb/azure/azure-functions/functions-identity-access-azure-sql-with-managed-identity).

#### Check if your database server has an Entra admin

    SQL_SERVER_RG="resource group of your SQL server"
    SQL_SERVER_NAME="name of your SQL server"
    az sql server ad-admin list \
        --resource-group $SQL_SERVER_RG \
        --server-name $SQL_SERVER_NAME

If there's no Micrososft Entra administrator, follow the steps [here](https://learn.microsoft.com/en-us/azure/azure-sql/database/authentication-aad-configure?view=azuresql&tabs=azure-portal#provision-azure-ad-admin-sql-database) to add one.

#### Add managed identity to your function app

Either enable system-assigned managed identity or add a user-assigned managed identity. You can do this from the portal (settings -> identity) or you can use the CLI; see instructions [here](https://learn.microsoft.com/en-gb/azure/app-service/overview-managed-identity?tabs=cli%2Cdotnet&toc=%2Fazure%2Fazure-functions%2Ftoc.json#add-a-system-assigned-identity). The name of the system-assigned identity is always the name of the function app.

#### Create database user for the managed identity

    CREATE USER [<identity-name>] FROM EXTERNAL PROVIDER;
    ALTER ROLE db_datareader ADD MEMBER [<identity-name>];
    ALTER ROLE db_datawriter ADD MEMBER [<identity-name>];

run the following to get the list of external users:

    SELECT * FROM sys.database_principals where type='E'

#### Set the connection string for the deployed function app

On the portal, navigate to the function app and look into settings -> environment variables. Create a new one called `SqlConnectionString` under App settings.

If you used the system-assigned managed identity, the connection string should look like:

    Server=tcp:your_Azure_SQL_server.database.windows.net,1433;Initial Catalog=your_Azure_SQL_database;Persist Security Info=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Authentication="Active Directory Managed Identity";

If you used a user-assigned managed identity, the connection string should look like:

    Server=tcp:your_Azure_SQL_server.database.windows.net,1433;Initial Catalog=your_Azure_SQL_database;Persist Security Info=False;User ID=ClientIdOfManagedIdentity;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Authentication="Active Directory Managed Identity";

Once you have this set up, you can try calling the dbtime endpoint again using the proper function key; this time it should work! Finally, check the query endpoints, use the propert keys for the functions; example:

    curl -v "https://your_function_app.azurewebsites.net/api/facility?code=code_for_the_facility_function&clientId=&search=&top=&orderbyField=&orderbyDir="

### Secure the APIs using Microsoft Entra

This is an optional but very useful enhancement. So far external clients (for example the virtual data provider plugin in Dataverse) can call the function endpoints but they need the key for each function. Rotating and managing the keys becomes a concerted effort that both the function app maintainers and API consumers need to coordinate.

Entra authentication can solve this problem because API consumers no longer need to manage secrets like the function key. This option is also called "EasyAuth" for App Service and Function apps. Follow the [quickstart](https://learn.microsoft.com/en-us/azure/app-service/scenario-secure-app-authentication-app-service?tabs=workforce-configuration). Look at the [page here](https://learn.microsoft.com/en-us/azure/app-service/configure-authentication-provider-aad?tabs=workforce-configuration) for more details.

to be continued...

### Secure the APIs to be accessible from virtual networks only

Another idea to lock down access to the function app is to make it available only to selected virtual networks (VNets) and leverage Power Platform's [Virtual network support](https://learn.microsoft.com/en-us/power-platform/admin/vnet-support-overview) feature to call the function endpoints from a delegated Azure subnet.

One caveat to look out for is that once a Power Platform environment is configured for Virtual Network support, all Dataverse plug-ins and connectors execute requests in the delegated subnet. Review network policies and existing plug-ins before enabling Virtual Network support.

Detailed steps to set up Virtual Network support for your environment are [here](https://learn.microsoft.com/en-us/power-platform/admin/vnet-support-setup-configure). Make sure you have the Microsoft.PowerPlatform provider registered in your Azure subscription.

## Check Power Platform region and Azure regions

You will need to create two VNets and a subnet in each according to the list of [Supported regions](https://learn.microsoft.com/en-us/power-platform/admin/vnet-support-overview#supported-regions).

If your Power Platform components need public internet access, then you will need to deploy an Azure NAT Gateway as well.

## Lock down function app

Select your function app in the Azure portal, click on Settings->Networking and change the setting `Public network access`. Change the default to `deny` and allow traffic from the two VNet's subnets.

## Delegate subnets to Power Platform

You can run the PowerShell script as part of the [guide](https://learn.microsoft.com/en-us/power-platform/admin/vnet-support-setup-configure) or you can manually delegate each subnet on the Azure portal, instructions are [here](https://learn.microsoft.com/en-us/azure/virtual-network/manage-subnet-delegation?tabs=manage-subnet-delegation-portal#delegate-a-subnet-to-an-azure-service). Delegate the subnets to `Microsoft.PowerPlatform/enterprisePolicies`.

## Create policy

Follow the [guide](https://learn.microsoft.com/en-us/power-platform/admin/vnet-support-setup-configure#create-the-enterprise-policy).

## Assign power platform environment to policy

Follow the [guide](/subscriptions/fb08dd31-84d8-43a7-b576-5a66a8c9148a/resourceGroups/funcapp-api-for-virtual-table/providers/Microsoft.Network/virtualNetworks/vnet-northeurope). The environment must be a managed environment.

You will need [this script](https://github.com/microsoft/PowerPlatform-EnterprisePolicies/blob/main/Source/SubnetInjection/RevertSubnetInjection.ps1) if you later want to remove the policy from the environment. You can use [this script](https://github.com/microsoft/PowerPlatform-EnterprisePolicies/blob/main/Source/SubnetInjection/GetSubnetInjectionEnterprisePolicyForEnvironment.ps1) to get the enterprise policy's arm id.

to be continued...