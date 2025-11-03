using System.ComponentModel.DataAnnotations;

namespace RobotBarApp.BE;
public class Event
{
    [Key]
    public Guid EventId { get; set; }
    public string Name { get; set; }
    public Guid MenuId { get; set; }
    public string Image { get; set; }
    public Guid EventBarSetupId { get; set; }
    
    public Menu Menu { get; set; }
    public ICollection<EventBarSetup> EventBarSetups { get; set; }
}