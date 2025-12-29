using System.ComponentModel.DataAnnotations;

namespace RobotBarApp.BE;

public class Ingredient
{
    [Key]
    public Guid IngredientId { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public string Image { get; set; }
    public string Color { get; set; }
    
    public ICollection<DoubleScript> DoubleScripts { get; set; }

    public ICollection<IngredientPosition> IngredientPositions { get; set; }
    public ICollection<DrinkContent> DrinkContents { get; set; }
    public ICollection<BarSetup> BarSetups { get; set; }
    public ICollection<SingleScript> SingleScripts { get; set; }
    
    public ICollection<IngredientUseCount> IngredientUseCounts { get; set; }
}