using RobotBarApp.BE;

namespace RobotBarApp.BLL.Interfaces;

public interface IDrinkAvailabilityService
{
    IEnumerable<Drink> GetAvailableDrinksForEvent(Guid eventId);
}