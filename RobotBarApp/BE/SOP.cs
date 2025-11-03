using System.ComponentModel.DataAnnotations;

namespace RobotBarApp.BE;

public class SOP
{
    [Key]
    public Guid SOPId { get; set; }
    public string Name { get; set; }
    public string Image { get; set; }
    
    public ICollection<SOPStep> SOPSteps { get; set; }
}