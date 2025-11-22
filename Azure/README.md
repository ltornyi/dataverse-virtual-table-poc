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

