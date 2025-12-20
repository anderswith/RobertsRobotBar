using RobotBarApp.BE;

namespace RobotBarApp.DAL.Repositories.Interfaces;

public interface IEventRepository
{
    void AddEvent(Event evt);
    IEnumerable<Event> GetAllEvents();
    Event? GetEventById(Guid eventId);
    void DeleteEvent(Event evt);
    void UpdateEvent(Event evt);
    Guid GetEventIdByDrinkId(Guid drinkId);
}