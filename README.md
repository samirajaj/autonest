# AutoNest

AutoNest is a full-stack vehicle marketplace and fleet-management platform. It provides a public catalog for discovering vehicles, customer workflows for favorites and purchase or rental requests, company tools for managing inventory, and an administrative area for platform operations.

The application uses role-based authorization throughout the React client and ASP.NET Core API.

## Features

### Public

- Responsive vehicle marketplace with search and filters
- Vehicle details and company listings
- Dark and light themes with saved system-aware preferences
- Customer registration, email confirmation, login, and password recovery
- Responsive navigation for desktop and mobile

### Customer

- Save and remove favorite vehicles
- Submit purchase or rental requests
- Review request and transaction history
- Rate completed transactions
- Manage username, email, password, and account details

### Company

- View operational dashboard metrics
- Create, update, soft-delete, and restore vehicle listings
- Upload vehicle images
- Review, approve, or reject customer requests
- Manage company credentials

### Administrator

- View platform dashboard metrics
- Manage customer and company accounts
- Lock accounts and reset passwords
- Manage subscription plans and company assignments
- Configure loyalty point ranges
- Access the protected Hangfire dashboard

## Technology stack

| Area            | Technologies                                                   |
| --------------- | -------------------------------------------------------------- |
| Client          | React 19, TypeScript, Vite, React Router, TanStack Query       |
| UI              | CSS design system, Lucide icons, Recharts                      |
| API             | ASP.NET Core Web API, .NET 9, JWT authentication, Swagger      |
| Business        | Role-based services, DTOs, validation and domain rules         |
| Data            | Entity Framework Core, ASP.NET Core Identity, SQL Server       |
| Background jobs | Hangfire with SQL Server storage                               |
| Email           | MailKit SMTP                                                   |
| Testing         | xUnit, ASP.NET Core integration tests, Vitest, Testing Library |

## Repository structure

```text
autonest/
|-- client/                         React application
|   |-- public/                     Brand and vehicle assets
|   `-- src/
|       |-- app/                    Routing, authentication and theme state
|       |-- features/               Customer, company and admin pages
|       |-- shared/                 API client, reusable components and types
|       `-- test/                   Frontend tests
|-- server/
|   |-- src/
|   |   |-- AutoNest.Api/          HTTP endpoints and infrastructure
|   |   |-- AutoNest.Business/     Contracts, services and domain rules
|   |   `-- AutoNest.Data/         EF Core entities, repositories and migrations
|   `-- tests/
|       |-- AutoNest.Api.Tests/     API integration tests
|       `-- AutoNest.Business.Tests/
|-- .gitignore
`-- README.md
```

## Prerequisites

