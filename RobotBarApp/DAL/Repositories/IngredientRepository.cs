using Microsoft.EntityFrameworkCore;
using RobotBarApp.BE;
using RobotBarApp.DAL.Repositories.Interfaces;

namespace RobotBarApp.DAL.Repositories;

public class IngredientRepository : IIngredientRepository
{
    private readonly RobotBarContext _context;
    
    public IngredientRepository(RobotBarContext context)
    {
        _context = context;
    }
    
    public void AddIngredient(Ingredient ingredient)
    {
        _context.Ingredients.Add(ingredient);
        _context.SaveChanges();
    }
    public IEnumerable<Ingredient> GetAllIngredients()
    {
        return _context.Ingredients.ToList();
    }
    public Ingredient? GetIngredientById(Guid ingredientId)
    {
        return _context.Ingredients
            .Include(i => i.IngredientPositions)
            .Include(i => i.SingleScripts)
            .FirstOrDefault(i => i.IngredientId == ingredientId);
    }
    public void DeleteIngredient(Ingredient ingredient)
    {
        _context.Ingredients.Remove(ingredient);
        _context.SaveChanges();
    }
    public void UpdateIngredient(Ingredient ingredient)
    {
        _context.Ingredients.Update(ingredient);
        _context.SaveChanges();
    }

    
    public void Save()
    {
        _context.SaveChanges();
    }

    public IEnumerable<Ingredient> GetIngredientByType(string type, Guid eventId)
    {
        return _context.Ingredients
            .Include(i => i.BarSetups)     
            .Where(i => i.Type == type &&
                        i.BarSetups.Any(bs => bs.EventId == eventId))
            .ToList();
    }
    
    public IEnumerable<Ingredient> GetIngredientsWithScripts(List<Guid> ingredientIds)
    {
        return _context.Ingredients
            .Where(i => ingredientIds.Contains(i.IngredientId))
            .Include(i => i.SingleScripts)
            .Include(x => x.DoubleScripts)
            .ToList();
    }

    public IEnumerable<Ingredient> GetIngredientsForPositions()
    {
        return _context.Ingredients
            .Include(i => i.IngredientPositions)
            .ToList();
    }
    
    
}