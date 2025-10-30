using System.ComponentModel.DataAnnotations;

namespace RobotBarApp.BE;

public class Drink
{
    [Key]
    public Guid DrinkId { get; set; }
    
}