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
        // 1) Fetch all ingredient use counts for this event
        var uses = _context.IngredientUseCounts
            .Where(u => u.EventId == eventId)
            .ToList();

        if (!uses.Any())
            return (new(), new());

        // 2) Extract the ingredient IDs
        var ingredientIds = uses
            .Select(u => u.IngredientId)
            .Distinct()
            .ToList();

        // 3) Load ingredients matching those IDs
        var ingredients = _context.Ingredients
            .Where(i => ingredientIds.Contains(i.IngredientId))
            .ToList();

        return (ingredients, uses);
    }


}
