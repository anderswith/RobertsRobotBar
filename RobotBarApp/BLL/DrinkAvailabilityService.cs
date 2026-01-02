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
        if (eventId == Guid.Empty)
            throw new ArgumentException("Event ID cannot be empty.");

        var barSetups = _barSetupRepo.GetBarSetupForEvent(eventId);

        if (!barSetups.Any())
            return Enumerable.Empty<Drink>();

        var barPositions = barSetups.ToDictionary(
            bs => bs.IngredientId,
            bs => bs.PositionNumber);

        var drinks = _drinkRepo.GetAllDrinksWithContentAndIngredientPositions();

        return drinks
            .Where(drink =>
                drink.DrinkContents.All(dc =>
                {
                    if (dc.Ingredient == null)
                        throw new InvalidOperationException(
                            $"Ingredient missing for drink {drink.DrinkId}");

                    if (!barPositions.TryGetValue(dc.IngredientId, out var barPosition))
                        return false;

                    return dc.Ingredient.IngredientPositions
                        .Any(ip => ip.Position == barPosition);
                }))
            .ToList();
    }

}