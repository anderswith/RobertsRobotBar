using System.ComponentModel.DataAnnotations;

namespace RobotBarApp.BE;

public class Sop
{
    [Key]
    public Guid SopId { get; set; }
    public string Name { get; set; }
    public string Image { get; set; }
    
    public ICollection<SopStep> SopSteps { get; set; }
}