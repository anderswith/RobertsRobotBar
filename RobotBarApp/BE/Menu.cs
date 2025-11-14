using System.ComponentModel.DataAnnotations;

namespace RobotBarApp.BE;

public class Menu
{
    [Key]
    public Guid MenuId { get; set; }
    public string Name { get; set; }
    
    public ICollection<MenuContent> MenuContents { get; set; }
    public ICollection<Event> Events { get; set; }
}