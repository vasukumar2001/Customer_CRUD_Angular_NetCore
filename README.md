# Customer CRUD Application (Angular 20 & .NET 10.0 Web API)

This project is a comprehensive showcase of a **Customer CRUD (Create, Read, Update, Delete)** application. It demonstrates a single **Angular 20** frontend working seamlessly against **three different ASP.NET Core 10 Web API backend architectures** (ADO.NET, Dapper, and Entity Framework Core). 

Additionally, the project includes **SQL Reporting Queries (Part B)** for relational data analysis.

---

## 📂 Repository Directory Structure

```text
Customer_CRUD_Angular_NetCore/
├── customer-app/                     # Angular 20 Frontend Application
│   ├── src/
│   │   ├── app/
│   │   │   ├── components/           # UI Components (List, Form)
│   │   │   ├── models/               # Customer TypeScript Interface
│   │   │   └── services/             # Http client service to consume APIs
│   │   └── styles.css                # Global styles
│   └── package.json                  # Dependencies (Angular 20, Bootstrap 5)
│
├── CustomerApi_ADONet/               # Backend Web API (ADO.NET & Stored Procedures)
│   └── CustomerApp/
│       └── CustomerApp/              # Controllers, Models, Repositories, Program.cs
│
├── CustomerApi_Dapper/               # Backend Web API (Dapper & Stored Procedures)
│   └── CustomerApp.Api/
│       └── CustomerApp.Api/          # Controllers, Models, Repositories, Program.cs
│
├── CustomerApi_EFCore/               # Backend Web API (EF Core, Unit of Work, Repository Pattern)
│   └── CustomerApp.Api/
│       └── CustomerApp.Api/          # Data context, UnitOfWork, Repositories, Controllers
│
└── ClaudMDFile/                      # Documentation guides & Database Queries
    ├── 1_AdoNet_API.md               # ADO.NET Backend Guide
    ├── 2_Dapper_API.md               # Dapper Backend Guide
    ├── 3_EntityFramework_API.md      # EF Core Backend Guide
    ├── 4_Angular_Frontend.md         # Angular Setup Guide
    ├── Part B SQL Query.md           # Part B SQL Query report answers
    └── Net core & Angular Practical .pdf
```

---

## 📝 Project Descriptions

### 💻 [customer-app](file:///D:/git/Customer_CRUD_Angular_NetCore/customer-app)
An **Angular 20** Single Page Application (SPA) utilizing modern standalone components and styled with **Bootstrap 5**. It provides a fully functional user interface for displaying, creating, updating, and deleting customer records with reactive form validations. The client application connects to the backend through a unified service at [customer.service.ts](file:///D:/git/Customer_CRUD_Angular_NetCore/customer-app/src/app/services/customer.service.ts), enabling easy switching between backend architectures by changing the base API endpoint URL.

### ⚡ [CustomerApi_ADONet](file:///D:/git/Customer_CRUD_Angular_NetCore/CustomerApi_ADONet)
An **ASP.NET Core 10.0 Web API** designed using the repository pattern with raw ADO.NET database access. It uses `Microsoft.Data.SqlClient` to establish direct connections to Microsoft SQL Server and execute database stored procedures for each CRUD operation. This project represents the highest performance approach, providing complete control over database commands, transactions, and object mapping via `SqlDataReader`.

### 🚀 [CustomerApi_Dapper](file:///D:/git/Customer_CRUD_Angular_NetCore/CustomerApi_Dapper)
An **ASP.NET Core 10.0 Web API** utilizing the **Dapper** micro-ORM. It combines the speed of stored procedures with automated mapping of query results to strongly typed C# models. Dapper simplifies the repository implementation by eliminating raw ADO.NET parameter mapping and boilerplate command execution logic.

### 🛠️ [CustomerApi_EFCore](file:///D:/git/Customer_CRUD_Angular_NetCore/CustomerApi_EFCore)
An **ASP.NET Core 10.0 Web API** built with **Entity Framework Core 10 (EF Core)**. It implements the Repository and Unit of Work patterns to group operations and write changes within a single transactional context. EF Core abstracts the database completely, automatically generating database tables from C# entities and managing state tracking without using database-side stored procedures.