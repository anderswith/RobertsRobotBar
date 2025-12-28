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
    /// Ingredient types in the DB that count towards RestrictedMaxCl.
    /// Note: Syrup is stored as "Syrup" in your DB.
    /// </summary>
    public static readonly string[] RestrictedTypes = { "Alkohol", "Mockohol", "Syrup" };
}
