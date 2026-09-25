# ProjectOps Stage 1 Learning Guide

This guide explains the basic .NET solution created for ProjectOps.

## What is a .NET solution?

A solution is a container that organizes one or more related .NET projects.
The `ProjectOps.sln` file tells development tools which projects belong
together.

## What is a project?

A project is a part of an application. It contains source code, settings, and
references to packages. Each project has a `.csproj` file that describes how
it should be built.

This solution has two projects:


## Why do we have ProjectOps.Api?

`ProjectOps.Api` is the backend. It will eventually provide REST API endpoints
and communicate with a database. For now, it contains the basic ASP.NET Core
Web API setup, controller support, HTTPS support, Swagger documentation, and a
small in-memory Projects endpoint.
communicate with SQL Server. It contains the basic ASP.NET Core Web API setup, controller support, HTTPS support, Swagger documentation, and the Projects endpoint.

## Why do we have ProjectOps.Web?

`ProjectOps.Web` is the frontend. It is a Blazor Web App, so it can display
pages and provide the user interface in C# and Razor components. It currently
contains only the normal starter structure.

## What is Program.cs?

`Program.cs` is the startup file for a .NET application. It creates the
application, registers services, configures the request pipeline, maps routes,
and starts the application.

## What is appsettings.json?

`appsettings.json` stores application configuration as JSON. Examples include
logging settings and connection strings. This Stage 1 application does not
have a database connection yet.
`appsettings.json` stores application configuration as JSON. Examples include logging settings and connection strings. ProjectOps stores the SQL Server connection string there instead of putting it directly in C# code.

## What happens when I run dotnet build?

`dotnet build` compiles the C# code in the projects. It also checks references
and reports errors that must be fixed before the application can run.

## What is localhost?

`localhost` means this computer. When an application runs at a localhost URL,
it is available only from your local machine unless you configure it for other
access.

## How do I restore and build the solution?

Open a terminal in the `ProjectOps` directory and run:

```powershell
dotnet restore ProjectOps.sln
dotnet build ProjectOps.sln
```

`dotnet restore` downloads the NuGet packages required by the projects.

## How do I run the API?

From the `ProjectOps` directory, run:

```powershell
dotnet run --project ProjectOps.Api
```

The terminal displays the localhost URLs. Open the HTTPS URL in a browser.
Swagger UI is available at:

```text
https://localhost:<port>/swagger
```

Replace `<port>` with the HTTPS port shown in the terminal.

Press `Ctrl+C` to stop the API.

## How do I run the Blazor application?

From the `ProjectOps` directory, run:

```powershell
dotnet run --project ProjectOps.Web
```

The terminal displays the localhost URLs. Open the HTTPS URL in a browser to
view the Blazor application.

Press `Ctrl+C` to stop the Blazor application.

## What is a Controller?

A controller is a C# class that receives HTTP requests and returns HTTP
responses. `ProjectsController` handles requests related to projects.
A controller is a C# class that receives HTTP requests and returns HTTP responses. `ProjectsController` handles requests related to projects. It asks `AppDbContext` for project records and returns the result.

## What is an API endpoint?

An API endpoint is a URL that performs a specific job. The Projects endpoint is
`GET /api/projects`. It returns the sample projects as JSON.
An API endpoint is a URL that performs a specific job. The Projects endpoint is `GET /api/projects`. It returns project records as JSON.

## What does GET mean?

GET is an HTTP method used to request data. It does not create or change data.

## What is HttpClient?

`HttpClient` is a .NET class used to send HTTP requests and receive HTTP
responses from another application.

## Why does Blazor use HttpClient?

The Blazor Projects page uses `HttpClient` to ask the backend API for project
data. This keeps the user interface and the backend separate.

## What is JSON?

JSON is a text format commonly used to send structured data over HTTP. The API
converts the three C# project objects to JSON, and Blazor converts that JSON
into C# objects.

## What is CORS?

CORS, or Cross-Origin Resource Sharing, is a browser security rule. The API
allows requests from the Blazor application's localhost origins so the page
can call the API on its different port.

## How does the data travel from API to Blazor?

The flow is:

```text
Blazor Projects Page
	↓
HttpClient
	↓
GET http://localhost:5103/api/projects
	↓
ProjectsController
	↓
3 hard-coded projects
	↓
JSON response
	↓
Blazor
	↓
HTML table
```
Before, the data flow was:

```text
Blazor
	↓
ProjectsController
	↓
Hard-coded List<Project>
```

After, the data flow is:

