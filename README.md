# MusicStore

A C# console application for managing a music record store. MusicStore brings together a record catalog, inventory tracking, sales, customers, reservations, and promotions in a menu-driven interface backed by Microsoft SQL Server.

This is a **study project** for practicing C#, Entity Framework Core, relational database design, and role-based application workflows.

## Features

- **Record catalog:** add, edit, list, and archive records; search by title, artist, or genre.
- **Inventory:** receive stock, write off records, and review stock movements while accounting for reserved quantities.
- **Sales:** sell available or reserved records, record sale items, and track customer spending.
- **Customers and reservations:** register customers, reserve records, cancel reservations, and support partial fulfillment and expiration.
- **Discounts:** create date-limited genre promotions and apply loyalty discounts based on customer spending. A sale uses the higher of the promotion and loyalty discounts.
- **Recommendations:** browse the newest releases and popular records, artists, and genres for the current day, week, month, or year.
- **Reference data:** manage artists, genres, and publishers.
- **Authentication:** masked password entry, salted PBKDF2-SHA256 password hashes, and role-based access to store operations.

### Roles

| Operation | Seller | Manager | Administrator |
| --- | :---: | :---: | :---: |
| Browse records, recommendations, sales, and stock history | Yes | Yes | Yes |
| Add customers, manage reservations, and complete sales | Yes | Yes | Yes |
| Add/edit records, receive/write off stock, and create promotions | No | Yes | Yes |
| Add/rename artists, genres, and publishers | No | Yes | Yes |
| Archive records and delete reference entries | No | No | Yes |

## Technology stack

- C# and .NET 10 (`net10.0`)
- Entity Framework Core with the SQL Server provider and code-first migrations
- Microsoft SQL Server
- JSON configuration through `Microsoft.Extensions.Configuration`

## Getting started

### Prerequisites

- .NET 10 SDK
- A running SQL Server instance
- A SQL client such as SQL Server Management Studio for account provisioning and optional demo data
- An interactive terminal for the console interface

Run the commands below from the directory containing `MusicStore.csproj` and this README. If you open the solution directory instead, enter its `MusicStore` subdirectory first.

### 1. Complete the project configuration

The current `MusicStore.csproj` does not declare the packages used by the source code. A build of the supplied project fails with missing Entity Framework Core and related namespaces. Add the required dependencies before building:

```powershell
dotnet add MusicStore.csproj package Microsoft.EntityFrameworkCore.SqlServer --version "10.*"
dotnet add MusicStore.csproj package Microsoft.Extensions.Configuration.Json --version "10.*"
```

The application reads `appsettings.json` from the executable directory. Add the following inside the `<Project>` element of `MusicStore.csproj` so configuration is included when building and publishing:

```xml
<ItemGroup>
  <None Update="appsettings.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    <CopyToPublishDirectory>PreserveNewest</CopyToPublishDirectory>
  </None>
</ItemGroup>
```

### 2. Configure SQL Server

Edit [appsettings.json](appsettings.json) to match your SQL Server instance. The supplied configuration uses the local default instance, the `MusicStore` database, and Windows authentication:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MusicStore;TrustServerCertificate=True;Trusted_Connection=True;"
  }
}
```

For a named instance, update `Server` accordingly, for example `localhost\\SQLEXPRESS` in JSON. The connecting account needs permission to apply schema migrations and, if the database does not exist, create it. The supplied certificate-trust setting is intended for local development.

### 3. Build and initialize the database

```powershell
dotnet restore MusicStore.csproj
dotnet build MusicStore.csproj
dotnet run --project MusicStore.csproj
```

At startup, `DatabaseInitializer` applies pending migrations automatically, upgrades legacy plaintext password records to hashes, and checks the database connection. On success, the application displays the sign-in screen.

A newly migrated database has no user accounts. Stop the application with `Ctrl+C`, then load the supplied database initialization file as described below.

### 4. Initialize the database with sample data

Open [InitDatabase.sql](InitDatabase.sql) in SQL Server Management Studio or another SQL client connected to your configured server, then execute the script. Run it after the first successful application startup has applied the schema migrations. It populates the database with sample records, customers, sales, reservations, promotions, and the existing demo accounts `admin`, `manager`, and `seller`; no manual account creation is needed.

> **Destructive operation:** this script deletes all existing application data, including users, before inserting its demo data. Use it only with a disposable development database. It targets `MusicStore` through a `USE` statement.

After the script completes, restart the application:

```powershell
dotnet run --project MusicStore.csproj
```

Sign in with one of the accounts created by the initialization script:

| Username | Password | Role |
| --- | --- | --- |
| `admin` | `admin123` | Administrator |
| `manager` | `manager123` | Manager |
| `seller` | `seller123` | Seller |

These public demo credentials are provided for this study project's local sample database. Passwords are stored as salted hashes in the initialization script.

## Using the application

After signing in, choose a numbered option from the main menu:

```text
1. Records menu
2. Record store
3. View recommendations
4. Customers menu
5. Sign out
6. Reference data          (Administrator and Manager only)
0. Exit application
```

For a fresh database, start by adding artists, genres, and publishers under **Reference data**, then add records and customers. Use **Record store** to manage stock and sales, and **Customers menu** to manage reservations and promotions. Available actions depend on your role.

## Project structure

```text
MusicStore/
|-- Data/               Database context, mappings, and startup initialization
|-- Helpers/            Password hashing and verification
|-- Menus/              Console screens and input handling
|-- Migrations/         EF Core schema migrations and model snapshot
|-- Models/             Catalog, customer, inventory, sales, and user entities
|-- Services/           Store operations, authentication, and authorization
|-- InitDatabase.sql    Destructive demo-data reset script
|-- appsettings.json    SQL Server connection configuration
|-- MusicStore.csproj   .NET console project
`-- Program.cs          Application entry point and service wiring
```

The project's root namespace is `MusicStore`, with `MusicStore.Data`, `MusicStore.Helpers`, `MusicStore.Menus`, `MusicStore.Migrations`, `MusicStore.Models`, and `MusicStore.Services` organizing the source. Records are represented by the `Plates` model. Archiving preserves historical relationships, while database constraints, transactions, and row-version checks protect inventory and sales updates.

## Troubleshooting

| Problem | What to check |
| --- | --- |
| Missing `Microsoft.EntityFrameworkCore` or related namespaces | Complete the package setup above, then restore and rebuild. |
| `appsettings.json` cannot be found | Ensure the copy settings above are present and rebuild; the file must be beside the executable. |
| Database connection or migration error | Check the SQL Server instance, connection string, authentication, and database permissions. |
| Sign-in always fails | Run `InitDatabase.sql` after schema migration, then use one of the demo accounts documented above. The script resets existing application data. |
| An operation is unavailable | Check the signed-in account's role against the permissions table. |
| A stock operation fails despite a positive quantity | Active reservations reduce available stock. Concurrent updates can also require retrying the operation. |

There is no automated test project in the supplied source. Database initialization and interactive workflows require a configured SQL Server instance for manual verification.
