using RobotBarApp.BE;
using RobotBarApp.DAL.Repositories.Interfaces;

namespace RobotBarApp.DAL.Repositories;

public class BarSetupRepository : IBarSetupRepository
{
    private readonly RobotBarContext _context;
    
    public BarSetupRepository(RobotBarContext context)
    {
        _context = context;
    }
    
    public void addBarSetup(BarSetup barSetup)
    {
        _context.Add(barSetup);
        _context.SaveChanges();
    }
    
    public IEnumerable<BarSetup> GetAllBarSetupsForEventById(Guid eventId)
    {
        return _context.BarSetups
            .Where(setup => setup.EventId == eventId)
            .ToList();
    }
    public void updateBarSetup(BarSetup barSetup)
    {
        _context.BarSetups.Update(barSetup);
        _context.SaveChanges();
    }
    
    public void deleteBarSetup(BarSetup barSetup)
    {
        _context.BarSetups.Remove(barSetup);
        _context.SaveChanges();
    }
    public BarSetup? GetBarSetupEventAndPosition(Guid eventId, int positionNumber)
    {
        return _context.BarSetups
            .FirstOrDefault(b => b.EventId == eventId && b.PositionNumber == positionNumber);
    }
}