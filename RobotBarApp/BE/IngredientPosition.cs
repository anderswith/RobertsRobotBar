using System.ComponentModel.DataAnnotations;

namespace RobotBarApp.BE;

public class IngredientPosition
{
    [Key]
    public Guid IngredientPositionId { get; set; }
    public Guid IngredientId { get; set; }
    public int Position { get; set; }
    
    public Ingredient Ingredient { get; set; }
    
}