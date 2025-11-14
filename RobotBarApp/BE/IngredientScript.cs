using System.ComponentModel.DataAnnotations;

namespace RobotBarApp.BE;

public class IngredientScript
{
    [Key]
    public Guid ScriptId { get; set; }
    public string UrScript { get; set; }
    public int Number { get; set; }
    public Guid IngredientId { get; set; }
    public int PositionNumber { get; set; }
    
    public Ingredient Ingredient { get; set; }
    
}