using RobotBarApp.BE;
using RobotBarApp.BLL.Interfaces;
using RobotBarApp.DAL.Repositories.Interfaces;

namespace RobotBarApp.BLL;

public class EventLogic : IEventLogic
{
    private readonly IEventRepository _eventRepository;
    public EventLogic(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }
    
    public void AddEvent(string name, string image, Guid menuId)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("Event name cannot be null or empty");
        }
        
        if (string.IsNullOrEmpty(image))
        {
            throw new ArgumentException("Event image URL cannot be null or empty");
        }
        if( menuId == Guid.Empty)
        {
            throw new ArgumentException("Menu ID cannot be empty");
        }

        Event evt = new Event
        {
            EventId = Guid.NewGuid(),
            Name = name,
            MenuId = menuId,
            Image = image
        };

        _eventRepository.AddEvent(evt);
    }
    
    public IEnumerable<Event> GetAllEvents()
    {
        return _eventRepository.GetAllEvents();
    }
    
    public Event? GetEventById(Guid eventId)
    {
        if(eventId == Guid.Empty)
        {
            throw new ArgumentException("Event ID cannot be empty");
        }
        return _eventRepository.GetEventById(eventId);
    }
    
    public void DeleteEvent(Guid eventId)
    {
        if(eventId == Guid.Empty)
        {
            throw new ArgumentException("Event ID cannot be empty");
        }
        Event? evt = _eventRepository.GetEventById(eventId);
        if(evt == null)
        {
            throw new ArgumentException("Event not found");
        }
        _eventRepository.DeleteEvent(evt);
    }
    
    public void UpdateEvent(Guid eventId, string name, string image,  Guid menuId)
    {
        if(eventId == Guid.Empty)
        {
            throw new ArgumentException("Event ID cannot be empty");
        }
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("Event name cannot be null or empty");
        }
        
        if (string.IsNullOrEmpty(image))
        {
            throw new ArgumentException("Event image URL cannot be null or empty");
        }
        if( menuId == Guid.Empty)
        {
            throw new ArgumentException("Menu ID cannot be empty");
        }

        Event? evt = _eventRepository.GetEventById(eventId);
        if(evt == null)
        {
            throw new ArgumentException("Event not found");
        }

        evt.Name = name;
        evt.Image = image;
        evt.MenuId = menuId;

        _eventRepository.UpdateEvent(evt);
    }
}