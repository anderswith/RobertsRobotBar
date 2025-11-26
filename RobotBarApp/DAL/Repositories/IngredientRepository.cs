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
        return _context.Ingredients.FirstOrDefault(i => i.IngredientId == ingredientId);
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

    public IEnumerable<Ingredient> GetIngredientByType(string type)
    {
        return _context.Ingredients.
            Where(i => i.Type == type)
            .ToList();
    }
    
    public IEnumerable<Ingredient> GetIngredientsWithScripts(List<Guid> ingredientIds)
    {
        return _context.Ingredients
            .Where(i => ingredientIds.Contains(i.IngredientId))
            .Include(i => i.IngredientScripts)
            .ToList();
    }
    
    
}