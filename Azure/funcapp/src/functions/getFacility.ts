import { app, HttpRequest, HttpResponseInit, InvocationContext, input} from "@azure/functions";
import { Facility } from "../common/Facility";
// import { isGuid } from "../common/utils";

const sqlInput = input.sql({
  commandText: `
    SELECT 
      facility_id, 
      client_id, 
      name, 
      facility_type, 
      address, 
      created_on, 
      modified_on 
    FROM poc.facility 
    WHERE facility_id = @facilityId`,
  commandType: 'Text',
  parameters: '@facilityId={id}',
  connectionStringSetting: 'SqlConnectionString',
});

export async function getFacility(request: HttpRequest, context: InvocationContext): Promise<HttpResponseInit> {
    context.log(`getFacility function processed request for url "${request.url}"`);

    // input binding handles parameter extraction and validation; invalid GUIDs result in 500 automatically
    // const id = request.params.id;
    // if (!isGuid(id)) {
    //     return { status: 400, jsonBody: { error: { code: "InvalidId", message: "Invalid facility_id format." }}} ;
    // }

    const dbResult = context.extraInputs.get(sqlInput) as Facility[];
    context.log(`dbResult: ${JSON.stringify(dbResult)}`);

    if (dbResult.length === 0) {
        return { status: 404, jsonBody: { error: { code: "FacilityNotFound", message: "Facility not found." }} };
    }

    return { status: 200, jsonBody: dbResult[0] };
};

app.http('getFacilityById', {
    methods: ['GET'],
    authLevel: 'function',
    route: 'facility/{id}',
    extraInputs: [sqlInput],
    handler: getFacility
});
