using RobotBarApp.BE;
using RobotBarApp.DAL.Repositories.Interfaces;

namespace RobotBarApp.DAL.Repositories;

public class SopRepository : ISopRepository
{
    private readonly RobotBarContext _context;
    
    public SopRepository(RobotBarContext context)
    {
        _context = context;
    }
    
    public void AddSop(Sop sop)
    {
        _context.Sops.Add(sop);
        _context.SaveChanges();
    }

    public IEnumerable<Sop> GetAllSops()
    {
        return _context.Sops.ToList();
    }

    public Sop? GetSopById(Guid sopId)
    {
        return _context.Sops.FirstOrDefault(c => c.SopId == sopId);
    }


    public void DeleteSop(Sop sop)
    {
        _context.Sops.Remove(sop);
        _context.SaveChanges();
        //delete should also delete steps
    }

    public void UpdateSop(Sop sop)
    {
        _context.Sops.Update(sop);
        _context.SaveChanges();
    }
}