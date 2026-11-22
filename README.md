# Employee Management API

Clean Architecture Web API for employee records using .NET 10/C# 14, MediatR CQRS, EF Core 10, SQL Server, FluentValidation, AutoMapper, built-in OpenAPI, and Scalar.

> NuGet currently flags the requested AutoMapper 14.0.0 release with high-severity advisories. It is pinned to match the requested stack; review those advisories and upgrade to a fixed supported release before production deployment.

## Run

1. Install the .NET 10 SDK and SQL Server LocalDB (or change `ConnectionStrings:Default` in `src\EmployeeManagement.API\appsettings.json` to your SQL Server connection string).
2. The initial migration is included. From this solution folder, install/update EF tooling and apply it:

   ```powershell
   dotnet tool update --global dotnet-ef --version 10.0.15
   dotnet ef database update --project .\src\EmployeeManagement.Infrastructure\EmployeeManagement.Infrastructure.csproj --startup-project .\src\EmployeeManagement.API\EmployeeManagement.API.csproj
   ```

   To add a migration after changing the data model, run `dotnet ef migrations add AddYourMigration --project .\src\EmployeeManagement.Infrastructure\EmployeeManagement.Infrastructure.csproj --startup-project .\src\EmployeeManagement.API\EmployeeManagement.API.csproj --output-dir Persistence\Migrations`, then run `dotnet ef database update` with the same project arguments.

3. Run the API:

   ```powershell
   dotnet run --project .\src\EmployeeManagement.API
   ```

   Open `/swagger` for Swagger UI, `/swagger/v1/swagger.json` for its OpenAPI document, `/scalar/v1` for the Scalar API reference, `/openapi/v1.json` for the built-in OpenAPI document, and `/health` for health status.

## Test

```powershell
# All unit and integration tests
dotnet test .\EmployeeManagement.slnx

# Unit tests
dotnet test .\tests\EmployeeManagement.UnitTests\EmployeeManagement.UnitTests.csproj

# Integration tests
dotnet test .\tests\EmployeeManagement.IntegrationTests\EmployeeManagement.IntegrationTests.csproj

# Application-layer coverage and HTML report (install reportgenerator once)
dotnet tool install --global dotnet-reportgenerator-globaltool
dotnet test .\tests\EmployeeManagement.UnitTests\EmployeeManagement.UnitTests.csproj --collect:"XPlat Code Coverage" --results-directory .\TestResults
reportgenerator -reports:".\TestResults\**\coverage.cobertura.xml" -targetdir:".\CoverageReport" -reporttypes:Html -assemblyfilters:"+EmployeeManagement.Application"
```

Integration tests replace SQL Server with a dedicated in-memory SQLite connection per test. The tests do not require SQL Server.

## Endpoints

### `POST /api/employees`

Request:

```json
{
  "firstName": "Jordan",
  "lastName": "Lee",
  "email": "jordan.lee@example.com",
  "phoneNumber": "+1-555-0100",
  "department": "Engineering",
  "designation": "Software Engineer",
  "salary": 95000.00,
  "dateOfJoining": "2024-06-15"
}
```

Success: `201 Created`, `Location: /api/employees/{id}`, body: `{ "id": "00000000-0000-0000-0000-000000000001" }`.

### `GET /api/employees/{id}`

Success: `200 OK`, body:

```json
{
  "id": "00000000-0000-0000-0000-000000000001",
  "firstName": "Jordan",
  "lastName": "Lee",
  "email": "jordan.lee@example.com",
  "phoneNumber": "+1-555-0100",
  "department": "Engineering",
  "designation": "Software Engineer",
  "salary": 95000.0,
  "dateOfJoining": "2024-06-15",
  "isActive": true,
  "createdAt": "2024-06-15T00:00:00+00:00",
  "updatedAt": null
}
```

Unknown IDs return a ProblemDetails `404`.

### `GET /api/employees?pageNumber=1&pageSize=20&search=Jordan&department=Engineering`

Success: `200 OK`, with the same employee DTO fields in `items`:

```json
{
  "items": [
    {
      "id": "00000000-0000-0000-0000-000000000001",
      "firstName": "Jordan",
      "lastName": "Lee",
      "email": "jordan.lee@example.com",
      "phoneNumber": "+1-555-0100",
      "department": "Engineering",
      "designation": "Software Engineer",
      "salary": 95000.0,
      "dateOfJoining": "2024-06-15",
      "isActive": true,
      "createdAt": "2024-06-15T00:00:00+00:00",
      "updatedAt": null
    }
  ],
  "pageNumber": 1,
  "pageSize": 20,
  "totalCount": 1
}
```

Search matches first name, last name, or email. Only active employees are listed; page size is bounded from 1 to 100.

### `PUT /api/employees/{id}`

Request uses the same fields as create. Success: `204 No Content`; invalid input returns ProblemDetails `400`; unknown ID returns `404`.

### `DELETE /api/employees/{id}`

Success: `204 No Content` and the employee is soft-deleted (excluded from reads). Unknown IDs return `404`.

Validation errors return ProblemDetails with an `errors` object; duplicate email returns `409 Conflict`.