Install the following tools before running the project:

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) for the application
- .NET 10 SDK/runtime for the test projects in this checkout
- [Node.js 20 or newer](https://nodejs.org/)
- SQL Server or SQL Server LocalDB
- Git

## Configuration and secrets

Committed `appsettings*.json` files contain no credentials. Use [.NET user-secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) for local development:

```powershell
cd server/src/AutoNest.Api

dotnet user-secrets init

dotnet user-secrets set "ConnectionStrings:AutoNest" "Server=(localdb)\MSSQLLocalDB;Database=AutoNest;Trusted_Connection=True;TrustServerCertificate=True"
dotnet user-secrets set "Jwt:Key" "replace-with-a-random-signing-key-of-at-least-32-characters"

dotnet user-secrets set "Admin:Email" "admin@example.com"
dotnet user-secrets set "Admin:UserName" "autonest-admin"
dotnet user-secrets set "Admin:Password" "replace-with-a-strong-admin-password"

dotnet user-secrets set "Seed:DefaultPassword" "replace-with-a-strong-demo-account-password"
```

SMTP is optional. When `Smtp:Host` is empty, email delivery is suppressed and logged.

```powershell
dotnet user-secrets set "Smtp:Host" "smtp.example.com"
dotnet user-secrets set "Smtp:Port" "587"
dotnet user-secrets set "Smtp:UserName" "smtp-user"
dotnet user-secrets set "Smtp:Password" "smtp-password"
dotnet user-secrets set "Smtp:From" "noreply@example.com"
```

For hosted environments, use environment variables with double underscores:

```text
ConnectionStrings__AutoNest
Jwt__Key
Admin__Email
Admin__UserName
Admin__Password
Seed__DefaultPassword
Smtp__Host
Smtp__Port
Smtp__UserName
Smtp__Password
Smtp__From
```

Never commit user-secrets, environment files, production connection strings, signing keys, or email credentials.

## Environment-specific URLs

ASP.NET Core automatically loads the appropriate environment configuration:

| Environment        | Client URL                            |
| ------------------ | ------------------------------------- |
| Development        | `http://localhost:5173/`              |
| Production/default | `https://autonest-website.vercel.app` |

`ClientBaseUrl` is used to generate email confirmation and password-reset links. Override it through `ClientBaseUrl` or the `ClientBaseUrl` environment variable for another deployment.

The React client sends requests to the relative `/api` path. During development, Vite proxies those requests to `https://localhost:7038`.

## Local development

### 1. Clone the repository

```powershell
git clone YOUR_REPOSITORY_URL
cd autonest
```

### 2. Configure backend secrets

Follow the user-secrets commands in the [Configuration and secrets](#configuration-and-secrets) section.

### 3. Start the API

```powershell
dotnet restore server/AutoNest.sln
dotnet run --project server/src/AutoNest.Api --launch-profile https
```

Development endpoints:

- API: `https://localhost:7038`
- Swagger UI: `https://localhost:7038/swagger`
- Hangfire: `https://localhost:7038/hangfire`

The first non-test startup applies EF Core migrations, creates roles and lookup data, creates the configured administrator, and adds demo marketplace data when the database is empty.

### 4. Start the React client

Open a second terminal:

```powershell
cd client
npm install
npm run dev
```

Open `http://localhost:5173/`.

If the local HTTPS development certificate is not trusted, run:

```powershell
dotnet dev-certs https --trust
```

## Database

The EF Core context and migrations are in `server/src/AutoNest.Data`. The API applies pending migrations automatically outside the test environment.

To create a new migration:

```powershell
dotnet ef migrations add MigrationName --project server/src/AutoNest.Data --startup-project server/src/AutoNest.Api
```

To apply migrations manually:

```powershell
dotnet ef database update --project server/src/AutoNest.Data --startup-project server/src/AutoNest.Api
```

Vehicle uploads are stored in SQL Server. The shared placeholder is a static API asset at `/api/assets/placeholder.png`, so seed data does not duplicate placeholder image blobs.

## API overview

| Resource            | Purpose                                                 |
| ------------------- | ------------------------------------------------------- |
| `/api/auth`         | Registration, login, confirmation and password recovery |
| `/api/cars`         | Public catalog, details and vehicle images              |
| `/api/companies`    | Public company listings                                 |
| `/api/cities`       | Registration lookup data                                |
| `/api/favorites`    | Customer favorites                                      |
| `/api/requests`     | Customer purchase and rental requests                   |
| `/api/transactions` | Customer transactions and ratings                       |
| `/api/profile`      | Customer account management                             |
| `/api/company`      | Company dashboard, vehicles and request management      |
| `/api/admin`        | Administrative accounts, plans and point ranges         |
| `/api/assets`       | Shared static API assets                                |

Protected endpoints require an `Authorization: Bearer <token>` header. Authorization is enforced by the API in addition to the client route guards.

## Background jobs

Hangfire schedules the following recurring operations:

- Removal of vehicles soft-deleted for more than 30 days
- Expiration and release of stale requests
- Subscription-plan expiration and notifications
- Rental return reminders

The Hangfire dashboard requires an authenticated administrator.

## Testing and quality checks

Run backend verification:

```powershell
dotnet build server/AutoNest.sln
dotnet test server/AutoNest.sln
```

Run frontend verification:

```powershell
cd client
npm test
npm run lint
npm run build
```

## Production notes

- Provide all secrets through the hosting platform's secret manager or environment variables.
- Configure SQL Server with encryption and least-privilege credentials.
- Route the frontend's `/api` requests to the deployed ASP.NET Core API.
- Set `ClientBaseUrl` to the deployed frontend origin.
- Restrict CORS to trusted frontend origins before production use.
- Configure persistent ASP.NET Core Data Protection keys when running multiple API instances.
- Use a production SMTP provider if confirmation and recovery emails are required.
- Run database backups and monitor Hangfire jobs.

## Security

If a credential was ever committed or pushed, removing it from `appsettings.json` is not sufficient. Rotate the credential and, when necessary, remove it from Git history using an appropriate history-rewriting tool.

## Contributing

1. Create a focused branch.
2. Keep business rules in the Business project and persistence concerns in Data.
3. Add or update tests with each behavioral change.
4. Run all backend and frontend checks before opening a pull request.
5. Describe configuration or migration changes clearly in the pull request.
