using System.ComponentModel.DataAnnotations;

namespace RobotBarApp.BE;

public class BarSetup
{
    [Key]
    public Guid EventBarSetupId { get; set; }
    public int PositionNumber { get; set; }
    public Guid IngredientId { get; set; }
    public Guid EventId { get; set; }

    public Ingredient Ingredient { get; set; }
    public Event Event { get; set; }
}