# Copilot Instructions for MISA.QLSX

## Big picture

- Workspace has a Vue 3 FE in `MISA.QLSX.FE/` and a .NET 8 BE in `MISA.QLSX.BE/`.
- FE is feature-based: pages in `src/views/*`, shared UI in `src/components/*`, shared paging/search flow in `src/composables/usePagingTable.js`.
- BE follows 3 layers: `MISA.QLSX.Api` (controllers), `MISA.QLSX.Core` (entities/services/contracts), `MISA.QLSX.Infrastructure` (Dapper/MySQL repositories).
- Generic base classes are the main extension points: `BaseController<T>`, `BaseServices<T>`, and `BaseRepository<T>`.

## FE conventions

- Use `BaseAPI` for all HTTP clients; concrete APIs only set `this.controller` and inherit `getAll`, `paging`, `create`, `update`, `delete`.
- Use `usePagingTable()` for list pages; avoid duplicating pagination, search debounce, and reload state.
- `MsTable.vue` is the canonical reusable table: paging, sorting, filtering, row selection, pinned columns, and the column-customization drawer already exist there.
- Prefer custom controls in `src/components/*`; use Ant Design Vue only when a custom control is missing or not worth adding.
- Router is centralized in `src/router/index.js`; feature routes map directly to `/shifts`, `/employees`, `/contract-templates`, and `/contracts`.
- FE payloads and component data stay camelCase end-to-end.

## BE conventions

- Controllers inherit from `BaseController<T>` and expose CRUD plus `paging`, `query-all`, and `batch-delete`.
- Repositories inherit from `BaseRepository<T>`; override `GetReadTableName()` when reads should come from a view.
- Example: `EmployeeRepository` reads from `vw_employee_detail` but still writes to the base table.
- Keep `FieldMap` and `GetSearchFields()` whitelists in sync with supported filter/search fields; never build SQL from arbitrary request keys.
- `Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true` is enabled in `Program.cs`, so snake_case DB columns map to PascalCase C# properties.
- `ValidateExceptionMiddleware` turns domain validation errors into API responses; business rules belong in services, not controllers.

## Data/model patterns

- FE column definitions drive table rendering and user column settings; see `src/views/shift/Index.vue` for the canonical table config pattern.
- BE entities use PascalCase properties with `[Column("snake_case_column")]` mapping.
- Keep read models and write models aligned with the SQL view or table used by the repository.
- `ShiftRepository` and `EmployeeRepository` are good examples of repository-specific search/filter mappings.

## Workflows

- FE: `npm install`, `npm run dev`, `npm run build`, `npm run lint`, `npm run format`.
- BE: `dotnet build MISA.QLSX.BE.sln`, `dotnet run --project MISA.QLSX.Api/MISA.QLSX.Api.csproj`.
- MCP MySQL server: `cd docs/mcp && npm install && npm start` (for direct DB access, schema inspection, migrations).
- When changing UI, validate layout and comments in the same pass; prefer small, targeted edits over broad reformatting.

## Integration points

- `Program.cs` wires CORS, DI, Swagger, Dapper underscore mapping, exception middleware, and EPPlus license setup.
- `BaseAPI` wraps Axios and expects controllers to follow the base route conventions.
- `MsTable.vue` is used broadly across feature pages, so keep its props, emitted events, and persisted column settings backward compatible.
- **MCP MySQL Server** (`docs/mcp/index.js`) provides AI agents with direct DB access for queries, schema inspection, and migrations — connection reads from `MISA.QLSX.Api/appsettings.json` or `docs/mcp/.env`.

## Guidance for AI agents

- Read the feature page, its API, and the matching BE controller/repository before changing behavior.
- Preserve existing naming and comment style; add comments only where UI or logic is non-obvious.
- For database operations (schema inspection, migrations, data verification), use the MCP MySQL server tools instead of calling the API directly.
- Prefer the smallest fix that matches the current architecture.
