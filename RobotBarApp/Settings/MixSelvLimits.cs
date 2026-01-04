namespace RobotBarApp.Settings;
public static class MixSelvLimits
{
    /// Step size when user presses +/- for normal ingredients.
    public const int StepCl = 2;
    
    /// Step size for Soda/Juice ingredients (treated as chunks).
    public const int SodaChunkCl = 20;
    
    /// Maximum total capacity of the glass.
    public const int GlassMaxCl = 30;
    
    /// Combined max across restricted ingredient types (Alkohol + Mockohol + Syrup).
    public const int RestrictedMaxCl = 10;
    
    /// Allowed amount for a single shot (cl).
    public const int SingleShotCl = 2;
    
    /// Allowed amount for a double shot (cl).
    public const int DoubleShotCl = 4;
    
    /// Combined max for Alkohol + Mock (subset of RestrictedMaxCl).
    public const int AlcoholAndMockMaxCl = 4;
    
    /// Ingredient types in the DB that count towards RestrictedMaxCl.
    public static readonly string[] RestrictedTypes = { "Alkohol", "Mock", "Syrup" };
    
    /// Ingredient types that count towards AlcoholAndMockMaxCl.
    public static readonly string[] AlcoholAndMockTypes = { "Alkohol", "Mock" };
}
