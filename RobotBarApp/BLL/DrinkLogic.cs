using RobotBarApp.BE;
using RobotBarApp.DAL.Repositories.Interfaces;

namespace RobotBarApp.BLL;

public class DrinkLogic
{
    private readonly IDrinkRepository _drinkRepository;
    public DrinkLogic(IDrinkRepository drinkRepository)
    {
        _drinkRepository = drinkRepository;
    }
    
    public IEnumerable<Drink> GetAllDrinks()
    {
        return _drinkRepository.GetAllDrinks().ToList();
    }
    
    public Drink? GetDrinkById(Guid drinkId)
    {
        if (drinkId == Guid.Empty)
        {
            throw new ArgumentException("Invalid drink ID.");
        }
        return _drinkRepository.GetDrinkById(drinkId);
    }

    public void AddDrink(string name, string image, bool IsMocktail, List<Guid> ingredientIds, List<string> scriptNames)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("Drink name cannot be null or empty.");
        }
        if (string.IsNullOrEmpty(image))
        {
            throw new ArgumentException("Drink image cannot be null or empty.");
        }

        if (IsMocktail == null)
        {
            throw new ArgumentException("Must choose if drink is mocktail or not.");
        }
        if(ingredientIds == null || ingredientIds.Count == 0)
        {
            throw new ArgumentException("Drink must have at least one ingredient.");
        }
        if(scriptNames == null || scriptNames.Count == 0)
        {
            throw new ArgumentException("Drink must have at least one script.");
        }
        if (scriptNames.Any(s => string.IsNullOrWhiteSpace(s)))
        {
            throw new ArgumentException("Script name cannot be null or whitespace.");
        }

        
        Drink drink = new Drink
        {
            DrinkId = Guid.NewGuid(),
            Name = name,
            Image = image,
            IsMocktail = IsMocktail,
            DrinkContents = new List<DrinkContent>(),
            DrinkScripts = new List<DrinkScript>()
        };
        // Add DrinkContents
        foreach (var ingredientId in ingredientIds)
        {
            drink.DrinkContents.Add(new DrinkContent
            {
                DrinkContentId = Guid.NewGuid(),
                DrinkId = drink.DrinkId,
                IngredientId = ingredientId
            });
        }
        
        // Add scripts based on names 
        int counter = 1;
        foreach (var scriptName in scriptNames)
        {
            if (string.IsNullOrWhiteSpace(scriptName))
            {
                throw new ArgumentException("Script name cannot be null or empty.");
            }
            
            drink.DrinkScripts.Add(new DrinkScript
            {
                ScriptId = Guid.NewGuid(),
                UrScript = scriptName,
                Number = counter++,         
                DrinkId = drink.DrinkId
            });
        }
        _drinkRepository.AddDrink(drink);
    }
    public void DeleteDrink(Guid drinkId)
    {
        var drink = _drinkRepository.GetDrinkById(drinkId);
        if (drink == null)
        {
            throw new KeyNotFoundException($"Drink not found.");
        }
        _drinkRepository.DeleteDrink(drink);
    }
    
    public void UpdateDrink(Guid drinkId, string name, string image, bool isMocktail, List<Guid> ingredientIds, List<string> scriptNames)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("Drink name cannot be null or empty.");
        }

        if (string.IsNullOrEmpty(image))
        {
            throw new ArgumentException("Drink image cannot be null or empty.");
        }
            
        if (ingredientIds == null || ingredientIds.Count == 0)
        {
            throw new ArgumentException("Drink must have at least one ingredient.");
        }

        if (scriptNames == null || scriptNames.Count == 0)
        {
            throw new ArgumentException("Drink must have at least one script.");
        }
        if (scriptNames.Any(s => string.IsNullOrWhiteSpace(s)))
        {
            throw new ArgumentException("Script name cannot be null or whitespace.");
        }
        
        var existingDrink = _drinkRepository.GetDrinkById(drinkId);

        if (existingDrink == null)
        {
            throw new KeyNotFoundException($"Drink not found.");
        }

        
        existingDrink.Name = name;
        existingDrink.Image = image;
        existingDrink.IsMocktail = isMocktail;

    // Update Ingredients (DrinkContents)

    var currentIngredientIds = existingDrink.DrinkContents
        .Select(dc => dc.IngredientId)
        .ToList();

    // Remove old ingredients not in the new list
    var toRemoveIngredients = existingDrink.DrinkContents
        .Where(dc => !ingredientIds.Contains(dc.IngredientId))
        .ToList();

    foreach (var dc in toRemoveIngredients)
        existingDrink.DrinkContents.Remove(dc);

    // Add new ingredients
    var toAddIngredients = ingredientIds
        .Where(id => !currentIngredientIds.Contains(id))
        .ToList();

    foreach (var id in toAddIngredients)
    {
        existingDrink.DrinkContents.Add(new DrinkContent
        {
            DrinkContentId = Guid.NewGuid(),
            DrinkId = existingDrink.DrinkId,
            IngredientId = id
        });
    }


    // Update Scripts (DrinkScripts)
    
    var currentScriptNames = existingDrink.DrinkScripts
        .Select(s => s.UrScript)
        .ToList();

    // Remove scripts that no longer exist
    var toRemoveScripts = existingDrink.DrinkScripts
        .Where(s => !scriptNames.Contains(s.UrScript))
        .ToList();

    foreach (var script in toRemoveScripts)
    {
        existingDrink.DrinkScripts.Remove(script);
    }
    
    // Add new scripts
    var toAddScripts = scriptNames
        .Where(name => !currentScriptNames.Contains(name))
        .ToList();

    int nextNumber = 1;

    if (existingDrink.DrinkScripts.Any())
    {
        int highestNumber = existingDrink.DrinkScripts.Max(script => script.Number);
        nextNumber = highestNumber + 1;
    }

    foreach (var scriptName in toAddScripts)
    {
        existingDrink.DrinkScripts.Add(new DrinkScript
        {
            ScriptId = Guid.NewGuid(),
            UrScript = scriptName,
            Number = nextNumber++,
            DrinkId = existingDrink.DrinkId
        });
    }
        
        _drinkRepository.UpdateDrink(existingDrink);
    }

    
}