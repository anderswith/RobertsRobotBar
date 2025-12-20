using RobotBarApp.BE;
using RobotBarApp.DAL.Repositories.Interfaces;

namespace RobotBarApp.DAL.Repositories;

public class EventRepository : IEventRepository
{
    private readonly RobotBarContext _context;
    
    public EventRepository(RobotBarContext context)
    {
        _context = context;
    }
    
    public void AddEvent(Event evt)
    {
        _context.Events.Add(evt);
        _context.SaveChanges();
    }
    
    public IEnumerable<Event> GetAllEvents()
    {
        return _context.Events.ToList();
    }
    
    public Event? GetEventById(Guid eventId)
    {
        return _context.Events.FirstOrDefault(e => e.EventId == eventId);
    }
    public void DeleteEvent(Event evt)
    {
        _context.Events.Remove(evt);
        _context.SaveChanges();
    }
    public void UpdateEvent(Event evt)
    {
        _context.Events.Update(evt);
        _context.SaveChanges();
    }
    public Guid GetEventIdByDrinkId(Guid drinkId)
    {
        return _context.MenuContents
            .Where(mc => mc.DrinkId == drinkId)
            .Select(mc => mc.Menu.Event.EventId)
            .Single();

    }
    
    
    
}