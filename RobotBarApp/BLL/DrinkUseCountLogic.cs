using RobotBarApp.BE;
using RobotBarApp.BLL.Interfaces;
using RobotBarApp.DAL.Repositories.Interfaces;

namespace RobotBarApp.BLL;

public class DrinkUseCountLogic : IDrinkUseCountLogic
{
    private readonly IDrinkUseCountRepository _drinkUseCountRepository;
    public DrinkUseCountLogic(IDrinkUseCountRepository drinkUseCountRepository)
    {
        _drinkUseCountRepository = drinkUseCountRepository;
    }
    public void AddDrinkUseCount(Guid drinkId, Guid eventId)
    {
        var drinkUseCount = new DrinkUseCount
        {
            UseCountId = Guid.NewGuid(),
            TimeStamp = DateTime.Now,
            DrinkId = drinkId,
            EventId = eventId
        };
        _drinkUseCountRepository.AddDrinkUseCount(drinkUseCount);
    }

    public IEnumerable<DrinkUseCount> GetAllDrinkUseCounts()
    {
        return _drinkUseCountRepository.GetAllDrinkUseCounts();
    }


    public IEnumerable<(string DrinkName, int TotalUseCount)> GetAllDrinksUseCountForEvent(Guid eventId)
    {
        if (eventId == Guid.Empty)
            throw new ArgumentException("Event ID must be a valid GUID.");

        // Get drinks + all use counts for this event
        var (drinks, drinkUses) = _drinkUseCountRepository.GetAllDrinksUseCountForEvent(eventId);

        if (drinks == null || !drinks.Any())
            return Enumerable.Empty<(string, int)>();

        // Group all drinkUseCounts by DrinkId
        var stats = drinkUses
            .GroupBy(uc => uc.DrinkId)
            .Select(g =>
            {
                var drinkName = drinks.First(d => d.DrinkId == g.Key).Name;
                return (DrinkName: drinkName, TotalUseCount: g.Count());
            })
            .OrderByDescending(x => x.TotalUseCount)
            .ToList();

        return stats;
    }

    public IEnumerable<(string DrinkName, int TotalUseCount)> GetAllDrinkUseCountByTimeFrame(
        Guid eventId, DateTime start, DateTime end)
    {
        if (eventId == Guid.Empty)
            throw new ArgumentException("Event ID must be a valid GUID.");

        if (start >= end)
            throw new ArgumentException("Start time must be earlier than end time.");

        if (start == default || end == default)
            throw new ArgumentException("Start and end times must be valid dates.");

        // Get full statistics for the event (drinks + use counts)
        var (drinks, drinkUses) = _drinkUseCountRepository.GetAllDrinksUseCountForEvent(eventId);

        // Filter only the DrinkUseCounts in the timeframe
        var filtered = drinkUses
            .Where(uc => uc.TimeStamp >= start && uc.TimeStamp <= end)
            .ToList();

        if (!filtered.Any())
            return Enumerable.Empty<(string, int)>();

        // Group and create statistics output
        var stats = filtered
            .GroupBy(uc => uc.DrinkId)
            .Select(g =>
            {
                var drinkName = drinks.First(d => d.DrinkId == g.Key).Name;
                return (DrinkName: drinkName, TotalUseCount: g.Count());
            })
            .OrderByDescending(x => x.TotalUseCount)
            .ToList();

        return stats;
    }
}