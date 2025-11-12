using System.ComponentModel.DataAnnotations;

namespace RobotBarApp.BE;

public class DrinkUseCount
{
    [Key]
    public Guid UseCountId { get; set; }
    public DateTime TimeStamp { get; set; }
    public Guid DrinkId { get; set; }
    
    public Drink drink { get; set; }
}