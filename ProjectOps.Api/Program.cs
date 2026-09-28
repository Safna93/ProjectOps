using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Polly;
using ProjectOps.Api.Data;
using ProjectOps.Api.Exceptions;
using ProjectOps.Api.Middleware;
using ProjectOps.Api.Models;
using ProjectOps.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
var jwtSettings = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSettings["Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException(
        "JWT signing key configuration is missing. Configure Jwt:Key using .NET User Secrets in Development or a secure secret provider in Production.");
}

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
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();
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

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseExceptionHandler();

if (!app.Environment.IsEnvironment("Testing"))
{
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

        if (app.Environment.IsDevelopment())
        {
            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<AppUser>>();
            var demoUsers = new[]
            {
                (Username: "admin", PasswordKey: "DemoUsers:AdminPassword", Role: "Admin"),
                (Username: "user", PasswordKey: "DemoUsers:UserPassword", Role: "User")
            };

            foreach (var demoUser in demoUsers)
            {
                if (dbContext.AppUsers.Any(user => user.Username == demoUser.Username))
                {
                    continue;
                }

                var password = builder.Configuration[demoUser.PasswordKey];
                if (string.IsNullOrWhiteSpace(password))
                {
                    throw new InvalidOperationException(
                        $"Development seed password '{demoUser.PasswordKey}' is missing. Set it with dotnet user-secrets before starting the API.");
                }

                var user = new AppUser
                {
                    Username = demoUser.Username,
                    Role = demoUser.Role
                };
                user.PasswordHash = passwordHasher.HashPassword(user, password);
                dbContext.AppUsers.Add(user);
            }

            dbContext.SaveChanges();
        }
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

public partial class Program { }
