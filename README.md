# Invoice Management API

A small REST API built with **ASP.NET Core 9 (.NET 9)** for managing shipments and the invoices raised for them.
It follows the same business flow as shipping and finance billing systems: book a shipment, track its status, create an invoice with charge lines and tax, then issue it and mark it paid.

## Tech stack

- C#, ASP.NET Core 9 Minimal APIs
- Entity Framework Core 9 with SQLite (easy to switch to SQL Server)
- Built-in dependency injection
- Custom middleware (request timing and logging)
- OpenAPI document (`/openapi/v1.json`)

## Run it

1. Install the [.NET 9 SDK](https://dotnet.microsoft.com/download) (or newer) 
2. In this folder run:

```bash
dotnet restore
dotnet run
```

3. The API starts on `http://localhost:5080`. The database file `invoices.db` is created automatically on first run.
4. Open `requests.http` in Visual Studio or VS Code (REST Client extension) and send the requests in order,
   or use Postman / curl.

## Endpoints

| Method | Route | What it does |
|---|---|---|
| POST | `/api/shipments` | Create a shipment (status starts as `Booked`) |
| GET | `/api/shipments?status=` | List shipments, optional status filter |
| GET | `/api/shipments/{id}` | Get one shipment |
| PUT | `/api/shipments/{id}/status` | Move status forward: Booked, InTransit, Delivered |
| POST | `/api/invoices` | Create a draft invoice for a shipment with lines and tax rate |
| GET | `/api/invoices?status=&shipmentId=` | List invoices with filters |
| GET | `/api/invoices/{id}` | Get one invoice with lines and totals |
| POST | `/api/invoices/{id}/issue` | Draft to Issued |
| POST | `/api/invoices/{id}/pay` | Issued to Paid |

Status codes: `201` created, `200` ok, `400` validation errors, `404` not found, `409` invalid status change.

## Project structure

```
Program.cs                 App setup: services, middleware pipeline, endpoints
Models/Entities.cs         Shipment, Invoice, InvoiceLine and status enums
Data/AppDbContext.cs       EF Core DbContext and table configuration
Dtos/                      Request and response records, and mapping methods
Services/                  Business logic (interfaces and implementations)
Endpoints/                 Minimal API route groups and request validation
Middleware/                RequestTimingMiddleware
```

## How it works (for interview answers)

- **Request flow:** request, then middleware (timing), then routing to an endpoint, then the service (business rules), then the DbContext (EF Core and SQLite), then the response DTO as JSON.
- **Dependency injection:** services are registered in `Program.cs` with `AddScoped`. The endpoints receive `IInvoiceService`, and the service receives `AppDbContext`, so each layer depends on an abstraction and is easy to test.
- **DbContext:** `AppDbContext` maps the entities to tables, sets unique indexes on shipment and invoice numbers, and sets decimal precision. Relationships: one shipment has many invoices, and one invoice has many lines.
- **Invoice calculation:** subtotal is the sum of `quantity x unitPrice` for the lines, tax is `subtotal x taxRate / 100` rounded to 2 decimals, and total is subtotal plus tax. These are calculated properties, not stored columns.
- **Business rules:** shipment status only moves forward, and an invoice must go Draft, then Issued, then Paid. Breaking a rule returns `409 Conflict`.
- **Validation:** done in the endpoint layer, and returns a standard `400` validation problem response.
- **Middleware:** `RequestTimingMiddleware` logs each request and adds an `X-Elapsed-Ms` response header.

## Possible improvements (good to mention as next steps)

- EF Core migrations instead of `EnsureCreated()`
- Switch to SQL Server (change `UseSqlite` to `UseSqlServer` and the connection string)
- Unit tests for `InvoiceService` and integration tests with `WebApplicationFactory`
- JWT authentication, pagination, Swagger UI or Scalar, Docker
- A safer invoice-number generator for concurrent requests (the current one is simple for demo use)

## Push to GitHub

```bash
git init
git add .
git commit -m "Initial commit: Invoice Management API"
git branch -M main
git remote add origin https://github.com/<your-username>/InvoiceManagementApi.git
git push -u origin main
```
