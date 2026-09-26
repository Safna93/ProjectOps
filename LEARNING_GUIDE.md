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

## Stage 14 - Configuration and Environments

### What is configuration?

Configuration is information an application reads while it starts or runs.
Examples include API addresses, database connection strings, logging levels,
and JWT settings. Keeping these values outside application code makes it
possible to use the same code in different environments.

### Why avoid hard-coded API URLs?

A hard-coded URL means the API address is written directly in a page or C#
file. That makes it harder to switch from a developer's computer to a test or
production environment. ProjectOps now reads the API address from
configuration, so the pages can use relative paths such as `api/projects`.

### IConfiguration and appsettings files

`IConfiguration` is the ASP.NET Core service used to read configuration
values. In ProjectOps, `Program.cs` reads `ApiSettings:BaseUrl` and uses it to
configure `HttpClient`.

`appsettings.json` contains general settings. ASP.NET Core also loads an
environment-specific file, such as `appsettings.Development.json` or
`appsettings.Production.json`. Values in an environment-specific file override
the same values from `appsettings.json`.

The Development API address is stored in
`ProjectOps.Web/appsettings.Development.json`:

```json
{
	"ApiSettings": {
		"BaseUrl": "http://localhost:5103/"
	}
}
```

### ASPNETCORE_ENVIRONMENT

`ASPNETCORE_ENVIRONMENT` tells ASP.NET Core which environment the application
is running in. For example, when its value is `Development`, ASP.NET Core
loads `appsettings.Development.json`. If the value is `Production`, it loads
`appsettings.Production.json` instead.

### Development, UAT/Staging, and Production

- **Development** is where developers run and test the application on their
	computers.
- **UAT** (User Acceptance Testing), often called **Staging**, is a test
	environment where people check a release before production.
- **Production** is the live environment used by real users.

Each environment can provide its own API base URL. Production URLs are not
invented or included as examples here; configure the real address when it is
known, for example through the `ApiSettings__BaseUrl` environment variable.

```text
Development:
Blazor -> http://localhost:5103

UAT:
Blazor -> UAT API URL

Production:
Blazor -> Production API URL

Same application code.
Different configuration.
```

### HttpClient BaseAddress and relative URLs

`HttpClient.BaseAddress` is the starting address used for requests. ProjectOps
sets it once in `ProjectOps.Web/Program.cs` from `ApiSettings:BaseUrl`.
Components then use relative URLs, such as `api/Auth/login` and `api/projects`.
The HTTP client combines the relative path with its configured base address.

### API configuration and secrets

The API already reads its SQL Server connection string, JWT settings, and
logging levels from configuration. These are configuration concerns; this
stage does not restructure the API or change how JWT or the database works.

Configuration is not automatically secret. `appsettings.json` and other
settings files may be committed to Git. Production secrets, such as database
passwords, JWT signing keys, and API keys, should not normally be committed to
source control. Use environment variables or a secure secret store to provide
those values in a real deployment. ProjectOps does not add a secret store in
this stage.

### Stage 14 files

Modified:

- `ProjectOps.Web/Program.cs`
- `ProjectOps.Web/appsettings.Development.json`
- `ProjectOps.Web/Components/Pages/Login.razor`
- `ProjectOps.Web/Components/Pages/Projects.razor`
- `LEARNING_GUIDE.md`

## Stage 12 - Blazor Login and JWT Integration

### Why does Blazor need a login page?

Swagger was useful for testing the API manually, but a real frontend must
perform the login itself. The Blazor login page sends the username and
password to the API and receives the JWT that it needs for protected requests.

### LoginRequest and login response

The Blazor app sends a `LoginRequest` containing `Username` and `Password` to
`POST http://localhost:5103/api/Auth/login`.

The API returns a login response containing `Token` and `ExpiresAt`. The token
is the JWT that the frontend sends with later Projects API requests.

### What is AuthService?

`AuthService` is a small frontend service that stores the current JWT in the
browser's `sessionStorage`. It provides asynchronous operations to store the
token, read the token, check whether a token exists, and log out through
JavaScript interop.

The service is registered with scoped lifetime for the Interactive Server
Blazor application. JavaScript interop lets it store the token in the current
browser tab's `sessionStorage`; it is not stored in cookies or a database.

### Why use sessionStorage?

`sessionStorage` is browser-session storage. The token remains available while
the user navigates between Blazor pages and survives a page refresh in the same
browser tab. Closing the tab or browser session removes the token, so the user
must log in again.

This is appropriate for this learning project because it keeps the example
simple while allowing the token to remain available during navigation.
Production authentication and token storage require a deliberate security
design, including careful decisions about token lifetime, storage, and
protection against token theft.

### How is the bearer token sent?

Before each protected Projects API request, the Blazor page retrieves the JWT
from `sessionStorage`, creates an `HttpRequestMessage`, and adds this header:

```text
Authorization: Bearer <token>
```

The page uses a separate request message for GET, POST, PUT, and DELETE. This
avoids permanently changing `HttpClient.DefaultRequestHeaders` or adding
duplicate authorization values.

### Frontend authentication vs API authentication

The Blazor frontend controls its page experience. It redirects to `/login`
when there is no token and provides a Logout button that clears the token from
`sessionStorage`.

The API is the real security boundary. `[Authorize]` remains on
`ProjectsController`, and the API validates the JWT on every protected request.
Frontend checks improve the user experience, but they cannot replace API
authentication because clients can bypass the UI.

### Login flow

1. The user opens `/login` and enters a username and password.
2. Blazor sends the credentials with `PostAsJsonAsync()`.
3. Invalid credentials display `Invalid username or password.`.
4. Valid credentials return a JWT and expiration time.
5. `AuthService` stores the JWT in browser `sessionStorage`.
6. Blazor navigates to `/projects`.

### Logout flow

When the user selects Logout, `AuthService` removes the JWT from
`sessionStorage` and Blazor navigates to `/login`. Protected project requests
cannot succeed until the user logs in again and receives a new token.

### Complete Stage 12 request flow

```text
Login page
	↓
POST /api/Auth/login
	↓
AuthController
	↓
JWT returned
	↓
AuthService stores token in sessionStorage
	↓
Projects page
	↓
Authorization: Bearer <token>
	↓
Projects API
	↓
[Authorize] validates the token
	↓
ProjectsController
	↓
EF Core and SQL Server
```

### Stage 12 testing steps

1. Start `ProjectOps.Api` and `ProjectOps.Web`.
2. Open the Blazor `/projects` page without logging in. It redirects to
	 `/login`.
3. Enter invalid credentials. The page displays `Invalid username or
	 password.`.
4. Enter the valid learning credentials. The login request succeeds and the
	 app navigates to `/projects`.
5. Confirm the existing projects are displayed.
6. Create a project, edit a project, and delete a project while logged in.
7. Select Logout. The token is cleared and the app returns to `/login`.
8. Log in again before using the protected Projects API.

### Production note

This sessionStorage token service is only for learning. Production applications
need a deliberate and more robust token or session strategy, secure storage,
token expiration handling, and protection against exposing credentials or
tokens.

### Stage 12 files

Created:

- `ProjectOps.Web/Models/LoginRequest.cs`
- `ProjectOps.Web/Models/LoginResponse.cs`
- `ProjectOps.Web/Services/AuthService.cs`
- `ProjectOps.Web/Components/Pages/Login.razor`

Modified:

- `ProjectOps.Web/Program.cs`
- `ProjectOps.Web/Components/Pages/Projects.razor`
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

## Stage 8 - Model Validation

Validation checks whether input is acceptable before the application saves it.
ProjectOps uses Data Annotations on the `Project` model. `[Required]` means a
value must be provided, and `[StringLength]` limits how long a text value can
be.

The API model validates requests on POST and PUT. Because the controller uses
`[ApiController]`, ASP.NET Core automatically returns `400 Bad Request` with
validation information when a request is invalid.

The Blazor Projects form uses `<DataAnnotationsValidator />` to apply the same
rules before sending a request. Each `<ValidationMessage>` displays the error
for its related field. Frontend validation improves the user experience, but
backend validation is still required because API clients can bypass the UI.

### Stage 8 files

Modified:

- `ProjectOps.Api/Models/Project.cs`
- `ProjectOps.Web/Models/Project.cs`
- `ProjectOps.Web/Components/Pages/Projects.razor`
- `LEARNING_GUIDE.md`

## Stage 9 - Global Exception Handling

### What is an exception?

An exception is a problem that happens while the application is running. For
example, a database operation might fail or code might try to use something
that is not available.

### Expected errors and unexpected erors

An expected error is a normal situation that the application can handle. For
example, requesting a project that does not exist can return `404 Not Found`,
and sending invalid data can return `400 Bad Request`.

An unexpected error is a problem that the application did not expect. It
should be handled safely without exposing technical details to the user.

### 400 Bad Request and 500 Internal Server Error

`400 Bad Request` means the client sent a request that the API cannot accept.
For example, a request can fail validation because a required field is empty.

`500 Internal Server Error` means the server encountered an unexpected problem
while processing an otherwise valid request.

### What is global exception handling?

Global exception handling is one central place that handles unexpected errors
for the whole API. ProjectOps uses `GlobalExceptionHandler` instead of putting
repetitive `try`/`catch` blocks in every controller action.

### What is middleware?

Middleware is code in the ASP.NET Core request pipeline. Each middleware can
inspect or change a request and response. The exception-handling middleware
wraps the rest of the pipeline so it can handle exceptions thrown by
controllers or database operations.