```text
Blazor
	↓
GET /api/projects
	↓
ProjectsController
	↓
AppDbContext
	↓
Entity Framework Core
	↓
SQL Server
	↓
Projects table
	↓
EF Core converts database rows into Project objects
	↓
Controller returns JSON
	↓
Blazor displays projects
```

## Useful commands

These commands operate on the complete solution:

```powershell
dotnet --version
dotnet restore ProjectOps.sln
dotnet build ProjectOps.sln
dotnet run --project ProjectOps.Api
dotnet run --project ProjectOps.Web
```

## What is Entity Framework Core?

Entity Framework Core, often called EF Core, is a .NET tool that lets C# code work with a database. It maps C# classes to database tables and can translate C# queries into SQL queries.

## What is DbContext?

`DbContext` is the main EF Core class used to communicate with a database. `AppDbContext` represents the database connection and provides access to the tables used by this application.

## What is DbSet<Project>?

`DbSet<Project>` represents the `Projects` table. It lets the application query project rows and save project objects to SQL Server.

## What is a connection string?

A connection string tells the application how to connect to a database. This project connects to the local `SQLEXPRESS` SQL Server instance and uses the database name `ProjectOpsDb`.

## What is dependency injection?

Dependency injection is a way for .NET to create and provide objects that a class needs. `ProjectsController` receives `AppDbContext` in its constructor instead of creating the context itself.

## Why do we register AppDbContext in Program.cs?

Registering `AppDbContext` in `Program.cs` tells ASP.NET Core how to create it, including which connection string and SQL Server provider to use. ASP.NET Core then supplies it to the controller when needed.

## What is a migration?

A migration is a record of database structure changes. The `InitialCreate` migration describes how to create the `Projects` table for this application.

## What does dotnet ef migrations add InitialCreate do?

This command compares the EF Core model with the current migration history and creates a new migration named `InitialCreate`. It generates C# instructions for creating the database structure; it does not insert the sample data by itself.

```powershell
dotnet ef migrations add InitialCreate --project ProjectOps.Api
```

## What does dotnet ef database update do?

This command applies the migrations to SQL Server. It creates `ProjectOpsDb`, the `Projects` table, and EF Core's migration history table when they do not already exist.

```powershell
dotnet ef database update --project ProjectOps.Api
```

## What does ToListAsync() do?

`ToListAsync()` runs the database query asynchronously and turns the returned rows into a C# list of `Project` objects. `await` lets the application continue handling other work while SQL Server responds.

## Stage 3 files

Created:

- `ProjectOps.Api/Data/AppDbContext.cs`
- `ProjectOps.Api/Migrations/InitialCreate.cs` and related migration files

Modified:

- `ProjectOps.Api/ProjectOps.Api.csproj`
- `ProjectOps.Api/Program.cs`
- `ProjectOps.Api/appsettings.json`
- `ProjectOps.Api/Controllers/ProjectsController.cs`
- `LEARNING_GUIDE.md`

## What is POST?

POST is an HTTP method used to create new data. In this application, `POST
/api/projects` creates a new project and saves it in SQL Server.

GET retrieves or reads data. POST creates new data.

## What is a request body?

The request body is the data sent with an HTTP request. For this POST request,
the body is JSON containing `ProjectCode`, `ProjectName`, `ClientName`, and
`Status`. The client does not need to send `Id` because SQL Server generates it.

## How does JSON become a C# Project object?

ASP.NET Core model binding reads the JSON request body. It matches the JSON
property names to the properties on the C# `Project` class and creates a
`Project` object for the controller action.

## What does _context.Projects.Add(project) do?

`Add(project)` tells Entity Framework Core to track the new project as an
insert. It does not send the SQL INSERT to the database yet.

## What is the difference between Add() and SaveChangesAsync()?

`Add()` marks the object as a new database record in EF Core's change tracker.
`SaveChangesAsync()` sends the pending INSERT to SQL Server and waits for the
database to finish.

## Why is Id 0 before SaveChangesAsync()?

The `Id` property is an integer identity column. Before the insert, the new C#
object has the default integer value, which is `0`. The client does not set it.

During `SaveChangesAsync()`, SQL Server generates the next identity value and
EF Core copies that generated value back into `project.Id`. After the call,
the object contains its real database Id.

## What does HTTP 201 Created mean?

HTTP status `201 Created` means the server successfully created a new resource.
The response also contains the created `Project` object, including its new Id.

## Complete POST flow

