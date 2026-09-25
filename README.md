# ProjectOps

ProjectOps is a full-stack project management application built using ASP.NET Core Web API, Blazor, Entity Framework Core, and SQL Server.

This project is being developed step-by-step to practice real-world .NET full-stack development concepts.

## Technologies

- C#
- .NET
- ASP.NET Core Web API
- Blazor
- Entity Framework Core
- SQL Server
- REST API
- Swagger / OpenAPI
- Git & GitHub

## Current Features

- View all projects
- Create a new project
- Edit an existing project
- Delete a project
- Blazor frontend integrated with ASP.NET Core Web API
- SQL Server database integration
- Entity Framework Core migrations
- Swagger API testing
- CORS configuration
- Async database operations

## Architecture

Browser  
↓  
Blazor Web App  
↓  
HttpClient  
↓  
ASP.NET Core Web API  
↓  
ProjectsController  
↓  
Entity Framework Core  
↓  
SQL Server (ProjectOpsDb)

## API Endpoints

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/projects` | Get all projects |
| POST | `/api/projects` | Create a project |
| PUT | `/api/projects/{id}` | Update a project |
| DELETE | `/api/projects/{id}` | Delete a project |

## Project Structure

ProjectOps  
├── ProjectOps.Api - ASP.NET Core Web API  
├── ProjectOps.Web - Blazor frontend  
└── ProjectOps.sln - Solution file

## Future Enhancements

The project will continue to be enhanced with:

- JWT Authentication
- Authorization
- Input validation
- Global exception handling
- Logging
- Automated testing
- Azure deployment
- CI/CD

## Learning Goal

The goal of ProjectOps is to understand the complete flow of a .NET application:

**Blazor → Web API → Entity Framework Core → SQL Server**

The project will be continuously upgraded as additional .NET concepts are learned and implemented.