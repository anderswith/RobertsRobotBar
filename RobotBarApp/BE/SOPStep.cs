using System.ComponentModel.DataAnnotations;

namespace RobotBarApp.BE;

public class SOPStep
{
    [Key]
    public Guid SOPStepsId { get; set; }
    public Guid SOPId { get; set; }
    public string Description { get; set; }
    public string Image { get; set; }
    public int StepCount { get; set; }
    
    public SOP SOP { get; set; }
}