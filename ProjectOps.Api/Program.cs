using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Polly;
using ProjectOps.Api.Data;
using ProjectOps.Api.Exceptions;
using ProjectOps.Api.Models;
using ProjectOps.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
var jwtSettings = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSettings["Key"] ?? throw new InvalidOperationException("JWT key is not configured.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter a JWT token as: Bearer {token}"
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
{
    [new OpenApiSecuritySchemeReference("Bearer", document)] = []
});
});
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddHttpClient<IExternalProjectService, ExternalProjectService>(client =>
    {
        var baseUrl = builder.Configuration["ResilienceDemo:BaseUrl"]
            ?? throw new InvalidOperationException("ResilienceDemo:BaseUrl is not configured.");
        client.BaseAddress = new Uri(baseUrl);
        client.Timeout = TimeSpan.FromSeconds(5);
    })
    .AddResilienceHandler("external-project-retry", pipeline =>
    {
        pipeline.AddRetry(new HttpRetryStrategyOptions
        {
            MaxRetryAttempts = 2,
            Delay = TimeSpan.FromMilliseconds(200),
            BackoffType = Polly.DelayBackoffType.Constant
        });
    });
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
