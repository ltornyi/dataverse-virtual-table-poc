import { app, HttpRequest, HttpResponseInit, InvocationContext, input } from "@azure/functions";
import { formatDate } from "../common/utils";

const sqlInput = input.sql({
  commandText: 'select SYSUTCDATETIME() as DbTime',
  commandType: 'Text',
  connectionStringSetting: 'SqlConnectionString',
});

export async function DbTime(request: HttpRequest, context: InvocationContext): Promise<HttpResponseInit> {
    context.log(`DbTime function processed request for url "${request.url}"`);

    const dbResult = context.extraInputs.get(sqlInput) as any[];
    context.log(`dbResult: ${JSON.stringify(dbResult)}`);

    const dbNow = new Date(dbResult[0]?.DbTime);
    const formattedDate = formatDate(dbNow);
    const response = {formattedDate: formattedDate, dbNow: dbNow};

    return {jsonBody: response};
};

app.http('DbTime', {
    methods: ['GET'],
    authLevel: 'function',
    extraInputs: [sqlInput],
    handler: DbTime
});
