namespace RobotBarApp.Services.Application.Interfaces;

public interface IEventSessionService
{
    Guid? CurrentEventId { get; }
    void StartEvent(Guid eventId);
    void EndEvent();
    bool HasActiveEvent { get; }
}
