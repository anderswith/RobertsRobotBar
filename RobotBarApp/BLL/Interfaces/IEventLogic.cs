using RobotBarApp.BE;

namespace RobotBarApp.BLL.Interfaces;

public interface IEventLogic
{
    Guid AddEvent(string name, string image, Guid? menuId);
    IEnumerable<Event> GetAllEvents();
    Event? GetEventById(Guid eventId);
    void DeleteEvent(Guid eventId);
    void UpdateEvent(Guid eventId, string name, string image, Guid menuId);
    
}