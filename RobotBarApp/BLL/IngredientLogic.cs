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
    public void AddIngredient(string name, string type, string image, double size, double dose)
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
        if(size < 0)
        {
            throw new ArgumentException("Ingredient size cannot be negative.");
        }
        if(dose <= 0)
        {
            throw new ArgumentException("Ingredient dose cannot be negative or 0.");
        }
    
        Ingredient ingredient = new Ingredient
        {
            IngredientId = Guid.NewGuid(),
            Name = name,
            Type = type,
            Image = image,
            UseCount = 0,
            Size = size,
            Dose = dose
        };
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
    public void UpdateIngredient(Guid ingredientId, string name, string type, string image, double size, double dose)
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
        if(size < 0)
        {
            throw new ArgumentException("Ingredient size cannot be negative.");
        }
        if(dose <= 0)
        {
            throw new ArgumentException("Ingredient dose cannot be negative or 0.");
        }

        var existingIngredient = _ingredientRepository.GetIngredientById(ingredientId);
        if (existingIngredient == null)
        {
            existingIngredient = new Ingredient
            {
                IngredientId = Guid.NewGuid(),
                Name = name,
                Type = type,
                Image = image,
                Size = size,
                Dose = dose
            };
            _ingredientRepository.AddIngredient(existingIngredient);
        }
        else
        {
            existingIngredient.Name = name;
            existingIngredient.Type = type;
            existingIngredient.Image = image;
            existingIngredient.Size = size;
            existingIngredient.Dose = dose;
            
            _ingredientRepository.UpdateIngredient(existingIngredient);
        }
        
    }
}