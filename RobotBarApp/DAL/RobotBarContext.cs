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
    public DbSet<Sop> Sops { get; set; }
    public DbSet<SopStep> SopSteps { get; set; }
    public DbSet<Log> Logs { get; set; }
    public DbSet<IngredientScript> IngredientScripts { get; set; }
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
        // Each Sop has many SopSteps
        modelBuilder.Entity<Sop>()
            .HasMany(s => s.SopSteps)
            .WithOne(s => s.Sop)
            .HasForeignKey(s => s.SopId)
            .OnDelete(DeleteBehavior.Cascade); //delete all SopSteps if Sop is deleted
        
        // DRINK → DRINKCONTENT (1-to-many)
        modelBuilder.Entity<Drink>()
            .HasMany(d => d.DrinkContents)
            .WithOne(dc => dc.Drink)
            .HasForeignKey(dc => dc.DrinkId)
            .OnDelete(DeleteBehavior.Cascade);

        // INGREDIENT → DRINKCONTENT (1-to-many)
        modelBuilder.Entity<Ingredient>()
            .HasMany(i => i.DrinkContents)
            .WithOne(dc => dc.Ingredient)
            .HasForeignKey(dc => dc.IngredientId)
            .OnDelete(DeleteBehavior.Restrict);

        // DRINK → DRINKSCRIPT (1-to-many)
        modelBuilder.Entity<Drink>()
            .HasMany(d => d.DrinkScripts)
            .WithOne(ds => ds.Drink)
            .HasForeignKey(ds => ds.DrinkId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<Event>()
            .HasOne(e => e.Menu)
            .WithMany(m => m.Events)
            .HasForeignKey(e => e.MenuId)
            .OnDelete(DeleteBehavior.SetNull);
        
        modelBuilder.Entity<Event>().Ignore("MenuId1");
    }
    
}