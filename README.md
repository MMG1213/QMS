# QMS

Fujitec Quality Management System: an Angular frontend and an ASP.NET Core Web API backend.

## Repository layout

```
QMS/
├── QMS.slnx                  # .NET solution (open in Visual Studio / Rider)
├── src/
│   ├── backend/
│   │   └── Qms.Api/          # ASP.NET Core Web API (.NET 10, EF Core, SQL Server)
│   │       ├── Controllers/
│   │       ├── Data/         # QmsDbContext
│   │       ├── DTOs/
│   │       ├── Migrations/
│   │       ├── Models/
│   │       └── Services/
│   └── frontend/             # Angular 22 app
│       ├── angular.json
│       ├── package.json
│       ├── proxy.conf.json   # /api -> http://localhost:5193
│       ├── public/
│       └── src/app/          # components, pages, services, guards, interceptors
└── README.md
```

## Running locally

**Backend** (http://localhost:5193). The connection string is in
`src/backend/Qms.Api/appsettings.json`.

```bash
cd src/backend/Qms.Api
dotnet run --launch-profile http
```

**Frontend** (http://localhost:4200). Calls to `/api` are proxied to the backend.

```bash
cd src/frontend
npm install
npm start
```

## Build and test

```bash
dotnet build QMS.slnx                       # backend
cd src/frontend && npm run build && npm test   # frontend
```
