using Microsoft.EntityFrameworkCore;
using ProjectOps.Api.Models;

namespace ProjectOps.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Project> Projects => Set<Project>();
}