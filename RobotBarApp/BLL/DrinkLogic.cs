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

    public void AddDrink(
        string name,
        string image,
        bool isMocktail,
        ICollection<DrinkContent> contents,
        List<string> scriptNames)
    {
        DrinkValidation(name, image, isMocktail, contents, scriptNames);

        var drink = new Drink
        {
            DrinkId = Guid.NewGuid(),
            Name = name,
            Image = image,
            IsMocktail = isMocktail,
            DrinkContents = new List<DrinkContent>(),
            DrinkScripts = new List<DrinkScript>()
        };

        foreach (var content in contents)
        {
            content.DrinkId = drink.DrinkId;
            content.DrinkContentId = Guid.NewGuid();
            drink.DrinkContents.Add(content);
        }

        int counter = 1;
        foreach (var scriptName in scriptNames)
        {
            drink.DrinkScripts.Add(new DrinkScript
            {
                ScriptId = Guid.NewGuid(),
                DrinkId = drink.DrinkId,
                UrScript = scriptName,
                Number = counter++
            });
        }

        _drinkRepository.AddDrink(drink);
    }

    private static void DrinkValidation(
        string name,
        string image,
        bool isMocktail,
        ICollection<DrinkContent> contents,
        List<string> scriptNames)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Drink name cannot be null or empty.");

        if (string.IsNullOrWhiteSpace(image))
            throw new ArgumentException("Drink image cannot be null or empty.");

        if (contents == null || contents.Count == 0)
            throw new ArgumentException("Drink must have at least one ingredient.");
        if(isMocktail == null)
            throw new ArgumentException("isMocktail must be specified.");

        foreach (var content in contents)
        {
            if (content.IngredientId == Guid.Empty)
                throw new ArgumentException("Invalid ingredient.");

            if (string.IsNullOrWhiteSpace(content.Dose))
                throw new ArgumentException("Dose cannot be null or empty.");

            if (content.Dose != "single" && content.Dose != "double")
                throw new ArgumentException("Dose must be either 'single' or 'double'.");
        }

        if (scriptNames == null || scriptNames.Count == 0)
            throw new ArgumentException("Drink must have at least one script.");

        if (scriptNames.Any(string.IsNullOrWhiteSpace))
            throw new ArgumentException("Script name cannot be null or whitespace.");
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
    ICollection<DrinkContent> contents,
    List<string> scriptNames)
{
    DrinkValidation(name, image, isMocktail, contents, scriptNames);

    var existingDrink = _drinkRepository.GetDrinkById(drinkId)
        ?? throw new KeyNotFoundException("Drink not found.");

    // Update simple properties
    existingDrink.Name = name;
    existingDrink.Image = image;
    existingDrink.IsMocktail = isMocktail;

    // Build lookup of desired contents by IngredientId
    var desiredContents = contents.ToDictionary(c => c.IngredientId);

    // Remove contents that no longer exist
    foreach (var existingContent in existingDrink.DrinkContents.ToList())
    {
        if (!desiredContents.ContainsKey(existingContent.IngredientId))
        {
            _drinkRepository.RemoveDrinkContent(existingContent);
            existingDrink.DrinkContents.Remove(existingContent);
        }
    }

    // Add new contents or update existing ones
    foreach (var desired in desiredContents.Values)
    {
        var existing = existingDrink.DrinkContents
            .FirstOrDefault(dc => dc.IngredientId == desired.IngredientId);

        if (existing == null)
        {
            // New ingredient
            desired.DrinkContentId = Guid.NewGuid();
            desired.DrinkId = existingDrink.DrinkId;
            existingDrink.DrinkContents.Add(desired);
        }
        else
        {
            // Update dose
            existing.Dose = desired.Dose;
        }
    }

    // Replace scripts (simple + predictable strategy)
    existingDrink.DrinkScripts.Clear();

    int counter = 1;
    foreach (var scriptName in scriptNames)
    {
        existingDrink.DrinkScripts.Add(new DrinkScript
        {
            ScriptId = Guid.NewGuid(),
            DrinkId = existingDrink.DrinkId,
            UrScript = scriptName,
            Number = counter++
        });
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