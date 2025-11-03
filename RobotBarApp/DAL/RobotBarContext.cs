using System.IO;
using Microsoft.EntityFrameworkCore;
using RobotBarApp.BE;

namespace RobotBarApp.DAL;

public class RobotBarContext : DbContext
{
    
    public DbSet<Ingredient> Ingredients { get; set; }
    public DbSet<Drink> Drinks { get; set; }
    public DbSet<DrinkContent> DrinkContents { get; set; }
    public DbSet<Menu> Menus { get; set; }
    public DbSet<MenuContent> MenuContents { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<BarSetup> BarSetups { get; set; }
    public DbSet<Sop> Sops { get; set; }
    public DbSet<SopStep> SopSteps { get; set; }
    public DbSet<Log> Logs { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var basePath = AppContext.BaseDirectory; // bin/Debug/net9.0/
        var projectRoot = Path.GetFullPath(Path.Combine(basePath, @"..\..\..\.."));
        var dbPath = Path.Combine(projectRoot, "RobotBarApp", "DAL", "Database", "RobotBar.db");

        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

        optionsBuilder.UseSqlite($"Data Source={dbPath}");
        //commands to create database and apply migrations:
        //dotnet ef migrations add InitialCreate (migrate - before create)
        //dotnet ef database update (builds/updates database)
        //dotnet ef database drop  (delete)
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Sop>()
            .HasMany(s => s.SopSteps)
            .WithOne(s => s.Sop)
            .HasForeignKey(s => s.SopId)
            .OnDelete(DeleteBehavior.Cascade); //cascade to delete steps when sop is deleted
    }
    
}