```text
Swagger
	↓
JSON
	↓
HTTP POST
	↓
ProjectsController
	↓
Model Binding creates Project object
	↓
EF Core Add()
	↓
SaveChangesAsync()
	↓
SQL INSERT
	↓
ProjectOpsDb
	↓
Projects table
	↓
SQL Server generates Id
	↓
API returns HTTP 201 Created
```

## Stage 4 debugging breakpoints

Place breakpoints in `ProjectOps.Api/Controllers/ProjectsController.cs` at:

1. The first line inside `CreateProject(Project project)` to inspect the
	 object created from the Swagger JSON request.
2. `_dbContext.Projects.Add(project);` to observe EF Core tracking the object.
3. The line immediately before `await _dbContext.SaveChangesAsync();` to see
	 that `project.Id` is `0`.
4. The line immediately after `await _dbContext.SaveChangesAsync();` to see
	 the identity Id generated by SQL Server.

Use the Swagger request body without an `Id`:

```json
{
	"projectCode": "P004",
	"projectName": "Airport Expansion",
	"clientName": "Metro Infrastructure",
	"status": "Planning"
}
```

## Stage 7 Blazor CRUD files

Modified:

- `ProjectOps.Web/Components/Pages/Projects.razor`
- `LEARNING_GUIDE.md`

The existing `ProjectOps.Web.Models.Project` model, `HttpClient` registration,
and Projects navigation link were reused. No duplicate model or new API
architecture was added.

## What is HttpClient?

`HttpClient` is the .NET class the Blazor page uses to send HTTP requests to
the API and receive HTTP responses.

## What does GetFromJsonAsync() do?

`GetFromJsonAsync()` sends a GET request and converts the JSON response into a
C# object. The Projects page uses a GET request to load the project list.

## What does PostAsJsonAsync() do?

`PostAsJsonAsync()` converts a C# project into JSON and sends it in a POST
request. The API creates the project and saves it in SQL Server.

## What does PutAsJsonAsync() do?

`PutAsJsonAsync()` converts the edited C# project into JSON and sends it in a
PUT request to the project's URL. The API updates the existing database row.

## What does DeleteAsync() do?

`DeleteAsync()` sends a DELETE request to the project's URL. The API removes
that project from SQL Server, and the page reloads the list.

## Blazor CRUD architecture

The request flow is:

```text
Browser
	↓
Blazor Component
	↓
HttpClient
	↓
HTTP Request
	↓
ASP.NET Core Controller
	↓
EF Core
	↓
SQL Server
```

The response flow is:

```text
SQL Server
	↓
EF Core
	↓
Controller
	↓
JSON / HTTP Response
	↓
HttpClient
	↓
Blazor
	↓
User sees updated screen
```

## What is PUT?

PUT is an HTTP method used to update an existing resource. In this application,
`PUT /api/projects/{id}` changes the project identified by the URL.

GET means read, POST means create, and PUT means update.

## What is a route parameter?

A route parameter is a value included in the URL. In
`PUT /api/projects/{id}`, the `{id}` part is replaced with a real Id, such as
`PUT /api/projects/5`.

## What does FindAsync(id) do?

`FindAsync(id)` asks EF Core to find the project with that primary key. If the
project is found, EF Core returns the existing tracked C# object. If it is not
found, the action returns `NotFound()` and the API sends HTTP 404.

## What is change tracking?

Change tracking is how EF Core remembers the original values of an entity it
loaded. When values on the tracked object change, EF Core detects those
changes and knows which row needs to be updated.

## How does EF Core know which fields changed?

`FindAsync(id)` loads and tracks the existing project. Assigning new values to
its properties changes the tracked object. When `SaveChangesAsync()` runs, EF
Core compares the original and current values and creates the SQL UPDATE for
the changed row.

## What does SaveChangesAsync() do during an update?

`SaveChangesAsync()` sends the pending SQL UPDATE to SQL Server and waits for it
to finish. It updates the existing row; it does not create a new project.

## What does HTTP 204 No Content mean?

HTTP status `204 No Content` means the update succeeded and there is no response
body to return. The updated data can be read with `GET /api/projects`.

## Complete PUT flow

```text
Swagger
	↓
PUT /api/projects/5
	↓
id = 5
	↓
JSON request body
	↓
ProjectsController
	↓
FindAsync(5)
	↓
SQL Server finds existing row
	↓
Change C# object's values
	↓
EF Core detects changes
	↓
SaveChangesAsync()
	↓
SQL UPDATE
	↓
Existing database row updated
```

## Stage 5 modified files

- `ProjectOps.Api/Controllers/ProjectsController.cs`
- `LEARNING_GUIDE.md`
