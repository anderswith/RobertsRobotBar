namespace RobotBarApp.DAL.Repositories.Interfaces;

public class EventRepository
{
    private readonly RobotBarContext _context;
    
    public EventRepository(RobotBarContext context)
    {
        _context = context;
    }
}