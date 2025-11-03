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
    public double Dose { get; set; }
    public int PositionNumber { get; set; }
    public string Script { get; set; }
    
    public ICollection<DrinkContent> DrinkContents { get; set; }
    public ICollection<EventBarSetup> EventBarSetups { get; set; }
}