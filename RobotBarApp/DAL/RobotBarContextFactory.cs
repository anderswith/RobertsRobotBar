using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.IO;

namespace RobotBarApp.DAL
{
    public class RobotBarContextFactory : IDesignTimeDbContextFactory<RobotBarContext>
    {
        public RobotBarContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<RobotBarContext>();

            var basePath = Directory.GetCurrentDirectory();
            var dbPath = Path.Combine(basePath, "RobotBarApp", "DAL", "Database", "RobotBar.db");

            Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

            optionsBuilder.UseSqlite($"Data Source={dbPath}");

            return new RobotBarContext(optionsBuilder.Options);
            
            
        }
    }
}