namespace RobotBarApp.Settings;

/// <summary>
/// Central place for Mix Selv limit tuning.
/// Keep values in cl.
/// </summary>
public static class MixSelvLimits
{
    /// <summary>
    /// Step size when user presses +/- for normal ingredients.
    /// </summary>
    public const int StepCl = 2;

    /// <summary>
    /// Step size for Soda/Juice ingredients (treated as chunks).
    /// </summary>
    public const int SodaChunkCl = 20;

    /// <summary>
    /// Maximum total capacity of the glass.
    /// </summary>
    public const int GlassMaxCl = 30;

    /// <summary>
    /// Combined max across restricted ingredient types (Alkohol + Mockohol + Syrup).
    /// </summary>
    public const int RestrictedMaxCl = 10;

    /// <summary>
    /// Allowed amount for a single shot (cl).
    /// </summary>
    public const int SingleShotCl = 2;

    /// <summary>
    /// Allowed amount for a double shot (cl).
    /// </summary>
    public const int DoubleShotCl = 4;

    /// <summary>
    /// Combined max for Alkohol + Mock (subset of RestrictedMaxCl).
    /// </summary>
    public const int AlcoholAndMockMaxCl = 4;

    /// <summary>
    /// Ingredient types in the DB that count towards RestrictedMaxCl.
    /// </summary>
    public static readonly string[] RestrictedTypes = { "Alkohol", "Mock", "Syrup" };

    /// <summary>
    /// Ingredient types that count towards AlcoholAndMockMaxCl.
    /// </summary>
    public static readonly string[] AlcoholAndMockTypes = { "Alkohol", "Mock" };
}
