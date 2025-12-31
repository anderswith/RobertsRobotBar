using System.Windows.Media;
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
    public void AddIngredient(string name, string type, string image, string color, int positionNumber, List<string> singleScriptNames, List<string> doubleScriptNames)
    {
        
        IngredientValidation(name, type, image, color, positionNumber, singleScriptNames, doubleScriptNames);

        Ingredient ingredient = new Ingredient
        {
            IngredientId = Guid.NewGuid(),
            Name = name,
            Type = type,
            Image = image,
            Color = color,
            IngredientPositions = new List<IngredientPosition>
            {
                new IngredientPosition
                {
                    IngredientPositionId = Guid.NewGuid(),
                    Position = positionNumber
                } 
            },
            SingleScripts = new List<SingleScript>(),
            DoubleScripts = new List<DoubleScript>()
        };
        
        int singleNumber = 1;
        foreach (var singletName in singleScriptNames)
        {
            ingredient.SingleScripts.Add(new SingleScript
            {
                ScriptId = Guid.NewGuid(),
                UrScript = singletName,
                Number = singleNumber++,
                IngredientId = ingredient.IngredientId
            });
        }
        
        int doubleNumber = 1;
        foreach (var doubleName in doubleScriptNames)
        {
            ingredient.DoubleScripts.Add(new DoubleScript
            {
                ScriptId = Guid.NewGuid(),
                UrScript = doubleName,
                Number = doubleNumber++,
                IngredientId = ingredient.IngredientId
            });
        }
        _ingredientRepository.AddIngredient(ingredient);
    }

    private static void IngredientValidation(string name, string type, string image, string color, int positionNumber, 
        List<string> singleScriptNames, List<string> doubleScriptNames)
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

        if (string.IsNullOrEmpty(color))
        {
            throw new AggregateException("Ingredient color cannot be null or empty.");
        }
        
        if (singleScriptNames == null || singleScriptNames.Count == 0)
        {
            throw new ArgumentException("Ingredient must have at least one single script.");
        }

        if (singleScriptNames.Any(s => string.IsNullOrWhiteSpace(s)))
        {
            throw new ArgumentException("Script name cannot be null or whitespace.");
        }
        if (doubleScriptNames == null || doubleScriptNames.Count == 0)
        {
            throw new ArgumentException("Ingredient must have atleast one double script.");
        }

        if (doubleScriptNames.Any(s => string.IsNullOrWhiteSpace(s)))
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
    public void UpdateIngredient(Guid ingredientId, string name, string type, string image, string color, 
        int positionNumber, List<string> singleScriptNames, List<string> doubleScriptNames) 
    {
        if(Guid.Empty == ingredientId)
        {
            throw new ArgumentException("Ingredient ID cannot be empty.");
        }
        IngredientValidation(name, type, image, color, positionNumber, singleScriptNames, doubleScriptNames);
       

        var existingIngredient = _ingredientRepository.GetIngredientById(ingredientId);
        if (existingIngredient == null)
        {
            throw new ArgumentException("Ingredient not found.");
        }
        
        existingIngredient.Name = name;
        existingIngredient.Type = type;
        existingIngredient.Image = image;
        existingIngredient.Color = color;
        
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
        
        
        
        // single script
        var singleScript = existingIngredient.SingleScripts.FirstOrDefault();

        if (singleScript == null)
        {
            existingIngredient.SingleScripts.Add(new SingleScript
            {
                ScriptId = Guid.NewGuid(),
                IngredientId = existingIngredient.IngredientId,
                UrScript = singleScriptNames.First(),
                Number = 1
            });
        }
        else
        {
            singleScript.UrScript = singleScriptNames.First();
        }
        
        // doubel script
        var doubleScript = existingIngredient.DoubleScripts.FirstOrDefault();
        if (doubleScript == null)
        {
            existingIngredient.DoubleScripts.Add(new DoubleScript
            {
                ScriptId = Guid.NewGuid(),
                IngredientId = existingIngredient.IngredientId,
                UrScript = doubleScriptNames.First(),
                Number = 1
            });
        }
        else
        {
            doubleScript.UrScript = doubleScriptNames.First();
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
    public IEnumerable<Ingredient> getMockohol(Guid eventId)
    {
        return _ingredientRepository.GetIngredientByType("Mock", eventId);
    }

    public IEnumerable<Ingredient> GetIngredientsForPositions()
    {
        return _ingredientRepository.GetIngredientsForPositions();
    }
}