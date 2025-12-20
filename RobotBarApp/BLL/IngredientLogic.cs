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
        
        IngredientValidation(name, type, image, size, dose, positionNumber, scriptNames);
        dose = dose?.ToLowerInvariant();
        Ingredient ingredient = new Ingredient
        {
            IngredientId = Guid.NewGuid(),
            Name = name,
            Type = type,
            Image = image,
            Size = size,
            Dose = dose,
            IngredientPositions = new List<IngredientPosition>
            {
                new IngredientPosition
                {
                    IngredientPositionId = Guid.NewGuid(),
                    Position = positionNumber
                }
            },
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

    private static void IngredientValidation(string name, string type, string image, double size, string dose, int positionNumber, List<string> scriptNames)
    {
        if(positionNumber <= 0)
        {
            throw new ArgumentException("Ingredient position number cannot be negative.");
        }
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
        var normalizedDose = dose?.ToLowerInvariant();
        if(string.IsNullOrEmpty(normalizedDose) || normalizedDose !="single" && normalizedDose !="double")
        {
            throw new ArgumentException("Ingredient dose has to be single or double.");
        }
        

        if (scriptNames == null || scriptNames.Count == 0)
        {
            throw new ArgumentException("Ingredient must have at least one script.");
        }

        if (scriptNames.Any(s => string.IsNullOrWhiteSpace(s)))
        {
            throw new ArgumentException("Script name cannot be null or whitespace.");
        }
        
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
        if(Guid.Empty == ingredientId)
        {
            throw new ArgumentException("Ingredient ID cannot be empty.");
        }
        IngredientValidation(name, type, image, size, dose, positionNumber, scriptNames);
       
        dose = dose?.ToLowerInvariant();
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
        
        // Position
        var position = existingIngredient.IngredientPositions.FirstOrDefault();

        if (position == null)
        {
            existingIngredient.IngredientPositions.Add(new IngredientPosition
            {
                IngredientPositionId = Guid.NewGuid(),
                IngredientId = existingIngredient.IngredientId,
                Position = positionNumber
            });
        }
        else
        {
            position.Position = positionNumber;
        }
        
        
        
        // Scripts
        var script = existingIngredient.IngredientScripts.FirstOrDefault();

        if (script == null)
        {
            existingIngredient.IngredientScripts.Add(new IngredientScript
            {
                ScriptId = Guid.NewGuid(),
                IngredientId = existingIngredient.IngredientId,
                UrScript = scriptNames.First(),
                Number = 1
            });
        }
        else
        {
            script.UrScript = scriptNames.First();
        }

        
        _ingredientRepository.UpdateIngredient(existingIngredient);
        
    }

    public IEnumerable<Ingredient> GetAlcohol(Guid eventId)
    {
        return _ingredientRepository.GetIngredientByType("Alkohol",eventId);
    }

    public IEnumerable<Ingredient> GetSyrups(Guid eventId)
    {
        return _ingredientRepository.GetIngredientByType("Syrup",eventId);
    }

    public IEnumerable<Ingredient> GetSoda(Guid eventId)
    {
        return _ingredientRepository.GetIngredientByType("Soda", eventId);
    }
    public IEnumerable<Ingredient> GetIngredientsWithScripts(List<Guid> ingredientIds)
    {
        return _ingredientRepository.GetIngredientsWithScripts(ingredientIds);
    }

    public IEnumerable<Ingredient> GetIngredientsForPositions()
    {
        return _ingredientRepository.GetIngredientsForPositions();
    }
}