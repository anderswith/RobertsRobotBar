using System.ComponentModel.DataAnnotations;

namespace RobotBarApp.BE;

public class Ingredient
{
    [Key]
    public Guid IngredientId { get; set; }
}