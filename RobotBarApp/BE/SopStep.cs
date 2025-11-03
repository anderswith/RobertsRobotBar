using System.ComponentModel.DataAnnotations;

namespace RobotBarApp.BE;

public class SopStep
{
    [Key]
    public Guid SopStepsId { get; set; }
    public Guid SopId { get; set; }
    public string Description { get; set; }
    public string Image { get; set; }
    public int StepCount { get; set; }
    
    public Sop Sop { get; set; }
}