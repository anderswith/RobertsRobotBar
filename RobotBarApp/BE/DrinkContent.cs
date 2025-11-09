using System.ComponentModel.DataAnnotations;

namespace RobotBarApp.BE;
public class DrinkContent
{
    [Key]
    public Guid DrinkContentId { get; set; }
    public Guid IngredientId { get; set; }
    public Guid DrinkId { get; set; }
    
    public Drink Drink { get; set; }
    public Ingredient Ingredient { get; set; }
}