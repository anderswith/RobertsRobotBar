using RobotBarApp.BE;
using RobotBarApp.BLL.Interfaces;
using RobotBarApp.DAL.Repositories.Interfaces;

namespace RobotBarApp.BLL;

public class DrinkLogic : IDrinkLogic
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

    public void AddDrink(string name, string image, bool isMocktail, List<Guid> ingredientIds, List<string> scriptNames)
    {
        
        DrinkValidation(name, image, isMocktail, ingredientIds, scriptNames);


        Drink drink = new Drink
        {
            DrinkId = Guid.NewGuid(),
            Name = name,
            Image = image,
            IsMocktail = isMocktail,
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

    private static void DrinkValidation(string name, string image, bool isMocktail, List<Guid> ingredientIds, List<string> scriptNames)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("Drink name cannot be null or empty.");
        }
        if (string.IsNullOrEmpty(image))
        {
            throw new ArgumentException("Drink image cannot be null or empty.");
        }

        if (isMocktail == null)
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
    

    public void UpdateDrink(
        Guid drinkId,
        string name,
        string image,
        bool isMocktail,
        List<Guid> ingredientIds,
        List<string> scriptNames)
    {
        DrinkValidation(name, image, isMocktail, ingredientIds, scriptNames);

        var existingDrink = _drinkRepository.GetDrinkById(drinkId)
                            ?? throw new KeyNotFoundException("Drink not found.");

        existingDrink.Name = name;
        existingDrink.Image = image;
        existingDrink.IsMocktail = isMocktail;

        var desiredIngredientIds = ingredientIds.Distinct().ToHashSet();

        // Remove missing
        foreach (var content in existingDrink.DrinkContents.ToList())
        {
            if (!desiredIngredientIds.Contains(content.IngredientId))
            {
                _drinkRepository.RemoveDrinkContent(content);
                existingDrink.DrinkContents.Remove(content);
            }
        }

        // Add new
        var existingIds = existingDrink.DrinkContents
            .Select(dc => dc.IngredientId)
            .ToHashSet();

        foreach (var ingredientId in desiredIngredientIds)
        {
            if (!existingIds.Contains(ingredientId))
            {
                existingDrink.DrinkContents.Add(new DrinkContent
                {
                    DrinkId = existingDrink.DrinkId,
                    IngredientId = ingredientId
                });
            }
        }

        // Script  
        var existingScript = existingDrink.DrinkScripts.FirstOrDefault();
        var newScript = scriptNames.FirstOrDefault();

        if (string.IsNullOrWhiteSpace(newScript))
        {
            if (existingScript != null)
                existingDrink.DrinkScripts.Remove(existingScript);
        }
        else if (existingScript == null)
        {
            existingDrink.DrinkScripts.Add(new DrinkScript
            {
                ScriptId = Guid.NewGuid(),
                DrinkId = existingDrink.DrinkId,
                UrScript = newScript,
                Number = 1
            });
        }
        else
        {
            existingScript.UrScript = newScript;
        }

        _drinkRepository.UpdateDrink(existingDrink);
    }

    public Drink? GetDrinksWithScripts(Guid drinkId)
    {
        return _drinkRepository.GetDrinkWithScripts(drinkId);
    }
    
    public IEnumerable<Ingredient> GetAvailableIngredientsForDrink(Guid drinkId)
    {
        if(drinkId == Guid.Empty)
        {
            throw new ArgumentException("Invalid drink ID.");
        }
        return _drinkRepository.GetAvailableIngredientsForDrink(drinkId);
    }
    public bool Exists(Guid drinkId)
    {
        if (drinkId == Guid.Empty)
            return false;

        return _drinkRepository.Exists(drinkId);
    }
}