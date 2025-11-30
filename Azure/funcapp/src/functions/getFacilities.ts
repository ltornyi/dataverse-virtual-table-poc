import { app, HttpRequest, HttpResponseInit, InvocationContext, input } from "@azure/functions";
import { Facility } from "../common/Facility";

const MAX_TOP = 1000;

const sqlInput = input.sql({
    commandText: `
        SELECT TOP(case when @top = '' then ${MAX_TOP} when try_convert(int, @top) is null then ${MAX_TOP} else try_convert(int, @top) end)
            facility_id,
            client_id,
            name,
            facility_type,
            address,
            created_on,
            modified_on
        FROM poc.facility
        WHERE (@clientId = '' or @clientId = ' ' or try_convert(uniqueidentifier, @clientId) is null OR client_id = try_convert(uniqueidentifier, @clientId))
          AND (@search = '' or @search = ' ' or name LIKE @search)
        ORDER BY
            CASE
                WHEN @orderbyField = 'name' AND @orderbyDir = 'asc' THEN name
                WHEN @orderbyField = 'facility_type' AND @orderbyDir = 'asc' THEN facility_type
                WHEN @orderbyField = 'address' AND @orderbyDir = 'asc' THEN address
                ELSE NULL
            END ASC,
            CASE
                WHEN @orderbyField = 'name' AND @orderbyDir = 'desc' THEN name
                WHEN @orderbyField = 'facility_type' AND @orderbyDir = 'desc' THEN facility_type
                WHEN @orderbyField = 'address' AND @orderbyDir = 'desc' THEN address
                ELSE NULL
            END DESC
    `,
    commandType: 'Text',
    parameters: '@clientId={Query.clientId},@search={Query.search},@top={Query.top},@orderbyField={Query.orderbyField},@orderbyDir={Query.orderbyDir}',
    connectionStringSetting: 'SqlConnectionString'
});

export async function getFacilities(request: HttpRequest, context: InvocationContext): Promise<HttpResponseInit> {
    context.log(`getFacilities function processed request for url "${request.url}"`);

    const dbResult = context.extraInputs.get(sqlInput) as Facility[] || [];

    //cannot mix types in SQL ORDER BY, so sorting by created_on and modified_on are handled here:
    const orderbyField = (request.query.get('orderbyField') || '').toString().toLowerCase();
    const orderbyDir = (request.query.get('orderbyDir') || '').toString().toLowerCase();
    if (orderbyField === 'created_on' || orderbyField === 'modified_on') {
        dbResult.sort((a, b) => {
            const dateA = new Date(a[orderbyField]);
            const dateB = new Date(b[orderbyField]);
            if (orderbyDir === 'asc') {
                return dateA.getTime() - dateB.getTime();
            } else {
                return dateB.getTime() - dateA.getTime();
            }
        });
    }

    return { status: 200, jsonBody: dbResult };
};

app.http('getFacilities', {
    methods: ['GET'],
    authLevel: 'function',
    route: 'facility',
    extraInputs: [sqlInput],
    handler: getFacilities
});