### What does IExceptionHandler do?

`IExceptionHandler` is an ASP.NET Core interface for handling exceptions in a
centralized way. `GlobalExceptionHandler` implements it, changes the response
status to `500`, and writes a safe error response.

### What is ProblemDetails?

`ProblemDetails` is a standard JSON format for describing an HTTP API error.
It gives the client a predictable response without returning the server's
stack trace or private implementation details.

### Why centralize exception handling?

Centralizing exception handling keeps controller actions simple and makes the
API's unexpected-error response consistent. It also avoids copying the same
`try`/`catch` code into every GET, POST, PUT, and DELETE action.

### Why not expose stack traces?

Stack traces and exception messages can reveal database names, file paths,
configuration, or other internal details. Those details help developers
diagnose problems but should not be sent to API users. The server can keep
technical details in its internal diagnostics while the client receives a
simple message:

```text
An unexpected error occurred while processing the request.
```

### Exception flow in ProjectOps

```text
Request
	↓
Controller
	↓
Service/EF Core operation
	↓
Unexpected Exception
	↓
Exception Handling Middleware
	↓
GlobalExceptionHandler
	↓
ProblemDetails
	↓
500 Internal Server Error
```

The exception-handling middleware is registered early in the pipeline. This
allows it to observe exceptions from the controller and the operations that
the controller calls. Validation errors still use the normal ASP.NET Core
`400 Bad Request` behavior; the global handler is for unexpected exceptions.

### Stage 9 files

Created:

- `ProjectOps.Api/Exceptions/GlobalExceptionHandler.cs`

Modified:

- `ProjectOps.Api/Program.cs`
- `ProjectOps.Api/Controllers/ProjectsController.cs` (temporary test route was
	added for verification and then removed)
- `LEARNING_GUIDE.md`

## Stage 10 - Structured Logging

### What is logging?

Logging means writing useful information about the application while it runs.
Logs help developers understand what the application is doing and diagnose
problems without showing technical details to users.

### What is ILogger<T>?

`ILogger<T>` is the built-in .NET logging service. The `T` identifies the class
that is writing the log, such as `ILogger<ProjectsController>` or
`ILogger<GlobalExceptionHandler>`.

### How is ILogger injected?

ASP.NET Core provides `ILogger<T>` through dependency injection. The
constructor of `ProjectsController` receives `ILogger<ProjectsController>`,
and `GlobalExceptionHandler` receives `ILogger<GlobalExceptionHandler>`.
Neither class needs to create a logger manually.

### Logging methods

- `LogInformation` records an important normal operation, such as a project
	being created.
- `LogWarning` records an unusual but expected situation, such as a requested
	project not being found.
- `LogError` records an unexpected failure and can include the exception.

### Standard log levels

- `Trace`: very detailed diagnostic information.
- `Debug`: information useful while debugging.
- `Information`: normal important application operations.
- `Warning`: an unusual situation that does not stop the application.
- `Error`: an operation failed or an unexpected exception occurred.
- `Critical`: a serious failure that may stop the application.

ProjectOps uses `Information` for successful CRUD operations, `Warning` when a
project is not found, and `Error` for unexpected exceptions.

### What is structured logging?

Structured logging stores values separately from the message text. For
example:

```csharp
_logger.LogInformation(
		"Project created successfully. ProjectId: {ProjectId}, ProjectCode: {ProjectCode}",
		project.Id,
		project.ProjectCode);
```

`{ProjectId}` and `{ProjectCode}` are named placeholders. Logging systems can
search and filter those values more easily than text built by string
concatenation.

### What should not be logged?

Do not log passwords, tokens, connection strings, private personal data, or
other sensitive information. Logs can be stored and viewed by developers or
operations staff, so they must be treated carefully.

### What developers see and users see

When an unexpected exception occurs, developers see the actual exception and
stack trace in the application logs. The client receives only a safe
`ProblemDetails` response with a general message and HTTP `500` status. This
keeps useful diagnostic information on the server without exposing internal
details to users.

### Exception logging flow

```text
Unexpected exception
	↓
GlobalExceptionHandler
	↓
ILogger logs actual exception for developers
	↓
ProblemDetails returns safe message to client
	↓
HTTP 500
```

When running locally, these logs appear in the terminal or Debug output where
`dotnet run` or the VS Code debugger is running.

### Stage 10 files

Modified:

- `ProjectOps.Api/Controllers/ProjectsController.cs`
- `ProjectOps.Api/Exceptions/GlobalExceptionHandler.cs`
- `LEARNING_GUIDE.md`

## Stage 11 - JWT Authentication and Authorization

### What is authentication?

Authentication means checking who someone is. In ProjectOps, the API checks a
username and password during login.

### What is authorization?

