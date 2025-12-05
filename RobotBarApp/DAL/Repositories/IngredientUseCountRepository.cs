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
    
    public (List<Ingredient> Ingredients, List<IngredientUseCount> IngredientUses)
        GetIngredientUseCountForEvent(Guid eventId)
    {
        // 1) Get all ingredient IDs that are part of the event setup
        var ingredientIds = _context.BarSetups
            .Where(e => e.EventId == eventId)
            .Select(e => e.IngredientId)
            .Distinct()
            .ToList();

        if (!ingredientIds.Any())
            return (new(), new());

        // 2) Load ingredients
        var ingredients = _context.Ingredients
            .Where(i => ingredientIds.Contains(i.IngredientId))
            .ToList();

        // 3) Load all use counts for these ingredients
        var uses = _context.IngredientUseCounts
            .Where(u => ingredientIds.Contains(u.IngredientId))
            .ToList();

        return (ingredients, uses);
    }

}
