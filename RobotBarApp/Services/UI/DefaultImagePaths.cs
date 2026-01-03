namespace RobotBarApp.Services.UI
{
    /// <summary>
    /// Centralized paths (DB stored) for default/fallback images.
    /// These are expected to exist under RobotBarApp/Resources/Default/.
    /// </summary>
    public static class DefaultImagePaths
    {
        // Stored in DB as relative paths (same style as ImageStorageService returns)
        public const string Ingredient = "Resources/Default/default_ingredient.png";
        public const string Drink = "Resources/Default/default_drink.png";
        public const string Event = "Resources/Default/default_event_png";
    }
}

