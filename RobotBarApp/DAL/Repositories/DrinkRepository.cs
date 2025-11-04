namespace RobotBarApp.DAL.Repositories;

public class DrinkRepository
{
    private readonly RobotBarContext _context;
    
    public DrinkRepository(RobotBarContext context)
    {
        _context = context;
    }
}