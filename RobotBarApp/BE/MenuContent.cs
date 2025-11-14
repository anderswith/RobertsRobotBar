using System.ComponentModel.DataAnnotations;

namespace RobotBarApp.BE;

public class MenuContent
{
    [Key]
    public Guid MenuContentId { get; set; }
    public Guid MenuId { get; set; }
    public Guid DrinkId { get; set; }
    
    public Menu Menu { get; set; }
    public Drink Drink { get; set; }
}