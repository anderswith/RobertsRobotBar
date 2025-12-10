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
        // 1. Ingredients available on the bar for this event
        var barIngredientIds = _barSetupRepo
            .GetBarSetupForEvent(eventId)
            .Select(bs => bs.IngredientId)
            .ToList();

        // 2. All drinks with their DrinkContent loaded
        var drinks = _drinkRepo.GetAllDrinksWithContent().ToList();

        // 3. Filter drinks that can be made from bar ingredients
        return drinks
            .Where(drink =>
                drink.DrinkContents.All(dc =>
                    barIngredientIds.Contains(dc.IngredientId)))
            .ToList();
    }
}