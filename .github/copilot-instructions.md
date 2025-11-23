## Quick orientation

This repository demonstrates a Dataverse virtual table backed by an Azure SQL table and a small Azure Functions HTTP API used by the Dataverse data provider.

- Key locations
  - `Azure/funcapp/` — TypeScript Azure Functions app (source in `src/`, build output to `dist/`).
  - `Azure/db/Facility.sql` — SQL schema + sample data for the Facility table used by the demo.
  - `Power Platform/` — Dataverse provider and integration (see this folder's README for details).

## High-level architecture (what matters to code changes)

- The Function App exposes simple HTTP APIs that Dataverse queries through a virtual table provider. The Function App is protected by EntraID in production; locally it uses `local.settings.json` and a `SqlConnectionString` entry.
- Data flow: Dataverse -> Function HTTP API -> Azure SQL Database (queries executed with `input.sql` bindings).
- Functions are registered using the `app.http(...)` pattern from `@azure/functions` (see `ServerTime.ts` and `DbTime.ts`). Each function returns a plain `HttpResponseInit` with a `jsonBody` payload.

## Developer workflows and commands (project-specific)

- Install Azure Functions Core Tools (macOS brew example from repo):
  - `brew tap azure/functions && brew install azure-functions-core-tools@4`
- Build: `npm run build` (runs `tsc`).
- Run locally: `npm start` (this runs `func start` after a clean + build as configured in `package.json`).
- Incremental compile during development: `npm run watch` (runs `tsc -w`).
- Create a new function during development: use `func new --name <Name> --template "HTTP trigger" --authlevel "function"` then implement the handler with the `app.http` registration pattern.

## Project conventions & patterns for code edits

- File layout: source is in `Azure/funcapp/src/` and compiled JS is placed into `dist/` by `tsc` (see `tsconfig.json`). Keep relative imports consistent with this layout.
- Function registration: use `app.http('<FunctionName>', { methods: ['GET'|'POST'...], authLevel: 'function', handler: <fn> })`.
  - Example: `app.http('DbTime', { methods: ['GET'], authLevel: 'function', extraInputs: [sqlInput], handler: DbTime })` (`DbTime.ts`).
- Database access: prefer Azure Functions SQL input binding (`input.sql({...})`) as used in `DbTime.ts`. The binding is provided via `context.extraInputs.get(sqlInput)` and the connection setting name must be `SqlConnectionString` (set in `local.settings.json` for local runs and in Function App configuration in Azure).
- HTTP responses: return { jsonBody: <object> } (the repo uses `HttpResponseInit` with `jsonBody`).
- Logging: use `context.log(...)` for function logs.
- Utilities: keep shared helpers in `src/common` (e.g., `formatDate` in `Azure/funcapp/src/common/utils.ts`).

## Environment & secrets

- Local dev uses `local.settings.json` in `Azure/funcapp/`. Add `SqlConnectionString` to `Values` for DB access. README shows how to obtain an ADO.NET connection string with Entra ID authentication.
- Production secrets should be configured through Azure Function configuration (Application settings) — do not hard-code credentials.

## Integration points and external dependencies

- Azure SQL Database (see `Azure/db/Facility.sql`). The SQL server may require firewall rules and/or EntraID configuration — the repo README documents typical steps.
- Dataverse: the Power Platform folder contains the virtual table provider implementation which calls the Function App APIs.

## What to watch for when changing code

- Changing TypeScript types or compiler options affects the build output in `dist/`; update `tsconfig.json` only if you understand how it affects `npm run build` and `func start`.
- If you add new function bindings (e.g., new SQL inputs or other bindings), register them on the `app.http(...)` call and include the binding object in `extraInputs` so the function handler can read them via `context.extraInputs.get(...)`.
- Keep `authLevel: 'function'` unless you explicitly intend a different auth model. Dataverse and demo callers expect function-level auth.

## Useful files to reference while coding

- `Azure/funcapp/package.json` — build/start scripts (prestart cleans `dist` and builds).
- `Azure/funcapp/tsconfig.json` — compiler config (outDir `dist`, rootDir `.`).
- `Azure/funcapp/src/functions/DbTime.ts` — example SQL input binding + `context.extraInputs.get(...)` usage.
- `Azure/funcapp/src/functions/ServerTime.ts` — example simple HTTP handler returning server time.
- `Azure/db/Facility.sql` — schema and example data used by demo.

## When in doubt (guidance for the AI agent)

- Follow the existing `app.http` registration pattern and `context.extraInputs` usage for new functions.
- Prefer non-breaking changes. Add new function names and handlers without renaming existing ones unless refactoring uniformly (update references in Power Platform provider if function URLs or names change).
- Use `local.settings.json` for any local-only config and do not commit secrets.
