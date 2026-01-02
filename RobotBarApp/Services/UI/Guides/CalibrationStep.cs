namespace RobotBarApp.BE;

public class CalibrationStep
{
    public int Number { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";

    /// <summary>
    /// Image resource path (recommended) or file path.
    /// Example: /RobotBarApp;component/Resources/Calibration/step01.png
    /// </summary>
    public string? ImageSource { get; set; }

    /// <summary>
    /// Optional robot scripts to queue for this step.
    /// Can be URScript text or file names depending on your RobotScriptRunner setup.
    /// </summary>
    public List<string> ScriptsToRun { get; set; } = new();

    public bool HasScripts => ScriptsToRun.Count > 0;
}

