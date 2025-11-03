using System.ComponentModel.DataAnnotations;

namespace RobotBarApp.BE;

public class Log
{
    [Key]
    public Guid LogId { get; set; }
    public DateTime TimeStamp { get; set; }
    public string LogMsg { get; set; }
    public string Type { get; set; }
}