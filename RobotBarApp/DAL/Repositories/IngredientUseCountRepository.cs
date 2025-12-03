using Microsoft.EntityFrameworkCore;
using RobotBarApp.BE;
using RobotBarApp.DAL.Repositories.Interfaces;

namespace RobotBarApp.DAL.Repositories;

public class IngredientUseCountRepository : IIngredientUseCountRepository
{
    private readonly RobotBarContext _context;
    public IngredientUseCountRepository(RobotBarContext context)
    {
        _context = context;
    }
    
    public void AddIngredientUseCount(IngredientUseCount ingredientUseCount)
    {
        _context.IngredientUseCounts.Add(ingredientUseCount);
        _context.SaveChanges();
    }
    
    public IEnumerable<IngredientUseCount> GetAllIngredientUseCounts()
    {
        return _context.IngredientUseCounts.ToList();
    }
    
    public IEnumerable<IngredientUseCount> GetAllIngredientUseCountByTimeFrame(DateTime start, DateTime end)
    {
        return _context.IngredientUseCounts
            .Where(iuc => iuc.TimeStamp >= start && iuc.TimeStamp <= end)
            .Include(icu => icu.Ingredient)
            .ToList();
    }

}
