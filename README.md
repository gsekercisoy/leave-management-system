**English** | [Türkçe](README_tr.md)

# Leave Management System

A portfolio full-stack starter: employees submit leave requests, managers approve/reject them, and approvals deduct from a yearly balance. Built with **.NET 8 Web API + EF Core** and **React 18 + Vite**.

## Features

- JWT auth with roles (`Admin`, `Manager`, `Employee`); password hashing (SHA256 + per-user salt, no Identity dependency)
- Employees: create requests, list own requests, view yearly balance
- Managers/Admins: list all requests, approve (deducts balance, rejects on insufficient balance) / reject
- xUnit unit tests for the approval/balance rules (EF Core InMemory)
- Docker Compose: API + SQL Server + web

## Tech Stack

| Layer   | Technology |
|---------|-----------|
| API     | .NET 8, ASP.NET Core, EF Core (SqlServer + InMemory), JWT Bearer, Swashbuckle |
| Tests   | xUnit, EF Core InMemory |
| Web     | React 18, react-router-dom, axios, Vite |
| Infra   | Docker, SQL Server 2022 |

## Quick Start

### API (requires .NET 8 SDK)

```bash
cd leave-api
# edit appsettings.json: connection string + Jwt:Key (placeholders, never commit secrets)
dotnet restore
dotnet run   # https://localhost:7010 (Swagger in Development)
```

Run tests:

```bash
dotnet test
```

### Web (requires Node 20+)

```bash
cd leave-web
npm install
npm run dev      # http://localhost:5173, /api proxied to https://localhost:7010
```

> The Vite proxy targets `https://localhost:7010` with `secure: false`. For `dotnet run` HTTPS, trust the dev cert once: `dotnet dev-certs https --trust` (Windows). Alternatively run the API on HTTP (`http://localhost:5010`) and point the proxy there.

### Docker

```bash
SA_PASSWORD=ChangeMe123! JWT_KEY=<long-random-secret-min-32-chars> docker compose up --build
# api: http://localhost:7010, web: http://localhost:8081
```

## API

| Method | Endpoint                    | Auth              | Notes |
|--------|-----------------------------|-------------------|-------|
| POST   | `/api/auth/register`        | —                 | `{username, password, role?}` → 201 |
| POST   | `/api/auth/login`           | —                 | `{username, password}` → `{token, username, role}` |
| GET    | `/api/leaves`               | Employee+         | Own requests |
| POST   | `/api/leaves`               | Employee+         | `{startDate, endDate, reason}` |
| GET    | `/api/leaves/all`           | Manager, Admin    | All requests |
| POST   | `/api/leaves/{id}/approve`  | Manager, Admin    | Deducts balance; 400 if insufficient |
| POST   | `/api/leaves/{id}/reject`   | Manager, Admin    | No balance change |
| GET    | `/api/balances/me`          | Employee+         | Current-year balance |

## Project Structure

```
leave-api/            .NET 8 Web API (Controllers, Models, Data, Services, Dtos)
leave-api.Tests/      xUnit unit tests (LeaveService rules, EF InMemory)
leave-web/            Vite + React 18 (Login/Register, Dashboard, Approvals)
docker-compose.yml    api + sqlserver + web
```
