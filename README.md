# StockManagement - Clean Architecture (.NET 8)

This repository contains a clean-architecture skeleton for the Stock Management system described in the spec.

Projects:
- src/StockManagement.Api - ASP.NET Core Web API (Swagger)
- src/StockManagement.Application - Application services and DTOs
- src/StockManagement.Domain - Domain entities and enums
- src/StockManagement.Infrastructure - EF Core InMemory persistence and repositories

Run (requires .NET 8 SDK):

```
cd src/StockManagement.Api
dotnet restore
dotnet run
```

The API will run on the configured Kestrel port and expose Swagger at `/swagger`.

Database migrations and switching providers
-----------------------------------------
You can run the app with different persistence providers by editing `src/StockManagement.Api/appsettings.json` or setting env vars. `Persistence:Provider` accepts `InMemory`, `SqlServer`, or `Postgres`.

To create EF Core migrations (example using SQL Server):

```
dotnet tool install --global dotnet-ef
cd src/StockManagement.Api
dotnet ef migrations add InitialCreate -p ../StockManagement.Infrastructure -s .
dotnet ef database update -p ../StockManagement.Infrastructure -s .
```

For Postgres change the connection string and run the same `dotnet ef` commands.

If you only want to run locally without a DB, keep `Persistence:Provider` = `InMemory`.


Mock JWT / local token helper
--------------------------------
This project includes a development-friendly mock JWT handler that decodes the JWT payload without validating signatures. The mock handler expects the token payload to contain any of the following fields:

- `name`: display/username
- `sub`: subject / user id
- `role`: single role string (e.g. `Inputter`) or
- `roles`: array of role strings (e.g. `["Inputter","Reconciler"]`)

When using the mock handler (default), the server accepts unsigned tokens and extracts roles from the token payload. To switch to strict validation (useful when integrating your auth microservice), set the environment variable `USE_MOCK_JWT=false` and provide `JWT_SECRET`, `JWT_ISSUER`, and `JWT_AUDIENCE`.

Quick way to create an unsigned token locally (bash/macOS):

```
# Example payload (change name/role as needed)
payload='{"name":"user1","sub":"user1","role":"Inputter"}'
header='{"alg":"none","typ":"JWT"}'

base64url() { openssl base64 -e | tr -d '=' | tr '/+' '_-' | tr -d '\n'; }

echo -n "$header" | base64url
echo -n "."$(echo -n "$payload" | base64url)

# Quick one-liner to produce full token
token=$(printf '%s' "$header" | base64url).$(printf '%s' "$payload" | base64url).
echo $token
```

Or use the provided helper script in `tools/generate_jwt.sh`:

```
chmod +x tools/generate_jwt.sh
./tools/generate_jwt.sh '{"name":"recon1","sub":"recon1","role":"Reconciler"}'
```

