using Microsoft.EntityFrameworkCore;
using ProjectOps.Api.Data;
using ProjectOps.Api.Exceptions;
using ProjectOps.Api.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorClient", policy =>
    {
        policy.WithOrigins("http://localhost:5218", "https://localhost:7162")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseExceptionHandler();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();

    if (!dbContext.Projects.Any())
    {
        dbContext.Projects.AddRange(
            new Project
            {
                ProjectCode = "P001",
                ProjectName = "Offshore Platform Upgrade",
                ClientName = "ABC Energy",
                Status = "In Progress"
            },
            new Project
            {
                ProjectCode = "P002",
                ProjectName = "Refinery Modernization",
                ClientName = "Global Energy",
                Status = "Planning"
            },
            new Project
            {
                ProjectCode = "P003",
                ProjectName = "Plant Maintenance",
                ClientName = "Industrial Corp",
                Status = "Completed"
            });

        dbContext.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("BlazorClient");

app.UseAuthorization();

app.MapControllers();

app.Run();
