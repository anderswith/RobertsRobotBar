using RobotBarApp.BE;
using RobotBarApp.BLL.Interfaces;
using RobotBarApp.DAL.Repositories.Interfaces;

namespace RobotBarApp.BLL;

public class IngredientLogic : IIngredientLogic
{
    private readonly IIngredientRepository _ingredientRepository;
    public IngredientLogic(IIngredientRepository ingredientRepository)
    {
        _ingredientRepository = ingredientRepository;
    }
    public void AddIngredient(string name, string type, string image, double size, string dose, int positionNumber, List<string> scriptNames)
    {
        if(string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("Ingredient name cannot be null or empty.");
        }
        if(string.IsNullOrEmpty(type))
        {
            throw new ArgumentException("Ingredient type cannot be null or empty.");
        }
        
        if(string.IsNullOrEmpty(image))
        {
            throw new ArgumentException("Ingredient image cannot be null or empty.");
        }
        if(size <= 0)
        {
            throw new ArgumentException("Ingredient size cannot be negative.");
        }
        dose = dose?.ToLowerInvariant();
        if(string.IsNullOrEmpty(dose) || dose !="single" && dose !="double")
        {
            throw new ArgumentException("Ingredient dose has to be single or double.");
        }
        if(positionNumber <= 0)
        {
            throw new ArgumentException("Ingredient position number cannot be negative.");
        }

        if (scriptNames == null || scriptNames.Count == 0)
        {
            throw new ArgumentException("Ingredient must have at least one script.");
        }

        if (scriptNames.Any(s => string.IsNullOrWhiteSpace(s)))
        {
            throw new ArgumentException("Script name cannot be null or whitespace.");
        }
        
        Ingredient ingredient = new Ingredient
        {
            IngredientId = Guid.NewGuid(),
            Name = name,
            Type = type,
            Image = image,
            Size = size,
            Dose = dose,
            PositionNumber = positionNumber,
            IngredientScripts = new List<IngredientScript>()
        };
        
        int number = 1;
        foreach (var scriptName in scriptNames)
        {
            ingredient.IngredientScripts.Add(new IngredientScript
            {
                ScriptId = Guid.NewGuid(),
                UrScript = scriptName,
                Number = number++,
                IngredientId = ingredient.IngredientId
            });
        }
        _ingredientRepository.AddIngredient(ingredient);
    }
    public IEnumerable<Ingredient> GetAllIngredients()
    {
        return _ingredientRepository.GetAllIngredients();
    }
    public Ingredient? GetIngredientById(Guid ingredientId)
    {
        return _ingredientRepository.GetIngredientById(ingredientId);
    }
    public void DeleteIngredient(Guid ingredientId)
    {
        
        var ingredient = _ingredientRepository.GetIngredientById(ingredientId);

        if (ingredient == null)
        {
            throw new KeyNotFoundException($"Ingredient with ID {ingredientId} not found.");
        }

        if (ingredient.DrinkContents != null && ingredient.DrinkContents.Any())
        {
            throw new InvalidOperationException("Cannot delete ingredient that is used in a drink.");
        }
        if(ingredient.BarSetups != null && ingredient.BarSetups.Any())
        {
            throw new InvalidOperationException("Cannot delete ingredient that is used in a bar setup.");
        }

        _ingredientRepository.DeleteIngredient(ingredient);
        
    }
    public void UpdateIngredient(Guid ingredientId, string name, string type, string image, double size, string dose, int positionNumber, List<string> scriptNames)
    {
        if(string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("Ingredient name cannot be null or empty.");
        }
        if(string.IsNullOrEmpty(type))
        {
            throw new ArgumentException("Ingredient type cannot be null or empty.");
        }
        if(string.IsNullOrEmpty(image))
        {
            throw new ArgumentException("Ingredient image cannot be null or empty.");
        }
        if(size <= 0)
        {
            throw new ArgumentException("Ingredient size cannot be negative.");
        }
        dose = dose?.ToLowerInvariant();
        if(string.IsNullOrEmpty(dose) || dose !="single" && dose !="double")
        {
            throw new ArgumentException("Ingredient dose has to be single or double.");
        }
        if(positionNumber <= 0)
        {
            throw new ArgumentException("Ingredient position number cannot be negative.");
        }
        if (scriptNames == null || scriptNames.Count == 0)
        {
            throw new ArgumentException("Ingredient must have at least one script.");
        }

        if (scriptNames.Any(s => string.IsNullOrWhiteSpace(s)))
        {
            throw new ArgumentException("Script name cannot be null or whitespace.");
        }

        var existingIngredient = _ingredientRepository.GetIngredientById(ingredientId);
        if (existingIngredient == null)
        {
            throw new ArgumentException("Ingredient not found.");
        }
        
        existingIngredient.Name = name;
        existingIngredient.Type = type;
        existingIngredient.Image = image;
        existingIngredient.Size = size;
        existingIngredient.Dose = dose;
        existingIngredient.PositionNumber = positionNumber;
        
        var currentScripts = existingIngredient.IngredientScripts?.ToList() ?? new List<IngredientScript>();

        // Remove old scripts not in the new list
        var toRemove = currentScripts.Where(s => !scriptNames.Contains(s.UrScript)).ToList();
        foreach (var script in toRemove)
        {
            existingIngredient.IngredientScripts.Remove(script);
        }
        
        // Add new scripts that didn’t exist before
        var toAdd = scriptNames.Where(s => !currentScripts.Select(c => c.UrScript).Contains(s)).ToList();

        int nextNumber = currentScripts.Count > 0 ? currentScripts.Max(s => s.Number) + 1 : 1;
        foreach (var scriptName in toAdd)
        {
            existingIngredient.IngredientScripts.Add(new IngredientScript
            {
                ScriptId = Guid.NewGuid(),
                UrScript = scriptName,
                Number = nextNumber++,
                IngredientId = existingIngredient.IngredientId
            });
        }
        
        _ingredientRepository.UpdateIngredient(existingIngredient);
        
    }
}