# Bookstore API

## Docker

```powershell
docker compose up --build
```

Swagger: [http://localhost:5277/swagger](http://localhost:5277/swagger)

Stop the containers with:

```powershell
docker compose down
```

## Local

Requires .NET 8 and SQL Server LocalDB.

```powershell
dotnet run --project src/Bookstore.Api
```
