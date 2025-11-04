using System.ComponentModel.DataAnnotations;

namespace RobotBarApp.BE;

public class DrinkScript
{
    [Key]
    public Guid ScriptId { get; set; }
    public string UrScript { get; set; }
    public int Number { get; set; }
    public Guid DrinkId { get; set; }
    
    public Drink Drink { get; set; }
}