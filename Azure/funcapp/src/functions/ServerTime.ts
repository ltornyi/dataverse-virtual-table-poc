import { app, HttpRequest, HttpResponseInit, InvocationContext } from "@azure/functions";
import { formatDate } from "../common/utils";

export async function ServerTime(request: HttpRequest, context: InvocationContext): Promise<HttpResponseInit> {
    context.log(`ServerTime function processed request for url "${request.url}"`);

    const now = new Date();
    const formattedDate = formatDate(now);
    const response = {formattedDate: formattedDate, now: now};

    return { jsonBody: response};
};

app.http('ServerTime', {
    methods: ['GET'],
    authLevel: 'function',
    handler: ServerTime
});
