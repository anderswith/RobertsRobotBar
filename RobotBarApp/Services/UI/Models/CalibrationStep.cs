namespace RobotBarApp.BE;

public class CalibrationStep
{
    public int Number { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    
    /// Image resource path (recommended) or file path.
    /// Example: /RobotBarApp;component/Resources/Calibration/step01.png
    public string? ImageSource { get; set; }
    
    /// Optional robot scripts to queue for this step.
    /// Can be URScript text or file names depending on your RobotScriptRunner setup.
    public List<string> ScriptsToRun { get; set; } = new();

    public bool HasScripts => ScriptsToRun.Count > 0;
}

