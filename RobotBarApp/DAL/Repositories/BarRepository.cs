namespace RobotBarApp.DAL.Repositories;

public class BarRepository
{
    private readonly RobotBarContext _context;
    
    public BarRepository(RobotBarContext context)
    {
        _context = context;
    }
}