Authorization means checking what an authenticated user is allowed to access.
After login, ProjectOps uses `[Authorize]` to require a valid JWT before the
Projects API can be used.

### Authentication vs authorization

Authentication answers: "Who are you?"

Authorization answers: "Are you allowed to access this resource?"

The login endpoint performs authentication. The `[Authorize]` attribute and
JWT middleware enforce authorization for protected endpoints.

### What is JWT?

JWT means JSON Web Token. It is a signed text token that an API can give to a
client after successful login. The client sends the token with later requests,
so the API can validate the request without asking the client for its password
again.

### JWT structure

A JWT has three parts separated by periods:

```text
Header.Payload.Signature
```

- The **Header** describes the token type and signing algorithm.
- The **Payload** contains claims, such as the username, issuer, audience, and
	expiration time.
- The **Signature** helps prove that the token was created by the API and was
	not changed.

### JWT terms

- A **claim** is a small piece of information inside the token, such as a
	username.
- The **issuer** identifies the application that created the token.
- The **audience** identifies the application or API the token is intended for.
- The **expiry** is the time after which the token is no longer accepted.
- The **signing key** is a secret value used to create and validate the token.

### HMAC SHA256 signing

ProjectOps signs tokens with HMAC SHA256. HMAC uses a shared secret key, and
SHA256 is the hashing algorithm. The API uses the same secret key to create and
validate the signature. If the token is changed or signed with another key,
validation fails.

### POST /api/Auth/login

`POST /api/Auth/login` accepts a JSON body containing `Username` and
`Password`. For this learning stage, the controller checks one hard-coded demo
username and password.

If the credentials are valid, the API generates a JWT and returns it with
HTTP `200 OK`. If they are invalid, the API returns `401 Unauthorized`.

The hard-coded username and password are only for learning. This is not a
production authentication system and it does not use ASP.NET Core Identity or
a users database.

### How the JWT is generated

After valid credentials are received, `AuthController` creates a username/name
claim and adds the configured issuer, audience, and expiration to the token.
It then signs the token with the configured HMAC SHA256 signing key and returns
the token to the client.

### AddAuthentication() and AddJwtBearer()

`AddAuthentication()` registers authentication services and identifies JWT
Bearer as the default authentication scheme.

`AddJwtBearer()` configures how ASP.NET Core reads and validates a token sent in
the HTTP `Authorization` header.

### TokenValidationParameters

`TokenValidationParameters` tells JWT Bearer authentication what to check. In
ProjectOps, it validates:

- The signing key.
- The issuer.
- The audience.
- The token lifetime and expiration.

If one of these checks fails, the request is not authenticated.

### Authentication middleware and authorization middleware

`app.UseAuthentication()` reads the bearer token and tries to identify the
caller.

`app.UseAuthorization()` checks whether that authenticated caller is allowed
to access the requested endpoint.

Authentication must come before authorization. Authorization needs the user
identity created by authentication. If authorization runs first, it cannot
properly determine whether the request has a valid user identity.

### [Authorize] on ProjectsController

`[Authorize]` on `ProjectsController` protects the GET, POST, PUT, and DELETE
project endpoints. Requests without a valid JWT receive `401 Unauthorized`.
Requests with a valid JWT can continue to the controller and database.

### Complete JWT flow

```text
Username + Password
	↓
POST /api/Auth/login
	↓
Validate credentials
	↓
Generate JWT
	↓
Client sends JWT
	↓
Authentication middleware validates JWT
	↓
[Authorize]
	↓
ProjectsController
	↓
Database
```

### Swagger tests

The JWT flow was tested through Swagger:

1. `GET /api/Projects` without a JWT returned `401 Unauthorized`.
2. Login with invalid credentials returned `401 Unauthorized`.
3. Login with valid demo credentials returned `200 OK` and generated a JWT.
4. The token was entered in Swagger's **Authorize** dialog. Only the token was
	 entered because Swagger adds the `Bearer` prefix automatically.
5. `GET /api/Projects` with the valid JWT returned `200 OK` and project data.

### Protecting secrets

The current username and password are hard-coded only for learning purposes.
The JWT signing key in `appsettings.json` is also a development-only demo key.
Real passwords, JWT signing keys, and other production secrets must not be
committed to Git or source control. Production applications should use secure
secret and configuration storage, such as environment variables, user secrets,
or a managed secret store.

### Stage 11 files

Created:

- `ProjectOps.Api/Models/LoginRequest.cs`
- `ProjectOps.Api/Controllers/AuthController.cs`

Modified:

- `ProjectOps.Api/ProjectOps.Api.csproj`
- `ProjectOps.Api/appsettings.json`
- `ProjectOps.Api/Program.cs`
- `ProjectOps.Api/Controllers/ProjectsController.cs`
- `LEARNING_GUIDE.md`
