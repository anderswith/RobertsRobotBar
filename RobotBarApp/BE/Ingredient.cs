using System.ComponentModel.DataAnnotations;

namespace RobotBarApp.BE;

public class Ingredient
{
    [Key]
    public Guid IngredientId { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public string Image { get; set; }
    public int UseCount { get; set; }
    public double Size { get; set; }
    public string Dose { get; set; }
    
    public ICollection<DrinkContent> DrinkContents { get; set; }
    public ICollection<BarSetup> BarSetups { get; set; }
    public ICollection<IngredientScript> IngredientScripts { get; set; }
}