using System.IO;
using System.Runtime.InteropServices.ComTypes;
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
    public DbSet<Log> Logs { get; set; }
    public DbSet<SingleScript> SingleScripts { get; set; }
    public DbSet<DoubleScript> DoubleScripts { get; set; }
    public DbSet<DrinkScript> DrinkScripts { get; set; }
    public DbSet<DrinkUseCount> DrinkUseCounts { get; set; }
    public DbSet<IngredientUseCount> IngredientUseCounts { get; set; }
    public DbSet<IngredientPosition> IngredientPositions { get; set; }
    
    public RobotBarContext(DbContextOptions<RobotBarContext> options)
        : base(options)
    {
        Console.WriteLine($"USING SQLITE DB: {Database.GetDbConnection().DataSource}");
    }
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
        modelBuilder.Entity<Drink>()
            .HasMany(d => d.DrinkContents)
            .WithOne(dc => dc.Drink)
            .HasForeignKey(dc => dc.DrinkId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<Ingredient>()
            .HasMany(i => i.DrinkContents)
            .WithOne(dc => dc.Ingredient)
            .HasForeignKey(dc => dc.IngredientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Drink>()
            .HasMany(d => d.DrinkScripts)
            .WithOne(ds => ds.Drink)
            .HasForeignKey(ds => ds.DrinkId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<Menu>()
            .HasMany(m => m.MenuContents)
            .WithOne(mc => mc.Menu)
            .HasForeignKey(mc => mc.MenuId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<Event>().Ignore("MenuId1");
    }
    
}