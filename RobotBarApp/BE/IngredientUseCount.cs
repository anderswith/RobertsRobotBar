using System.ComponentModel.DataAnnotations;

namespace RobotBarApp.BE;

public class IngredientUseCount
{
    [Key]
    public Guid UseCountId { get; set; }
    public DateTime TimeStamp { get; set; }
    public Guid IngredientId { get; set; }
    
    public Ingredient Ingredient { get; set; }
}