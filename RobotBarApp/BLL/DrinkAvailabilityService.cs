using RobotBarApp.BE;
using RobotBarApp.BLL.Interfaces;
using RobotBarApp.DAL.Repositories.Interfaces;

public class DrinkAvailabilityService : IDrinkAvailabilityService
{
    private readonly IDrinkRepository _drinkRepo;
    private readonly IBarSetupRepository _barSetupRepo;

    public DrinkAvailabilityService(
        IDrinkRepository drinkRepo,
        IBarSetupRepository barSetupRepo)
    {
        _drinkRepo = drinkRepo;
        _barSetupRepo = barSetupRepo;
    }

    public IEnumerable<Drink> GetAvailableDrinksForEvent(Guid eventId)
    {
        // 1. Build lookup: IngredientId -> PositionNumber for THIS event
        var barPositions = _barSetupRepo
            .GetBarSetupForEvent(eventId)
            .ToDictionary(
                bs => bs.IngredientId,
                bs => bs.PositionNumber);

        // 2. Load all drinks with full ingredient + position metadata
        var drinks = _drinkRepo
            .GetAllDrinksWithContentAndIngredientPositions();

        // 3. Filter drinks by position compatibility
        return drinks
            .Where(drink =>
                drink.DrinkContents.All(dc =>
                {
                    // Ingredient must exist on the bar
                    if (!barPositions.TryGetValue(dc.IngredientId, out var barPosition))
                        return false;

                    // Ingredient must allow that position
                    return dc.Ingredient.IngredientPositions
                        .Any(ip => ip.Position == barPosition);
                }))
            .ToList();
    }
}