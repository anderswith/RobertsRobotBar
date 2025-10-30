using System.ComponentModel.DataAnnotations;

namespace RobotBarApp.BE;

public class Menu
{
    [Key]
    public Guid MenuId { get; set; }
}