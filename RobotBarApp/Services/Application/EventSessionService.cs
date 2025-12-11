using RobotBarApp.Services.Application.Interfaces;

namespace RobotBarApp.Services.Application;

public class EventSessionService : IEventSessionService
{
    public Guid? CurrentEventId { get; private set; }

    public bool HasActiveEvent => CurrentEventId.HasValue;

    public void StartEvent(Guid eventId)
    {
        CurrentEventId = eventId;
    }

    public void EndEvent()
    {
        CurrentEventId = null;
    }
}
